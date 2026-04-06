# 세이브/로드 시스템 MCP 태스크 시퀀스

> SaveManager 싱글턴 배치, GameSaveData 데이터 클래스 생성, ISaveable 인터페이스, 자동저장 트리거, 수동 저장/로드 UI, 멀티슬롯 UI, 통합 테스트를 MCP for Unity 태스크로 상세 정의  
> 작성: Claude Code (Opus 4.6) | 2026-04-07  
> 기반 문서: docs/systems/save-load-architecture.md (ARC-011)  
> 문서 ID: ARC-012

---

## Context

이 문서는 `docs/systems/save-load-architecture.md`(ARC-011) Part II의 Phase A~D 개요를 상세한 MCP 태스크 시퀀스로 확장한다. SaveManager 싱글턴, ISaveable 인터페이스, GameSaveData 통합 루트 클래스, AutoSaveTrigger, SaveEvents, SaveSlotUI 등 세이브/로드 시스템의 전체 구현을 MCP for Unity 도구 호출 수준의 구체적 명세로 포함한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 세이브/로드 시스템의 데이터 레이어(GameSaveData, SaveMetaFile, SaveSlotInfo), 시스템 레이어(SaveManager, AutoSaveTrigger, SaveEvents, ISaveable, SaveMigrator, SaveDataValidator), UI 레이어(SaveSlotPanel, SaveSlotItemUI), 씬 배치, 통합 테스트를 완성한다.

**전제 조건**:
- ARC-002(scene-setup-tasks.md) Phase A~B 완료: 폴더 구조, SCN_Farm 기본 계층(MANAGERS, UI, Canvas_HUD, Canvas_Overlay, Canvas_Popup)
- ARC-003(farming-tasks.md) 완료: FarmGrid, FarmEvents, 기본 시스템 인프라
- Core 인프라(Singleton base class, GameManager, TimeManager) 컴파일 완료
- economy-architecture.md 기반 EconomyManager, EconomyEvents 구현 완료
- ARC-007(facilities-tasks.md) 완료: BuildingManager, BuildingEvents 구현 완료

---

# Part I -- 설계 요약

---

## 1. 세이브 구조 개요

세이브 시스템은 GameSaveData를 통합 루트로 하여 모든 시스템의 상태를 하나의 JSON 파일로 직렬화한다. 멀티슬롯(3개)을 지원하며, 원자적 쓰기와 백업 파일로 데이터 안정성을 보장한다.

(-> see `docs/systems/save-load-architecture.md` 섹션 1 for SaveManager 역할/책임, 저장 경로, 직렬화 방식, 멀티슬롯 구조)

### 1.1 GameSaveData 트리 구조

GameSaveData는 14개 시스템 SaveData와 3개 메타데이터 필드로 구성된 통합 루트 클래스이다.

