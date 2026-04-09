// S-04: 게임 상태의 저장/로드를 중앙 관리하는 싱글턴
// -> see docs/systems/save-load-architecture.md 섹션 4 for API 설계
// -> see docs/systems/save-load-architecture.md 섹션 6.1 for 원자적 쓰기
namespace SeedMind.Save
{
    using SeedMind.Core;
    using SeedMind.Save.Data;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class SaveManager : Singleton<SaveManager>
    {
        private const int MAX_SLOTS = 3;
        private const string SAVE_DIR = "Saves";
        private const string FILE_PREFIX = "save_";
        private const string FILE_EXT = ".json";
        private const string BACKUP_EXT = ".json.bak";
        private const string META_FILE = "save_meta.json";
        public const string CURRENT_VERSION = "1.0.0";

        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        private int _activeSlot = -1;
        private bool _isSaving;
        private bool _isLoading;
        private float _playTimeAccumulator;
        private readonly List<ISaveable> _saveables = new List<ISaveable>();

        public void Register(ISaveable saveable)
        {
            if (!_saveables.Contains(saveable))
                _saveables.Add(saveable);
        }

        public void Unregister(ISaveable saveable)
        {
            _saveables.Remove(saveable);
        }

        public async Task<bool> SaveAsync(int slotIndex)
        {
            if (_isSaving) return false;
            if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return false;

            _isSaving = true;
            SaveEvents.RaiseSaveStarted(slotIndex);

            try
            {
                var gameSaveData = new GameSaveData
                {
                    saveVersion = CURRENT_VERSION,
                    savedAt = DateTime.UtcNow.ToString("o"),
                    playTimeSeconds = (int)_playTimeAccumulator
                };

                foreach (var saveable in _saveables.OrderBy(s => s.SaveLoadOrder))
                    saveable.GetSaveData();

                string json = JsonConvert.SerializeObject(gameSaveData, _jsonSettings);
                string dir = Path.Combine(Application.persistentDataPath, SAVE_DIR);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                string mainPath = Path.Combine(dir, $"{FILE_PREFIX}{slotIndex}{FILE_EXT}");
                string backupPath = Path.Combine(dir, $"{FILE_PREFIX}{slotIndex}{BACKUP_EXT}");
                string tmpPath = mainPath + ".tmp";

                if (File.Exists(mainPath))
                    File.Copy(mainPath, backupPath, overwrite: true);

                await File.WriteAllTextAsync(tmpPath, json);
                if (File.Exists(mainPath)) File.Delete(mainPath);
                File.Move(tmpPath, mainPath);

                _isSaving = false;
                _activeSlot = slotIndex;
                SaveEvents.RaiseSaveCompleted(slotIndex);
                Debug.Log($"[SaveManager] 저장 완료: slot {slotIndex}");
                return true;
            }
            catch (Exception ex)
            {
                _isSaving = false;
                SaveEvents.RaiseSaveFailed(slotIndex, ex.Message);
                Debug.LogError($"[SaveManager] 저장 실패: slot {slotIndex} — {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoadAsync(int slotIndex)
        {
            if (_isLoading) return false;
            if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return false;

            _isLoading = true;
            SaveEvents.RaiseLoadStarted(slotIndex);

            try
            {
                string dir = Path.Combine(Application.persistentDataPath, SAVE_DIR);
                string mainPath = Path.Combine(dir, $"{FILE_PREFIX}{slotIndex}{FILE_EXT}");
                string backupPath = Path.Combine(dir, $"{FILE_PREFIX}{slotIndex}{BACKUP_EXT}");

                GameSaveData gameSaveData = TryDeserialize(mainPath);
                if (gameSaveData == null)
                {
                    Debug.LogWarning($"[SaveManager] 주 파일 손상, 백업 복구 시도: slot {slotIndex}");
                    gameSaveData = TryDeserialize(backupPath);
                    if (gameSaveData != null)
                        File.Copy(backupPath, mainPath, overwrite: true);
                }

                if (gameSaveData == null)
                {
                    _isLoading = false;
                    SaveEvents.RaiseLoadFailed(slotIndex, "세이브 및 백업 모두 손상");
                    return false;
                }

                gameSaveData = HandleVersionMismatch(gameSaveData);

                var errors = SaveDataValidator.Validate(gameSaveData);
                foreach (var err in errors)
                    Debug.LogWarning($"[SaveManager] 검증 경고: {err}");

                foreach (var saveable in _saveables.OrderBy(s => s.SaveLoadOrder))
                    saveable.LoadSaveData(gameSaveData);

                _isLoading = false;
                _activeSlot = slotIndex;
                SaveEvents.RaiseLoadCompleted(slotIndex);
                Debug.Log($"[SaveManager] 로드 완료: slot {slotIndex}");
                return true;
            }
            catch (SaveVersionException svEx)
            {
                _isLoading = false;
                SaveEvents.RaiseLoadFailed(slotIndex, svEx.Message);
                Debug.LogError($"[SaveManager] 버전 불일치: {svEx.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _isLoading = false;
                SaveEvents.RaiseLoadFailed(slotIndex, ex.Message);
                Debug.LogError($"[SaveManager] 로드 실패: slot {slotIndex} — {ex.Message}");
                return false;
            }
        }

        public async void AutoSaveAsync()
        {
            if (_activeSlot >= 0)
                await SaveAsync(_activeSlot);
        }

        public bool HasSave(int slotIndex)
        {
            string dir = Path.Combine(Application.persistentDataPath, SAVE_DIR);
            return File.Exists(Path.Combine(dir, $"{FILE_PREFIX}{slotIndex}{FILE_EXT}"));
        }

        public SaveSlotInfo GetSlotInfo(int slotIndex)
        {
            string dir = Path.Combine(Application.persistentDataPath, SAVE_DIR);
            string metaPath = Path.Combine(dir, META_FILE);
            if (!File.Exists(metaPath)) return null;
            string json = File.ReadAllText(metaPath);
            var meta = JsonConvert.DeserializeObject<SaveMetaFile>(json, _jsonSettings);
            if (meta?.slots == null || slotIndex >= meta.slots.Length) return null;
            return meta.slots[slotIndex];
        }

        public void DeleteSlot(int slotIndex)
        {
            string dir = Path.Combine(Application.persistentDataPath, SAVE_DIR);
            string mainPath = Path.Combine(dir, $"{FILE_PREFIX}{slotIndex}{FILE_EXT}");
            string backupPath = Path.Combine(dir, $"{FILE_PREFIX}{slotIndex}{BACKUP_EXT}");
            if (File.Exists(mainPath)) File.Delete(mainPath);
            if (File.Exists(backupPath)) File.Delete(backupPath);
        }

        private GameSaveData TryDeserialize(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<GameSaveData>(json, _jsonSettings);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] 역직렬화 실패: {path} — {ex.Message}");
                return null;
            }
        }

        private GameSaveData HandleVersionMismatch(GameSaveData data)
        {
            var savedVer = new Version(data.saveVersion);
            var currentVer = new Version(CURRENT_VERSION);
            if (savedVer.Major != currentVer.Major)
                throw new SaveVersionException($"호환 불가 버전: {data.saveVersion} (현재: {CURRENT_VERSION})");
            if (savedVer < currentVer)
                data = SaveMigrator.Migrate(data);
            return data;
        }
    }
}
