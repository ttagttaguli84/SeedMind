// CreateSoundAssets — 사운드 시스템 SO 에셋 일괄 생성 + AudioMixer 생성
// 실행: Unity 메뉴 > SeedMind > Create Sound Assets
using UnityEngine;
using UnityEditor;
using SeedMind.Audio;
using SeedMind.Audio.Data;

namespace SeedMind.Editor
{
    public static class CreateSoundAssets
    {
        private const string SFX_DIR = "Assets/_Project/Data/Audio/SFX";
        private const string BGM_DIR = "Assets/_Project/Data/Audio/BGM";
        private const string DATA_DIR = "Assets/_Project/Data/Audio";
        private const string MIXER_DIR = "Assets/_Project/Audio";

        [MenuItem("SeedMind/Create Sound Assets")]
        public static void Run()
        {
            CreateSoundDataSOs();
            CreateRegistrySOs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateSoundAssets] 완료: SoundData SO + SoundRegistry + BGMRegistry 생성됨.");
        }

        private static void CreateSoundDataSOs()
        {
            // MVP SFX 목록
            // baseVolume, pitchVariation, is3D → see docs/systems/sound-design.md 섹션 4.1
            var mvp = new (SFXId id, float vol, float pitch, bool is3D, float maxDist)[]
            {
                // 경작
                (SFXId.HoeTillBasic,   0.8f, 0.07f, true,  12f),
                (SFXId.SeedPlant,      0.6f, 0.05f, true,  10f),
                (SFXId.WaterBasic,     0.7f, 0.05f, true,  10f),
                (SFXId.ScytheBasic,    0.8f, 0.07f, true,  12f),
                (SFXId.Harvest,        0.9f, 0.05f, true,  10f),
                (SFXId.Fertilize,      0.6f, 0.05f, true,  10f),
                // 작물
                (SFXId.CropGrow,       0.5f, 0.10f, true,   8f),
                (SFXId.CropWither,     0.7f, 0.05f, true,   8f),
                // 도구
                (SFXId.ToolEquip,      0.6f, 0.05f, false,  0f),
                // UI
                (SFXId.UIClick,        0.7f, 0.05f, false,  0f),
                (SFXId.InventoryOpen,  0.6f, 0.03f, false,  0f),
                (SFXId.InventoryClose, 0.6f, 0.03f, false,  0f),
                (SFXId.Notification,   0.8f, 0.03f, false,  0f),
                (SFXId.UIError,        0.7f, 0.02f, false,  0f),
                (SFXId.UIConfirm,      0.8f, 0.03f, false,  0f),
                // 상점/NPC
                (SFXId.ShopOpen,       0.7f, 0.03f, false,  0f),
                (SFXId.ShopClose,      0.7f, 0.03f, false,  0f),
                (SFXId.Purchase,       0.9f, 0.05f, false,  0f),
                (SFXId.Sale,           0.8f, 0.05f, false,  0f),
                // 진행
                (SFXId.LevelUp,        1.0f, 0.02f, false,  0f),
                (SFXId.XPGain,         0.6f, 0.05f, false,  0f),
                (SFXId.GoldGain,       0.7f, 0.05f, false,  0f),
                (SFXId.GoldSpend,      0.7f, 0.05f, false,  0f),
                // 시간
                (SFXId.MorningChime,   0.9f, 0.02f, false,  0f),
                (SFXId.Sleep,          0.8f, 0.02f, false,  0f),
                // 환경
                (SFXId.FootstepDirt,   0.5f, 0.10f, false,  0f),
                (SFXId.FootstepGrass,  0.5f, 0.10f, false,  0f),
                // 추가 (SoundEventBridge 사용)
                (SFXId.QuestComplete,  1.0f, 0.02f, false,  0f),
                (SFXId.QuestReward,    0.9f, 0.02f, false,  0f),
                (SFXId.AchievementToast, 1.0f, 0.02f, false, 0f),
                (SFXId.ConstructStart,  0.8f, 0.05f, true,  12f),
                (SFXId.ConstructComplete, 1.0f, 0.02f, true, 12f),
                (SFXId.ProcessStart,   0.7f, 0.05f, true,  10f),
                (SFXId.ProcessComplete, 0.9f, 0.03f, true,  10f),
                (SFXId.ToolUpgradeStart,   0.8f, 0.03f, false, 0f),
                (SFXId.ToolUpgradeComplete, 1.0f, 0.02f, false, 0f),
                (SFXId.DialogueStart,  0.6f, 0.03f, false,  0f),
                (SFXId.EveningBell,    0.9f, 0.02f, false,  0f),
            };

            foreach (var (id, vol, pitch, is3D, maxDist) in mvp)
            {
                string path = $"{SFX_DIR}/SD_{id}.asset";
                var existing = AssetDatabase.LoadAssetAtPath<SoundData>(path);
                if (existing != null) continue; // 이미 있으면 스킵

                var so = ScriptableObject.CreateInstance<SoundData>();
                so.id = id;
                so.baseVolume = vol;
                so.pitchVariation = pitch;
                so.is3D = is3D;
                so.maxDistance = maxDist;
                AssetDatabase.CreateAsset(so, path);
            }
        }

        private static void CreateRegistrySOs()
        {
            // SoundRegistry
            string registryPath = $"{DATA_DIR}/SoundRegistry.asset";
            if (AssetDatabase.LoadAssetAtPath<SoundRegistry>(registryPath) == null)
            {
                var reg = ScriptableObject.CreateInstance<SoundRegistry>();
                AssetDatabase.CreateAsset(reg, registryPath);
            }

            // BGMRegistry
            string bgmPath = $"{DATA_DIR}/BGMRegistry.asset";
            if (AssetDatabase.LoadAssetAtPath<BGMRegistry>(bgmPath) == null)
            {
                var bgm = ScriptableObject.CreateInstance<BGMRegistry>();
                AssetDatabase.CreateAsset(bgm, bgmPath);
            }
        }

        // SoundRegistry entries 배열 자동 연결 (SO 생성 후 별도 실행)
        [MenuItem("SeedMind/Connect Sound Registry")]
        public static void ConnectRegistry()
        {
            string registryPath = $"{DATA_DIR}/SoundRegistry.asset";
            var registry = AssetDatabase.LoadAssetAtPath<SoundRegistry>(registryPath);
            if (registry == null) { Debug.LogError("[ConnectRegistry] SoundRegistry.asset 없음. Create Sound Assets 먼저 실행."); return; }

            var guids = AssetDatabase.FindAssets("t:SoundData", new[] { SFX_DIR });
            var list = new System.Collections.Generic.List<SoundData>();
            foreach (var guid in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                var sd = AssetDatabase.LoadAssetAtPath<SoundData>(p);
                if (sd != null) list.Add(sd);
            }

            var so = new SerializedObject(registry);
            var prop = so.FindProperty("entries");
            prop.arraySize = list.Count;
            for (int i = 0; i < list.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log($"[ConnectRegistry] SoundRegistry에 {list.Count}개 SoundData 연결 완료.");
        }
    }
}