(-> see `docs/systems/save-load-architecture.md` 섹션 2.1 for 전체 클래스 계층 트리)
(-> see `docs/systems/save-load-architecture.md` 섹션 2.2 for JSON 스키마)
(-> see `docs/systems/save-load-architecture.md` 섹션 2.3 for C# 통합 루트 클래스)

### 1.2 자동저장 트리거

자동저장은 AutoSaveTrigger 컴포넌트가 게임 이벤트(하루 종료, 계절 전환, 시설 건설 완료)를 감지하여 SaveManager에 요청한다. 쿨다운으로 중복 저장을 방지한다.

(-> see `docs/systems/save-load-architecture.md` 섹션 3 for 트리거 시스템 전체 설계)
(-> see `docs/systems/save-load-architecture.md` 섹션 3.2 for 트리거 이벤트 목록)
(-> see `docs/systems/save-load-architecture.md` 섹션 3.3 for 쿨다운 파라미터)

### 1.3 ISaveable 인터페이스 및 SaveLoadOrder

각 시스템은 ISaveable 인터페이스를 구현하여 SaveManager에 등록한다. SaveLoadOrder 값에 따라 저장/로드 순서가 결정된다.

(-> see `docs/systems/save-load-architecture.md` 섹션 7 for ISaveable 정의 및 SaveLoadOrder 전체 할당표)

### 1.4 오류 처리

원자적 쓰기(임시 파일 + rename), 백업 파일 복구, 세이브 버전 마이그레이션을 포함한다.

(-> see `docs/systems/save-load-architecture.md` 섹션 6 for 오류 처리 전체 설계)

---

## 2. 태스크 맵

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| T-1 | 폴더 생성 및 인터페이스/이벤트 스크립트 생성 | ~10회 |
| T-2 | SaveManager, AutoSaveTrigger 스크립트 생성 | ~4회 |
| T-3 | GameSaveData 및 데이터 클래스 스크립트 생성 | ~5회 |
| T-4 | SaveMigrator, SaveDataValidator 스크립트 생성 | ~3회 |
| T-5 | 씬 배치 및 컴포넌트 부착 | ~8회 |
| T-6 | 자동저장 트리거 이벤트 연결 확인 | ~6회 |
| T-7 | SaveSlotPanel UI 생성 | ~32회 |
| T-8 | 수동 저장/로드 UI 연결 (PauseMenu 연동) | ~8회 |
| T-9 | 통합 테스트 시퀀스 | ~18회 |
| **합계** | | **~94회** |

## 3. 스크립트 목록

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Save/ISaveable.cs` | `ISaveable` (interface) | `SeedMind.Save` | T-1 Phase 1 |
| S-02 | `Scripts/Save/SaveEvents.cs` | `SaveEvents` (static class) | `SeedMind.Save` | T-1 Phase 1 |
| S-03 | `Scripts/Save/SaveVersionException.cs` | `SaveVersionException` (Exception) | `SeedMind.Save` | T-1 Phase 1 |
| S-04 | `Scripts/Save/SaveManager.cs` | `SaveManager` (MonoBehaviour, Singleton) | `SeedMind.Save` | T-2 Phase 2 |
| S-05 | `Scripts/Save/AutoSaveTrigger.cs` | `AutoSaveTrigger` (MonoBehaviour) | `SeedMind.Save` | T-2 Phase 2 |
| S-06 | `Scripts/Save/Data/GameSaveData.cs` | `GameSaveData` ([Serializable]) | `SeedMind.Save.Data` | T-3 Phase 3 |
| S-07 | `Scripts/Save/Data/SaveMetaFile.cs` | `SaveMetaFile` ([Serializable]) | `SeedMind.Save.Data` | T-3 Phase 3 |
| S-08 | `Scripts/Save/Data/SaveSlotInfo.cs` | `SaveSlotInfo` ([Serializable]) | `SeedMind.Save.Data` | T-3 Phase 3 |
| S-09 | `Scripts/Save/SaveMigrator.cs` | `SaveMigrator` (static class) | `SeedMind.Save` | T-4 Phase 4 |
| S-10 | `Scripts/Save/SaveDataValidator.cs` | `SaveDataValidator` (static class) | `SeedMind.Save` | T-4 Phase 4 |
| S-11 | `Scripts/UI/SaveSlotPanel.cs` | `SaveSlotPanel` (MonoBehaviour) | `SeedMind.UI` | T-7 Phase 5 |
| S-12 | `Scripts/UI/SaveSlotItemUI.cs` | `SaveSlotItemUI` (MonoBehaviour) | `SeedMind.UI` | T-7 Phase 5 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-03 -> S-04~S-05 -> S-06~S-08 -> S-09~S-10 -> S-11~S-12. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

## 4. 씬 GameObject 목록

| # | 오브젝트명 | 부모 | 컴포넌트 | 생성 태스크 |
|---|-----------|------|----------|-----------|
| G-01 | `SaveManager` | `--- MANAGERS ---` | SaveManager, AutoSaveTrigger | T-5 |
| G-02 | `SaveSlotPanel` | `Canvas_Popup` | SaveSlotPanel | T-7 |
| G-03 | `SaveSlotItem_0` | `SaveSlotPanel` | SaveSlotItemUI | T-7 |
| G-04 | `SaveSlotItem_1` | `SaveSlotPanel` | SaveSlotItemUI | T-7 |
| G-05 | `SaveSlotItem_2` | `SaveSlotPanel` | SaveSlotItemUI | T-7 |
| G-06 | `SCN_Test_SaveLoad.unity` | (독립 씬) | 테스트 전용 | T-9 |

## 5. 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_HUD`, `Canvas_Overlay`, `Canvas_Popup` (UI 루트) | ARC-002 Phase B |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `Assets/_Project/Scripts/UI/` 폴더 | ARC-002 Phase A |
| `TimeManager` | ARC-002 |
| `FarmGrid`, `FarmEvents` | ARC-003 |
| `EconomyManager`, `EconomyEvents` | economy-architecture.md |
| `BuildingManager`, `BuildingEvents` | ARC-007 (facilities-tasks.md) |

## 6. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 스크립트 폴더 생성 | T-1 |
| `create_script` | C# 스크립트 파일 생성 | T-1~T-4, T-7 |
| `create_object` | 빈 GameObject 생성 | T-5, T-7 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | T-5, T-7 |
| `set_parent` | 오브젝트 부모 설정 | T-5, T-7 |
| `set_property` | 컴포넌트 프로퍼티 설정 | T-5~T-8 전체 |
| `save_scene` | 씬 저장 | T-5, T-7, T-9 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | T-9 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | T-1~T-4 |
| `execute_method` | 런타임 메서드 호출 (테스트) | T-9 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | T-9 |

[RISK] `create_script` 도구의 content 파라미터 길이 제한 사전 검증 필요. SaveManager.cs는 비동기 API, 원자적 쓰기, 백업 로직을 포함하여 상당히 길다. 필요 시 partial class로 분할하여 여러 `create_script` 호출로 나눌 수 있다.

---

# Part II -- MCP 태스크 시퀀스

---

## T-1: 폴더 생성 및 인터페이스/이벤트 스크립트 생성

**목적**: 세이브/로드 시스템의 폴더 구조를 생성하고, 의존성이 없는 기초 스크립트(ISaveable, SaveEvents, SaveVersionException)를 작성한다.

**전제**: ARC-002 Phase A 완료(기본 폴더 구조). Core 인프라(Singleton base class) 컴파일 완료.

**의존 태스크**: ARC-002(씬 기본 계층)

---

### T-1 Phase 1: 폴더 및 기초 스크립트 (S-01 ~ S-03)

#### T-1-01: 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Save"

create_folder
  path: "Assets/_Project/Scripts/Save/Data"
```

- **MCP 호출**: 2회

#### T-1-02: ISaveable 인터페이스 (S-01)

```
create_script
  path: "Assets/_Project/Scripts/Save/ISaveable.cs"
  content: |
    // S-01: 저장/로드 대상 시스템이 구현하는 인터페이스
    // -> see docs/systems/save-load-architecture.md 섹션 7
    // -> see docs/pipeline/data-pipeline.md Part II 섹션 3.6 for canonical 정의
    namespace SeedMind.Save
    {
        /// <summary>
        /// 저장/로드 대상 시스템이 구현하는 인터페이스.
        /// SaveManager가 이 인터페이스를 통해 각 시스템과 통신한다.
        /// </summary>
        public interface ISaveable
        {
            /// <summary>복원 순서 (낮을수록 먼저 로드).</summary>
            int SaveLoadOrder { get; }

            /// <summary>현재 상태를 직렬화 가능한 객체로 반환.</summary>
            object GetSaveData();

            /// <summary>직렬화된 데이터에서 상태를 복원.</summary>
            void LoadSaveData(object data);
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음

#### T-1-03: SaveEvents 정적 클래스 (S-02)

```
create_script
  path: "Assets/_Project/Scripts/Save/SaveEvents.cs"
  content: |
    // S-02: 세이브/로드 시스템의 정적 이벤트 허브
    // -> see docs/systems/save-load-architecture.md 섹션 5.1
    namespace SeedMind.Save
    {
        using System;

        /// <summary>
        /// 세이브/로드 시스템의 정적 이벤트 허브.
        /// UI, 자동저장 트리거 등 외부 시스템이 구독한다.
        /// </summary>
        public static class SaveEvents
        {
            // --- 저장 이벤트 ---
            public static event Action<int> OnSaveStarted;           // slotIndex
            public static event Action<int> OnSaveCompleted;         // slotIndex
            public static event Action<int, string> OnSaveFailed;    // slotIndex, errorMessage

            // --- 로드 이벤트 ---
            public static event Action<int> OnLoadStarted;           // slotIndex
            public static event Action<int> OnLoadCompleted;         // slotIndex
            public static event Action<int, string> OnLoadFailed;    // slotIndex, errorMessage

            // --- 자동저장 이벤트 ---
            public static event Action<string> OnAutoSaveTriggered;  // reason

            // --- 발행 메서드 ---
            internal static void RaiseSaveStarted(int slot) => OnSaveStarted?.Invoke(slot);
            internal static void RaiseSaveCompleted(int slot) => OnSaveCompleted?.Invoke(slot);
            internal static void RaiseSaveFailed(int slot, string msg) => OnSaveFailed?.Invoke(slot, msg);
            internal static void RaiseLoadStarted(int slot) => OnLoadStarted?.Invoke(slot);
            internal static void RaiseLoadCompleted(int slot) => OnLoadCompleted?.Invoke(slot);
            internal static void RaiseLoadFailed(int slot, string msg) => OnLoadFailed?.Invoke(slot, msg);
            internal static void RaiseAutoSaveTriggered(string reason) => OnAutoSaveTriggered?.Invoke(reason);
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음

#### T-1-04: SaveVersionException (S-03)

```
create_script
  path: "Assets/_Project/Scripts/Save/SaveVersionException.cs"
  content: |
    // S-03: 세이브 버전 불일치 예외
    // -> see docs/systems/save-load-architecture.md 섹션 6.3
    namespace SeedMind.Save
    {
        public class SaveVersionException : System.Exception
        {
            public SaveVersionException(string message) : base(message) { }
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음

#### T-1-05: 컴파일 대기

```
execute_menu_item
  menu_path: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: 콘솔에 컴파일 에러 없음 확인

---

**T-1 Phase 1 소계**: MCP 호출 6회

---

## T-2: SaveManager, AutoSaveTrigger 스크립트 생성

**목적**: 세이브/로드의 핵심 싱글턴(SaveManager)과 자동저장 트리거(AutoSaveTrigger)를 생성한다.

**전제**: T-1 Phase 1 완료 (ISaveable, SaveEvents 컴파일 완료)

**의존 태스크**: T-1

---

### T-2 Phase 2: 시스템 스크립트 (S-04 ~ S-05)

#### T-2-01: SaveManager (S-04)

```
create_script
  path: "Assets/_Project/Scripts/Save/SaveManager.cs"
  content: |
    // S-04: 게임 상태의 저장/로드를 중앙 관리하는 싱글턴
    // -> see docs/systems/save-load-architecture.md 섹션 4 for API 설계
    // -> see docs/systems/save-load-architecture.md 섹션 4.2 for 저장 흐름
    // -> see docs/systems/save-load-architecture.md 섹션 4.3 for 로드 흐름
    // -> see docs/systems/save-load-architecture.md 섹션 6.1 for 원자적 쓰기
    // -> see docs/systems/save-load-architecture.md 섹션 6.2 for 손상 복구
    namespace SeedMind.Save
    {
        using SeedMind.Save.Data;
        using Newtonsoft.Json;
        using System;
        using System.IO;
        using System.Threading.Tasks;
        using System.Collections.Generic;
        using System.Linq;
        using UnityEngine;

        /// <summary>
        /// 게임 상태의 저장/로드를 중앙 관리.
        /// DontDestroyOnLoad Singleton.
        /// </summary>
        public class SaveManager : Singleton<SaveManager>
        {
            // --- 상수 ---
            private const int MAX_SLOTS = 3;                     // -> see docs/systems/save-load-architecture.md 섹션 1 (슬롯 수 canonical)
            private const string SAVE_DIR = "Saves";
            private const string FILE_PREFIX = "save_";
            private const string FILE_EXT = ".json";
            private const string BACKUP_EXT = ".json.bak";
            private const string META_FILE = "save_meta.json";
            public const string CURRENT_VERSION = "1.0.0";

            // --- 직렬화 설정 ---
            // -> see docs/systems/save-load-architecture.md 섹션 1.3
            // -> see docs/pipeline/data-pipeline.md Part II 섹션 3.2
            private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                DefaultValueHandling = DefaultValueHandling.Include,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // --- 상태 ---
            private int _activeSlot = -1;
            private bool _isSaving;
            private bool _isLoading;
            private float _playTimeAccumulator;

            // --- ISaveable 레지스트리 ---
            private readonly List<ISaveable> _saveables = new List<ISaveable>();

            // === 공개 API ===

            /// <summary>비동기 저장. slotIndex: 0~2.</summary>
            public async Task<bool> SaveAsync(int slotIndex)
            {
                // 저장 흐름 -> see docs/systems/save-load-architecture.md 섹션 4.2
                if (_isSaving) return false;
                if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return false;

                _isSaving = true;
                SaveEvents.RaiseSaveStarted(slotIndex);

                try
                {
                    // GameSaveData 수집
                    var gameSaveData = new GameSaveData();
                    gameSaveData.saveVersion = CURRENT_VERSION;
                    gameSaveData.savedAt = DateTime.UtcNow.ToString("o");
                    gameSaveData.playTimeSeconds = (int)_playTimeAccumulator;

                    foreach (var saveable in _saveables.OrderBy(s => s.SaveLoadOrder))
                    {
                        saveable.GetSaveData();
                    }

                    // JSON 직렬화
                    string json = JsonConvert.SerializeObject(gameSaveData, _jsonSettings);

                    // 원자적 쓰기 -> see docs/systems/save-load-architecture.md 섹션 6.1
                    string dir = GetSaveDir();
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    string mainPath = GetSavePath(slotIndex);
                    string backupPath = GetBackupPath(slotIndex);
                    string tmpPath = mainPath + ".tmp";

                    if (File.Exists(mainPath))
                        File.Copy(mainPath, backupPath, overwrite: true);

                    await File.WriteAllTextAsync(tmpPath, json);

                    if (File.Exists(mainPath)) File.Delete(mainPath);
                    File.Move(tmpPath, mainPath);

                    // 메타 갱신
                    UpdateMeta(slotIndex, gameSaveData);

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

            /// <summary>비동기 로드. slotIndex: 0~2.</summary>
            public async Task<bool> LoadAsync(int slotIndex)
            {
                // 로드 흐름 -> see docs/systems/save-load-architecture.md 섹션 4.3
                if (_isLoading) return false;
                if (slotIndex < 0 || slotIndex >= MAX_SLOTS) return false;

                _isLoading = true;
                SaveEvents.RaiseLoadStarted(slotIndex);

                try
                {
                    GameSaveData gameSaveData = await TryLoadWithFallback(slotIndex);
                    if (gameSaveData == null)
                    {
                        _isLoading = false;
                        SaveEvents.RaiseLoadFailed(slotIndex, "세이브 및 백업 모두 손상");
                        return false;
                    }

                    // 버전 검증 + 마이그레이션
                    gameSaveData = HandleVersionMismatch(gameSaveData);

                    // 데이터 무결성 검증
                    var errors = SaveDataValidator.Validate(gameSaveData);
                    if (errors.Count > 0)
                    {
                        foreach (var err in errors)
                            Debug.LogWarning($"[SaveManager] 검증 경고: {err}");
                    }

                    // 시스템별 데이터 복원 (SaveLoadOrder 순)
                    // -> see docs/systems/save-load-architecture.md 섹션 7 for SaveLoadOrder 할당표
                    foreach (var saveable in _saveables.OrderBy(s => s.SaveLoadOrder))
                    {
                        saveable.LoadSaveData(gameSaveData);
                    }

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

            /// <summary>자동저장. 현재 _activeSlot에 저장.</summary>
            public async void AutoSaveAsync()
            {
                if (_activeSlot >= 0)
                    await SaveAsync(_activeSlot);
            }

            /// <summary>슬롯 메타 정보 조회.</summary>
            public SaveSlotInfo GetSlotInfo(int slotIndex)
            {
                // -> see docs/systems/save-load-architecture.md 섹션 4.4
                string metaPath = Path.Combine(GetSaveDir(), META_FILE);
                if (!File.Exists(metaPath)) return null;

                string json = File.ReadAllText(metaPath);
                var meta = JsonConvert.DeserializeObject<SaveMetaFile>(json, _jsonSettings);
                if (meta?.slots == null || slotIndex >= meta.slots.Length) return null;
                return meta.slots[slotIndex];
            }

            /// <summary>슬롯 삭제.</summary>
            public void DeleteSlot(int slotIndex)
            {
                string mainPath = GetSavePath(slotIndex);
                string backupPath = GetBackupPath(slotIndex);
                if (File.Exists(mainPath)) File.Delete(mainPath);
                if (File.Exists(backupPath)) File.Delete(backupPath);
                // save_meta.json 갱신
            }

            /// <summary>저장 파일 존재 여부 확인.</summary>
            public bool HasSave(int slotIndex)
            {
                return File.Exists(GetSavePath(slotIndex));
            }

            // === ISaveable 관리 ===

            public void Register(ISaveable saveable)
            {
                if (!_saveables.Contains(saveable))
                    _saveables.Add(saveable);
            }

            public void Unregister(ISaveable saveable)
            {
                _saveables.Remove(saveable);
            }

            // === 내부 메서드 ===

            // -> see docs/systems/save-load-architecture.md 섹션 6.2
            private async Task<GameSaveData> TryLoadWithFallback(int slotIndex)
            {
                string mainPath = GetSavePath(slotIndex);
                string backupPath = GetBackupPath(slotIndex);

                GameSaveData data = TryDeserialize(mainPath);
                if (data != null) return data;

                Debug.LogWarning($"[SaveManager] 주 세이브 파일 손상, 백업에서 복구 시도: slot {slotIndex}");
                data = TryDeserialize(backupPath);
                if (data != null)
                {
                    File.Copy(backupPath, mainPath, overwrite: true);
                    return data;
                }

                Debug.LogError($"[SaveManager] 세이브 및 백업 모두 손상: slot {slotIndex}");
                return null;
            }

            private GameSaveData TryDeserialize(string path)
            {
                if (!File.Exists(path)) return null;
                try
                {
                    string json = File.ReadAllText(path);
                    return JsonConvert.DeserializeObject<GameSaveData>(json, _jsonSettings);
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[SaveManager] 역직렬화 실패: {path} — {ex.Message}");
                    return null;
                }
            }

            // -> see docs/systems/save-load-architecture.md 섹션 6.3
            private GameSaveData HandleVersionMismatch(GameSaveData data)
            {
                var savedVer = new Version(data.saveVersion);
                var currentVer = new Version(CURRENT_VERSION);

                if (savedVer.Major != currentVer.Major)
                    throw new SaveVersionException(
                        $"호환되지 않는 세이브 버전: {data.saveVersion} (현재: {CURRENT_VERSION})");

                if (savedVer < currentVer)
                    data = SaveMigrator.Migrate(data);

                return data;
            }

            private void UpdateMeta(int slotIndex, GameSaveData gameSaveData)
            {
                // -> see docs/systems/save-load-architecture.md 섹션 4.4 for SaveSlotInfo 구조
                // 메타 파일 로드 -> 갱신 -> 재저장
            }

            // === 유틸리티 ===

            private string GetSaveDir()
            {
                return Path.Combine(Application.persistentDataPath, SAVE_DIR);
            }

            private string GetSavePath(int slotIndex)
            {
                return Path.Combine(GetSaveDir(), $"{FILE_PREFIX}{slotIndex}{FILE_EXT}");
            }

            private string GetBackupPath(int slotIndex)
            {
                return Path.Combine(GetSaveDir(), $"{FILE_PREFIX}{slotIndex}{BACKUP_EXT}");
            }
        }
    }
```

- **MCP 호출**: 1회
- **검증**: Singleton<T> base class 존재 여부, Newtonsoft.Json 패키지 설치 확인

[RISK] SaveManager.cs의 content가 매우 길다. MCP `create_script`의 content 파라미터에 길이 제한이 있을 경우, partial class로 분할해야 한다. 분할 시: `SaveManager.cs`(공개 API + 레지스트리), `SaveManager.IO.cs`(파일 I/O + 원자적 쓰기), `SaveManager.Migration.cs`(버전 처리).

#### T-2-02: AutoSaveTrigger (S-05)

```
create_script
  path: "Assets/_Project/Scripts/Save/AutoSaveTrigger.cs"
  content: |
    // S-05: 게임 이벤트를 감지하여 자동저장을 요청하는 컴포넌트
    // -> see docs/systems/save-load-architecture.md 섹션 3
    namespace SeedMind.Save
    {
        using UnityEngine;

        /// <summary>
        /// 게임 이벤트를 감지하여 자동저장을 요청하는 컴포넌트.
        /// SaveManager와 분리하여 트리거 로직을 독립 관리한다.
        /// </summary>
        public class AutoSaveTrigger : MonoBehaviour
        {
            // --- 설정 ---
            // -> see docs/systems/save-load-architecture.md 섹션 3.3 for 쿨다운 파라미터
            [SerializeField] private float saveCooldownSeconds = 60f;

            // --- 상태 ---
            private float _lastSaveTime;
            private bool _pendingSave;

            // --- 이벤트 구독 ---
            // -> see docs/systems/save-load-architecture.md 섹션 3.2 for 트리거 목록
            private void OnEnable()
            {
                // TimeManager.OnDayChanged += OnDayChanged;
                // TimeManager.OnSeasonChanged += OnSeasonChanged;
                // BuildingEvents.OnConstructionCompleted += OnFacilityBuilt;
            }

            private void OnDisable()
            {
                // TimeManager.OnDayChanged -= OnDayChanged;
                // TimeManager.OnSeasonChanged -= OnSeasonChanged;
                // BuildingEvents.OnConstructionCompleted -= OnFacilityBuilt;
            }

            private void OnDayChanged(int newDay)
            {
                RequestAutoSave("DayChanged");
            }

            private void OnSeasonChanged()
            {
                RequestAutoSave("SeasonChanged");
            }

            private void OnFacilityBuilt()
            {
                RequestAutoSave("FacilityBuilt");
            }

            private void RequestAutoSave(string reason)
            {
                if (Time.realtimeSinceStartup - _lastSaveTime < saveCooldownSeconds)
                {
                    _pendingSave = true;
                    return;
                }
                ExecuteAutoSave(reason);
            }

            private void ExecuteAutoSave(string reason)
            {
                _lastSaveTime = Time.realtimeSinceStartup;
                _pendingSave = false;
                SaveEvents.RaiseAutoSaveTriggered(reason);
                SaveManager.Instance.AutoSaveAsync();
            }

            private void Update()
            {
                if (_pendingSave && Time.realtimeSinceStartup - _lastSaveTime >= saveCooldownSeconds)
                {
                    ExecuteAutoSave("Deferred");
                }
            }
        }
    }
```

- **MCP 호출**: 1회
- **검증**: TimeManager, BuildingEvents 이벤트 시그니처 일치 확인. 구독 코드는 주석 처리 상태이며, T-6에서 활성화한다.

#### T-2-03: 컴파일 대기

```
execute_menu_item
  menu_path: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: `SaveManager`, `AutoSaveTrigger` 컴파일 에러 없음 확인

---

**T-2 Phase 2 소계**: MCP 호출 3회

---

## T-3: GameSaveData 및 데이터 클래스 스크립트 생성

**목적**: 세이브 파일의 루트 데이터 구조(GameSaveData)와 메타 정보(SaveMetaFile, SaveSlotInfo)를 생성한다.

**전제**: T-2 완료 (SaveManager 컴파일 완료)

**의존 태스크**: T-2. 또한 각 시스템의 SaveData 클래스(PlayerSaveData, FarmSaveData 등)가 컴파일되어 있어야 GameSaveData에서 참조 가능.

---

### T-3 Phase 3: 데이터 클래스 (S-06 ~ S-08)

#### T-3-01: GameSaveData (S-06)

GameSaveData의 필드 목록은 직접 나열하지 않는다.

(-> see `docs/systems/save-load-architecture.md` 섹션 2.3 for C# 통합 루트 클래스 전체 필드)

```
create_script
  path: "Assets/_Project/Scripts/Save/Data/GameSaveData.cs"
  content: |
    // S-06: 하나의 세이브 슬롯에 저장되는 전체 게임 상태
    // -> see docs/systems/save-load-architecture.md 섹션 2.3 for 전체 필드 정의
    // 개별 SaveData 클래스의 canonical 정의는 각 시스템 문서를 참조
    // PATTERN-005: JSON 스키마(섹션 2.2)와 이 클래스의 필드 수 동일 확인 필수
    namespace SeedMind.Save.Data
    {
        [System.Serializable]
        public class GameSaveData
        {
            // --- 메타데이터 (3개) ---
            public string saveVersion;              // 세이브 포맷 버전
            public string savedAt;                  // 저장 시각 (ISO 8601)
            public int playTimeSeconds;             // 총 플레이 시간 (초)

            // --- 시스템별 세이브 데이터 (14개) ---
            // 각 필드의 타입 정의는 해당 시스템 폴더에 배치
            // -> see docs/systems/save-load-architecture.md 섹션 2.1 for 전체 트리
            // -> see docs/systems/save-load-architecture.md 섹션 8.1 for 개별 SaveData 파일 배치

            // 구현 시 각 시스템 SaveData 클래스를 using 후 필드 선언
            // (Phase 2 구현 시 실제 타입으로 교체)
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음. 메타데이터 3개 + 시스템 데이터 14개 = 총 17개 필드

[OPEN] GameSaveData에서 각 시스템 SaveData 타입을 직접 참조하면 asmdef 의존성이 모든 시스템으로 퍼진다. (-> see `docs/systems/save-load-architecture.md` [OPEN] 2 for 배치 위치 검토)

#### T-3-02: SaveMetaFile (S-07)

```
create_script
  path: "Assets/_Project/Scripts/Save/Data/SaveMetaFile.cs"
  content: |
    // S-07: 세이브 슬롯 메타 파일 구조
    // -> see docs/systems/save-load-architecture.md 섹션 4.4
    namespace SeedMind.Save.Data
    {
        [System.Serializable]
        public class SaveMetaFile
        {
            public SaveSlotInfo[] slots;  // MAX_SLOTS 크기
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음

#### T-3-03: SaveSlotInfo (S-08)

```
create_script
  path: "Assets/_Project/Scripts/Save/Data/SaveSlotInfo.cs"
  content: |
    // S-08: 개별 슬롯의 메타 정보 (UI 표시용)
    // -> see docs/systems/save-load-architecture.md 섹션 4.4
    // -> see docs/pipeline/data-pipeline.md Part II 섹션 3.3 for canonical 정의
    namespace SeedMind.Save.Data
    {
        [System.Serializable]
        public class SaveSlotInfo
        {
            public int slotIndex;
            public string slotName;             // 플레이어 지정 이름
            public string savedAt;              // ISO 8601
            public int year;                    // 표시용: 게임 내 연도
            public int seasonIndex;             // 표시용: 게임 내 계절
            public int day;                     // 표시용: 게임 내 일
            public int playTimeSeconds;         // 총 플레이 시간
            public int gold;                    // 표시용: 보유 골드
            public int level;                   // 표시용: 플레이어 레벨
            public bool exists;                 // 세이브 데이터 존재 여부
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음. save-load-architecture.md 섹션 4.4의 필드와 일치 확인.

#### T-3-04: 컴파일 대기

```
execute_menu_item
  menu_path: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: `GameSaveData`, `SaveMetaFile`, `SaveSlotInfo` 컴파일 에러 없음

---

**T-3 Phase 3 소계**: MCP 호출 4회

---

## T-4: SaveMigrator, SaveDataValidator 스크립트 생성

**목적**: 세이브 버전 마이그레이션과 데이터 무결성 검증 유틸리티를 생성한다.

**전제**: T-3 완료 (GameSaveData 컴파일 완료)

**의존 태스크**: T-3

---

### T-4 Phase 4: 유틸리티 스크립트 (S-09 ~ S-10)

#### T-4-01: SaveMigrator (S-09)

```
create_script
  path: "Assets/_Project/Scripts/Save/SaveMigrator.cs"
  content: |
    // S-09: 세이브 버전 마이그레이션 체인
    // -> see docs/systems/save-load-architecture.md 섹션 6.3
    // -> see docs/pipeline/data-pipeline.md Part II 섹션 3.8 for canonical 정의
    namespace SeedMind.Save
    {
        using SeedMind.Save.Data;
        using UnityEngine;

        /// <summary>
        /// 구버전 세이브 파일을 현재 버전으로 단계적 변환.
        /// </summary>
        public static class SaveMigrator
        {
            /// <summary>
            /// 세이브 데이터를 현재 버전으로 마이그레이션.
            /// 버전 체인을 순차적으로 적용한다.
            /// </summary>
            public static GameSaveData Migrate(GameSaveData data)
            {
                Debug.Log($"[SaveMigrator] 마이그레이션 시작: {data.saveVersion} -> {SaveManager.CURRENT_VERSION}");

                // 버전별 마이그레이션 체인
                // 향후 버전 업데이트 시 여기에 변환 로직 추가
                // 예: if (data.saveVersion == "1.0.0") data = MigrateTo_1_1_0(data);

                data.saveVersion = SaveManager.CURRENT_VERSION;
                Debug.Log($"[SaveMigrator] 마이그레이션 완료: {data.saveVersion}");
                return data;
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-4-02: SaveDataValidator (S-10)

```
create_script
  path: "Assets/_Project/Scripts/Save/SaveDataValidator.cs"
  content: |
    // S-10: 세이브 데이터 무결성 검증
    // -> see docs/systems/save-load-architecture.md 섹션 6.4
    // -> see docs/pipeline/data-pipeline.md Part II 섹션 5.2 for 검증 규칙
    namespace SeedMind.Save
    {
        using SeedMind.Save.Data;
        using System.Collections.Generic;

        /// <summary>
        /// 역직렬화된 세이브 데이터의 무결성을 검증.
        /// 치명적이지 않은 오류는 경고 목록으로 반환한다.
        /// </summary>
        public static class SaveDataValidator
        {
            public static List<string> Validate(GameSaveData data)
            {
                var errors = new List<string>();

                if (data == null)
                {
                    errors.Add("GameSaveData is null");
                    return errors;
                }

                if (string.IsNullOrEmpty(data.saveVersion))
                    errors.Add("saveVersion이 비어있음");

                if (data.playTimeSeconds < 0)
                    errors.Add("playTimeSeconds가 음수");

                // 추가 검증 규칙은 구현 시 확장
                // -> see docs/pipeline/data-pipeline.md Part II 섹션 5.2

                return errors;
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-4-03: 컴파일 대기

```
execute_menu_item
  menu_path: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: `SaveMigrator`, `SaveDataValidator` 컴파일 에러 없음

---

**T-4 Phase 4 소계**: MCP 호출 3회

---

## T-5: 씬 배치 및 컴포넌트 부착

**목적**: SCN_Farm 씬에 SaveManager GameObject를 배치하고 컴포넌트를 부착한다.

**전제**: T-4 완료 (전체 Save 스크립트 컴파일 완료)

**의존 태스크**: T-4, ARC-002(씬 기본 계층)

---

#### T-5-01: SaveManager GameObject 생성

```
create_object
  name: "SaveManager"
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

#### T-5-02: 부모 설정

```
set_parent
  object: "SaveManager"
  parent: "--- MANAGERS ---"
```

- **MCP 호출**: 1회

#### T-5-03: SaveManager 컴포넌트 부착

```
add_component
  object: "SaveManager"
  component: "SeedMind.Save.SaveManager"
```

- **MCP 호출**: 1회
- **검증**: `SaveManager` 컴포넌트 부착 성공 확인

#### T-5-04: AutoSaveTrigger 컴포넌트 부착

```
add_component
  object: "SaveManager"
  component: "SeedMind.Save.AutoSaveTrigger"
```

- **MCP 호출**: 1회
- **검증**: `AutoSaveTrigger` 컴포넌트 부착 성공 확인

#### T-5-05: AutoSaveTrigger 쿨다운 설정

```
set_property
  object: "SaveManager"
  component: "AutoSaveTrigger"
  property: "saveCooldownSeconds"
  value: 60
```

- **MCP 호출**: 1회
- 쿨다운 값 (-> see `docs/systems/save-load-architecture.md` 섹션 3.3)

#### T-5-06: DontDestroyOnLoad 확인

SaveManager는 Singleton<T> 기반이므로 DontDestroyOnLoad가 자동 적용된다. 별도 MCP 호출 불필요.

#### T-5-07: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

**T-5 소계**: MCP 호출 6회

---

## T-6: 자동저장 트리거 이벤트 연결 확인

**목적**: AutoSaveTrigger의 이벤트 구독 코드를 활성화하고, 올바르게 연결되는지 확인한다.

**전제**: T-5 완료. TimeManager, BuildingEvents가 컴파일 완료 상태.

**의존 태스크**: T-5, ARC-002(TimeManager), ARC-007(BuildingEvents)

---

#### T-6-01: AutoSaveTrigger 이벤트 구독 활성화

AutoSaveTrigger.cs의 주석 처리된 이벤트 구독 코드를 활성화한다. 이 작업은 `create_script`로 AutoSaveTrigger.cs를 재작성하여 수행한다.

```
create_script
  path: "Assets/_Project/Scripts/Save/AutoSaveTrigger.cs"
  content: |
    // S-05 (updated): 이벤트 구독 활성화 버전
    // -> see docs/systems/save-load-architecture.md 섹션 3
    namespace SeedMind.Save
    {
        using UnityEngine;

        public class AutoSaveTrigger : MonoBehaviour
        {
            [SerializeField] private float saveCooldownSeconds = 60f;
            // -> see docs/systems/save-load-architecture.md 섹션 3.3

            private float _lastSaveTime;
            private bool _pendingSave;

            private void OnEnable()
            {
                // -> see docs/systems/save-load-architecture.md 섹션 3.2
                TimeManager.OnDayChanged += OnDayChanged;
                TimeManager.OnSeasonChanged += OnSeasonChanged;
                BuildingEvents.OnConstructionCompleted += OnFacilityBuilt;
            }

            private void OnDisable()
            {
                TimeManager.OnDayChanged -= OnDayChanged;
                TimeManager.OnSeasonChanged -= OnSeasonChanged;
                BuildingEvents.OnConstructionCompleted -= OnFacilityBuilt;
            }

            private void OnDayChanged(int newDay)
            {
                RequestAutoSave("DayChanged");
            }

            private void OnSeasonChanged()
            {
                RequestAutoSave("SeasonChanged");
            }

            private void OnFacilityBuilt()
            {
                RequestAutoSave("FacilityBuilt");
            }

            private void RequestAutoSave(string reason)
            {
                if (Time.realtimeSinceStartup - _lastSaveTime < saveCooldownSeconds)
                {
                    _pendingSave = true;
                    return;
                }
                ExecuteAutoSave(reason);
            }

            private void ExecuteAutoSave(string reason)
            {
                _lastSaveTime = Time.realtimeSinceStartup;
                _pendingSave = false;
                SaveEvents.RaiseAutoSaveTriggered(reason);
                SaveManager.Instance.AutoSaveAsync();
            }

            private void Update()
            {
                if (_pendingSave && Time.realtimeSinceStartup - _lastSaveTime >= saveCooldownSeconds)
                {
                    ExecuteAutoSave("Deferred");
                }
            }
        }
    }
```

- **MCP 호출**: 1회

[RISK] TimeManager.OnDayChanged, TimeManager.OnSeasonChanged, BuildingEvents.OnConstructionCompleted의 정확한 delegate 시그니처가 구현체마다 다를 수 있다. 컴파일 전에 각 이벤트의 시그니처를 확인해야 한다. 시그니처 불일치 시 핸들러 메서드의 파라미터를 수정한다.

#### T-6-02: 컴파일 대기

```
execute_menu_item
  menu_path: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: 이벤트 시그니처 불일치로 인한 컴파일 에러가 없는지 확인

#### T-6-03: 자동저장 동작 확인

```
enter_play_mode

execute_method
  class: "SeedMind.Save.SaveManager"
  method: "SaveAsync"
  args: [0]

get_console_logs
  filter: "[SaveManager]"

exit_play_mode
```

- **MCP 호출**: 4회
- **검증 체크리스트**:
  - `[SaveManager] 저장 완료: slot 0` 로그 출력
  - `[SaveEvents] OnSaveStarted` 이벤트 발행 확인
  - `[SaveEvents] OnSaveCompleted` 이벤트 발행 확인
  - `persistentDataPath/Saves/save_0.json` 파일 생성

---

**T-6 소계**: MCP 호출 6회

---

## T-7: SaveSlotPanel UI 생성

**목적**: 멀티슬롯 저장/로드 UI를 구성한다. 3개 슬롯 각각에 메타 정보(날짜, 골드, 플레이 시간)를 표시하는 패널을 만든다.

**전제**: T-5 완료. ARC-002 Phase B의 Canvas_Popup 존재.

**의존 태스크**: T-5, ARC-002

---

### T-7 Phase 5: UI 스크립트 (S-11 ~ S-12)

#### T-7-01: SaveSlotPanel 스크립트 (S-11)

```
create_script
  path: "Assets/_Project/Scripts/UI/SaveSlotPanel.cs"
  content: |
    // S-11: 저장 슬롯 선택 패널 UI
    // -> see docs/systems/save-load-architecture.md 섹션 4.4 for 슬롯 메타 정보
    // -> see docs/systems/ui-architecture.md for UI 시스템 전반
    namespace SeedMind.UI
    {
        using SeedMind.Save;
        using SeedMind.Save.Data;
        using UnityEngine;

        /// <summary>
        /// 3개 세이브 슬롯을 표시하는 패널.
        /// 저장 모드 / 로드 모드를 구분하여 동작한다.
        /// </summary>
        public class SaveSlotPanel : MonoBehaviour
        {
            [SerializeField] private SaveSlotItemUI[] slotItems;  // 3개

            private bool _isSaveMode;  // true: 저장, false: 로드

            public void Open(bool saveMode)
            {
                _isSaveMode = saveMode;
                gameObject.SetActive(true);
                RefreshSlots();
            }

            public void Close()
            {
                gameObject.SetActive(false);
            }

            public void OnSlotClicked(int slotIndex)
            {
                if (_isSaveMode)
                    SaveManager.Instance.SaveAsync(slotIndex);
                else
                {
                    if (SaveManager.Instance.HasSave(slotIndex))
                        SaveManager.Instance.LoadAsync(slotIndex);
                    else
                        Debug.Log($"[SaveSlotPanel] 슬롯 {slotIndex}에 저장 데이터 없음");
                }
            }

            private void RefreshSlots()
            {
                for (int i = 0; i < slotItems.Length; i++)
                {
                    var info = SaveManager.Instance.GetSlotInfo(i);
                    slotItems[i].SetData(info, i);
                }
            }

            private void OnEnable()
            {
                SaveEvents.OnSaveCompleted += OnSaveCompleted;
                SaveEvents.OnLoadCompleted += OnLoadCompleted;
            }

            private void OnDisable()
            {
                SaveEvents.OnSaveCompleted -= OnSaveCompleted;
                SaveEvents.OnLoadCompleted -= OnLoadCompleted;
            }

            private void OnSaveCompleted(int slot)
            {
                RefreshSlots();
            }

            private void OnLoadCompleted(int slot)
            {
                Close();
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-7-02: SaveSlotItemUI 스크립트 (S-12)

```
create_script
  path: "Assets/_Project/Scripts/UI/SaveSlotItemUI.cs"
  content: |
    // S-12: 개별 세이브 슬롯 UI 항목
    // -> see docs/systems/save-load-architecture.md 섹션 4.4 for SaveSlotInfo 필드
    namespace SeedMind.UI
    {
        using SeedMind.Save.Data;
        using UnityEngine;
        using TMPro;
        using UnityEngine.UI;

        /// <summary>
        /// 하나의 세이브 슬롯을 표시하는 UI 항목.
        /// 슬롯명, 날짜, 골드, 플레이 시간을 표시한다.
        /// </summary>
        public class SaveSlotItemUI : MonoBehaviour
        {
            [SerializeField] private TMP_Text slotNameText;
            [SerializeField] private TMP_Text dateInfoText;       // "1년 봄 15일"
            [SerializeField] private TMP_Text playTimeText;       // "00:32:15"
            [SerializeField] private TMP_Text goldText;           // "1,250G"
            [SerializeField] private GameObject emptySlotLabel;   // "비어있음"
            [SerializeField] private GameObject filledSlotGroup;  // 데이터 있을 때 표시
            [SerializeField] private Button selectButton;

            private int _slotIndex;

            public void SetData(SaveSlotInfo info, int index)
            {
                _slotIndex = index;

                if (info == null || !info.exists)
                {
                    emptySlotLabel.SetActive(true);
                    filledSlotGroup.SetActive(false);
                    slotNameText.text = $"슬롯 {index + 1}";
                    return;
                }

                emptySlotLabel.SetActive(false);
                filledSlotGroup.SetActive(true);

                slotNameText.text = string.IsNullOrEmpty(info.slotName)
                    ? $"슬롯 {index + 1}" : info.slotName;

                // 계절 이름 변환
                // -> see docs/systems/time-season.md for 계절 인덱스 매핑
                string[] seasonNames = { "봄", "여름", "가을", "겨울" };
                string season = (info.seasonIndex >= 0 && info.seasonIndex < 4)
                    ? seasonNames[info.seasonIndex] : "???";
                dateInfoText.text = $"{info.year}년 {season} {info.day}일";

                int hours = info.playTimeSeconds / 3600;
                int mins = (info.playTimeSeconds % 3600) / 60;
                int secs = info.playTimeSeconds % 60;
                playTimeText.text = $"{hours:D2}:{mins:D2}:{secs:D2}";

                goldText.text = $"{info.gold:N0}G";
            }

            public void OnClick()
            {
                var panel = GetComponentInParent<SaveSlotPanel>();
                panel?.OnSlotClicked(_slotIndex);
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-7-03: 컴파일 대기

```
execute_menu_item
  menu_path: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: `SaveSlotPanel`, `SaveSlotItemUI` 컴파일 에러 없음

#### T-7-04: SaveSlotPanel GameObject 생성

```
create_object
  name: "SaveSlotPanel"
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

#### T-7-05: SaveSlotPanel 부모 설정

```
set_parent
  object: "SaveSlotPanel"
  parent: "Canvas_Popup"
```

- **MCP 호출**: 1회

#### T-7-06: SaveSlotPanel 컴포넌트 부착

```
add_component
  object: "SaveSlotPanel"
  component: "SeedMind.UI.SaveSlotPanel"
```

- **MCP 호출**: 1회

#### T-7-07: RectTransform 설정 (전체 화면)

```
set_property
  object: "SaveSlotPanel"
  component: "RectTransform"
  property: "anchorMin"
  value: { "x": 0, "y": 0 }

set_property
  object: "SaveSlotPanel"
  component: "RectTransform"
  property: "anchorMax"
  value: { "x": 1, "y": 1 }

set_property
  object: "SaveSlotPanel"
  component: "RectTransform"
  property: "offsetMin"
  value: { "x": 0, "y": 0 }

set_property
  object: "SaveSlotPanel"
  component: "RectTransform"
  property: "offsetMax"
  value: { "x": 0, "y": 0 }
```

- **MCP 호출**: 4회

#### T-7-08: 배경 이미지 추가 (반투명 딤 처리)

```
add_component
  object: "SaveSlotPanel"
  component: "UnityEngine.UI.Image"

set_property
  object: "SaveSlotPanel"
  component: "Image"
  property: "color"
  value: { "r": 0, "g": 0, "b": 0, "a": 0.6 }
```

- **MCP 호출**: 2회

#### T-7-09: 슬롯 아이템 생성 (3개)

각 슬롯에 대해 동일한 구조를 반복한다. 슬롯 0을 예시로 기술하고, 슬롯 1~2는 동일 패턴이다.

**슬롯 0 (SaveSlotItem_0)**:

```
create_object
  name: "SaveSlotItem_0"
  scene: "SCN_Farm"

set_parent
  object: "SaveSlotItem_0"
  parent: "SaveSlotPanel"

add_component
  object: "SaveSlotItem_0"
  component: "SeedMind.UI.SaveSlotItemUI"

set_property
  object: "SaveSlotItem_0"
  component: "RectTransform"
  property: "anchoredPosition"
  value: { "x": 0, "y": 100 }

set_property
  object: "SaveSlotItem_0"
  component: "RectTransform"
  property: "sizeDelta"
  value: { "x": 400, "y": 120 }
```

- **MCP 호출 (슬롯 0)**: 5회

**슬롯 1 (SaveSlotItem_1)**: 동일 구조, `anchoredPosition.y` = 0

- **MCP 호출**: 5회

**슬롯 2 (SaveSlotItem_2)**: 동일 구조, `anchoredPosition.y` = -100

- **MCP 호출**: 5회

#### T-7-10: SaveSlotPanel에 슬롯 참조 설정

```
set_property
  object: "SaveSlotPanel"
  component: "SaveSlotPanel"
  property: "slotItems"
  value: ["SaveSlotItem_0", "SaveSlotItem_1", "SaveSlotItem_2"]
```

- **MCP 호출**: 1회

[RISK] `set_property`로 SerializedField 배열에 오브젝트 참조를 설정하는 것이 MCP에서 가능한지 사전 검증 필요. 불가능한 경우 Editor 스크립트를 통한 일괄 참조 설정이 필요하다.

#### T-7-11: 초기 상태 비활성화

```
set_property
  object: "SaveSlotPanel"
  component: "GameObject"
  property: "activeSelf"
  value: false
```

- **MCP 호출**: 1회

#### T-7-12: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

**T-7 Phase 5 소계**: MCP 호출 ~32회

---

## T-8: 수동 저장/로드 UI 연결 (PauseMenu 연동)

**목적**: 일시정지 메뉴의 "저장" / "불러오기" 버튼을 SaveSlotPanel과 연결한다.

**전제**: T-7 완료. PauseMenu UI가 존재한다고 가정.

**의존 태스크**: T-7, ui-architecture.md 기반 PauseMenu 구현

---

#### T-8-01: PauseMenu에 저장 버튼 참조 추가

PauseMenu가 SaveSlotPanel.Open(saveMode)을 호출하도록 연결한다.

```
set_property
  object: "PauseMenu"
  component: "PauseMenuUI"
  property: "saveSlotPanel"
  value: "SaveSlotPanel"
```

- **MCP 호출**: 1회

[OPEN] PauseMenuUI 클래스의 정확한 구현이 아직 확정되지 않았다. ui-architecture.md에서 PauseMenu 설계가 확정된 후 이 단계를 수정해야 할 수 있다.

#### T-8-02: 저장 버튼 OnClick 연결

```
set_property
  object: "Btn_Save"
  component: "Button"
  property: "onClick"
  value: { "target": "PauseMenu", "method": "OnSaveClicked" }
```

- **MCP 호출**: 1회

#### T-8-03: 불러오기 버튼 OnClick 연결

```
set_property
  object: "Btn_Load"
  component: "Button"
  property: "onClick"
  value: { "target": "PauseMenu", "method": "OnLoadClicked" }
```

- **MCP 호출**: 1회

#### T-8-04: 세이브 존재 여부에 따른 로드 버튼 분기

로드 버튼 활성/비활성은 SaveSlotPanel.Open 시 각 SaveSlotItemUI.SetData에서 처리된다. 빈 슬롯 클릭 시 SaveSlotPanel.OnSlotClicked에서 HasSave 확인 후 무시한다.

(-> see T-7-01 SaveSlotPanel.OnSlotClicked 구현)

- **MCP 호출**: 0회 (로직은 이미 T-7에서 구현됨)

#### T-8-05: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

**T-8 소계**: MCP 호출 4회

[OPEN] PauseMenu의 정확한 오브젝트명, 버튼명은 ui-architecture.md 기반 구현에 따라 변경될 수 있다. T-8의 호출 수는 PauseMenu 구조 확정 후 재산정 필요.

---

## T-9: 통합 테스트 시퀀스

**목적**: 세이브/로드 시스템의 핵심 시나리오를 MCP를 통해 검증한다.

**전제**: T-1~T-8 전체 완료.

**의존 태스크**: T-8 (전체 시스템 구성 완료)

---

### T-9-01: 테스트 씬 생성

```
execute_menu_item
  menu_path: "File/New Scene"

save_scene
  scene: "SCN_Test_SaveLoad"
  path: "Assets/_Project/Scenes/Test/"
```

- **MCP 호출**: 2회

### T-9-02: 테스트 시나리오 1 -- 기본 저장/로드

```
enter_play_mode

execute_method
  class: "SeedMind.Save.SaveManager"
  method: "SaveAsync"
  args: [0]

get_console_logs
  filter: "[SaveManager]"
```

- **기대 결과**: `[SaveManager] 저장 완료: slot 0`
- **MCP 호출**: 3회

### T-9-03: 테스트 시나리오 2 -- 로드 확인

```
execute_method
  class: "SeedMind.Save.SaveManager"
  method: "LoadAsync"
  args: [0]

get_console_logs
  filter: "[SaveManager]"
```

- **기대 결과**: `[SaveManager] 로드 완료: slot 0`
- **MCP 호출**: 2회

### T-9-04: 테스트 시나리오 3 -- 멀티슬롯 전환

```
execute_method
  class: "SeedMind.Save.SaveManager"
  method: "SaveAsync"
  args: [1]

execute_method
  class: "SeedMind.Save.SaveManager"
  method: "SaveAsync"
  args: [2]

execute_method
  class: "SeedMind.Save.SaveManager"
  method: "HasSave"
  args: [0]

execute_method
  class: "SeedMind.Save.SaveManager"
  method: "HasSave"
  args: [1]

execute_method
  class: "SeedMind.Save.SaveManager"
  method: "HasSave"
  args: [2]

get_console_logs
  filter: "[SaveManager]"
```

- **기대 결과**: 슬롯 0, 1, 2 모두 HasSave = true
- **MCP 호출**: 6회

### T-9-05: 테스트 시나리오 4 -- 빈 슬롯 로드 시도

```
execute_method
  class: "SeedMind.Save.SaveManager"
  method: "DeleteSlot"
  args: [2]

execute_method
  class: "SeedMind.Save.SaveManager"
  method: "LoadAsync"
  args: [2]

get_console_logs
  filter: "[SaveManager]"
```

- **기대 결과**: 백업 복구 시도 또는 로드 실패 로그
- **MCP 호출**: 3회

### T-9-06: 테스트 종료

```
exit_play_mode
```

- **MCP 호출**: 1회

---

**T-9 소계**: MCP 호출 17회

---

### 통합 테스트 검증 체크리스트

| # | 검증 항목 | 기대 결과 | 테스트 시나리오 |
|---|----------|----------|---------------|
| V-01 | 슬롯 0에 저장 | `save_0.json` 생성, 콘솔 로그 "저장 완료" | T-9-02 |
| V-02 | 슬롯 0에서 로드 | 콘솔 로그 "로드 완료", ISaveable.LoadSaveData 호출 | T-9-03 |
| V-03 | 슬롯 1, 2에 저장 | 각각 `save_1.json`, `save_2.json` 생성 | T-9-04 |
| V-04 | HasSave 확인 | 3개 슬롯 모두 true | T-9-04 |
| V-05 | 삭제된 슬롯 로드 | 실패 또는 백업 복구 | T-9-05 |
| V-06 | SaveEvents 발행 | OnSaveStarted, OnSaveCompleted 이벤트 구독자에게 전달 | T-9-02~T-9-04 |
| V-07 | 자동저장 트리거 | OnDayChanged 시 자동저장 실행 | T-6-03 |
| V-08 | 쿨다운 동작 | 60초 이내 중복 트리거 시 대기, 쿨다운 후 실행 | (수동 검증) |

---

## Cross-references

- `docs/systems/save-load-architecture.md` (ARC-011) -- Part I 설계 원본. SaveManager API, GameSaveData 구조, ISaveable, SaveLoadOrder, 오류 처리 전략
- `docs/architecture.md` -- 마스터 기술 아키텍처
- `docs/systems/project-structure.md` -- 네임스페이스 `SeedMind.Save`, `SeedMind.Save.Data`, 폴더 구조 `Scripts/Save/`
- `docs/pipeline/data-pipeline.md` -- SaveData 필드 정의 canonical (Part I 섹션 3, Part II 섹션 2~3), SaveSlotInfo canonical (섹션 3.3), 자동 저장 트리거 canonical (섹션 3.7), SaveMigrator canonical (섹션 3.8)
- `docs/mcp/scene-setup-tasks.md` (ARC-002) -- 씬 계층, MANAGERS, Canvas_Popup 생성
- `docs/mcp/farming-tasks.md` (ARC-003) -- FarmGrid, FarmEvents 생성
- `docs/mcp/facilities-tasks.md` (ARC-007) -- BuildingManager, BuildingEvents 생성
- `docs/systems/ui-architecture.md` (ARC-018) -- UI 시스템, PauseMenu 설계
- `docs/systems/time-season-architecture.md` -- TimeManager.OnDayChanged, OnSeasonChanged 이벤트 정의
- `docs/systems/economy-architecture.md` -- EconomySaveData (섹션 4.6)
- `docs/systems/inventory-architecture.md` -- InventorySaveData (섹션 6)
- `docs/systems/tutorial-architecture.md` -- TutorialSaveData (섹션 7)
- `docs/systems/achievement-architecture.md` (ARC-017) -- AchievementSaveData (섹션 7)

---

## Open Questions

1. [OPEN] **GameSaveData asmdef 배치**: GameSaveData가 모든 시스템 SaveData 타입을 참조하므로 의존성이 퍼진다. JObject 파싱 또는 인터페이스 기반 역직렬화로 해결 가능. (-> see `docs/systems/save-load-architecture.md` [OPEN] 2)

2. [OPEN] **PauseMenu 연동**: T-8의 PauseMenu 오브젝트명, 버튼명은 ui-architecture.md 구현 확정 후 수정 필요. (-> see `docs/systems/ui-architecture.md`)

3. [OPEN] **MCP set_property로 배열 참조 설정 가능 여부**: T-7-10에서 SaveSlotPanel.slotItems 배열에 오브젝트 참조를 설정할 때, MCP `set_property`가 SerializedField 배열의 오브젝트 참조를 지원하는지 사전 검증 필요.

4. [OPEN] **자동저장 전용 슬롯**: 현재 3개 슬롯에 자동저장이 덮어쓰는 구조. 별도 `save_auto.json` 슬롯 분리 여부. (-> see `docs/systems/save-load-architecture.md` [OPEN] 3)

---

## Risks

1. [RISK] **SaveManager.cs content 길이**: `create_script`의 content 파라미터에 길이 제한이 있을 경우 partial class 분할 필요: `SaveManager.cs` + `SaveManager.IO.cs` + `SaveManager.Migration.cs`.

2. [RISK] **이벤트 시그니처 불일치**: AutoSaveTrigger가 구독하는 TimeManager.OnDayChanged, BuildingEvents.OnConstructionCompleted의 delegate 시그니처가 실제 구현과 다를 수 있다. T-6에서 컴파일 시 확인하고 수정한다.

3. [RISK] **원자적 쓰기 플랫폼 호환성**: Windows에서 `File.Move` 대상 파일 존재 시 예외 발생. Unity 6의 .NET 버전 확인 필요. (-> see `docs/systems/save-load-architecture.md` [RISK] 1)

4. [RISK] **비동기 저장 중 게임 상태 변경**: GetSaveData() 호출이 단일 프레임 내에 완료되어야 한다. 수집이 여러 프레임에 걸치면 데이터 불일치 발생. (-> see `docs/systems/save-load-architecture.md` [RISK] 2)

5. [RISK] **SerializedField 배열 참조 설정**: MCP `set_property`로 MonoBehaviour의 SerializedField 배열에 씬 오브젝트 참조를 할당하는 기능이 지원되지 않을 수 있다. T-7-10에서 실패 시 Editor 스크립트를 통한 참조 설정으로 대체한다.

---

*이 문서는 Claude Code가 save-load-architecture.md(ARC-011)의 Part II 개요를 독립 태스크 문서로 분리하고, MCP for Unity 도구 호출 수준의 상세 명세로 확장하여 자율적으로 작성했습니다.*
