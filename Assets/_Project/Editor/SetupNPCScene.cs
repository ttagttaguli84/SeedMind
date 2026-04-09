// Editor 스크립트: NPC 씬 배치 (T-4)
// NPCs 구분자, NPC GO 3종, 매니저 3종 배치 + SO 참조 연결
using UnityEngine;
using UnityEditor;
using SeedMind.NPC;
using SeedMind.NPC.Data;

namespace SeedMind.Editor
{
    public static class SetupNPCScene
    {
        private const string NpcDataPath   = "Assets/_Project/Data/NPCs/";
        private const string PrefabPath    = "Assets/_Project/Prefabs/NPCs/";
        private const string ManagersName  = "--- MANAGERS ---";

        [MenuItem("SeedMind/Tools/Setup NPC Scene (T-4)")]
        public static void Setup()
        {
            // --- 비활성 포함 오브젝트 탐색 유틸 ---
            GameObject FindInactive(string n)
            {
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                    if (go.name == n && go.scene.IsValid()) return go;
                return null;
            }

            // --- MANAGERS 부모 탐색 ---
            var managers = FindInactive(ManagersName);
            if (managers == null)
            {
                Debug.LogError("[SeedMind] '--- MANAGERS ---' 오브젝트를 찾을 수 없습니다.");
                return;
            }

            // ========== G-01: --- NPCs --- 구분자 ==========
            var npcsRoot = FindInactive("--- NPCs ---");
            if (npcsRoot == null)
            {
                npcsRoot = new GameObject("--- NPCs ---");
                // SCN_Farm 루트에 배치 (부모 없음)
            }

            // ========== NPC SO 로드 ==========
            var soMerchant  = AssetDatabase.LoadAssetAtPath<NPCData>(NpcDataPath + "SO_NPC_GeneralMerchant.asset");
            var soBlacksmith = AssetDatabase.LoadAssetAtPath<NPCData>(NpcDataPath + "SO_NPC_Blacksmith.asset");
            var soCarpenter  = AssetDatabase.LoadAssetAtPath<NPCData>(NpcDataPath + "SO_NPC_Carpenter.asset");
            var soTraveling  = AssetDatabase.LoadAssetAtPath<NPCData>(NpcDataPath + "SO_NPC_TravelingMerchant.asset");
            var soPool       = AssetDatabase.LoadAssetAtPath<TravelingShopPoolData>(NpcDataPath + "SO_TravelingPool_Default.asset");

            // ========== G-02: NPC_GeneralMerchant ==========
            SetupNPC("NPC_GeneralMerchant", soMerchant, npcsRoot.transform, FindInactive);

            // ========== G-03: NPC_Blacksmith ==========
            SetupNPC("NPC_Blacksmith", soBlacksmith, npcsRoot.transform, FindInactive);

            // ========== G-04: NPC_Carpenter ==========
            SetupNPC("NPC_Carpenter", soCarpenter, npcsRoot.transform, FindInactive);

            // ========== G-05: NPCManager ==========
            var npcMgrGO = FindInactive("NPCManager") ?? new GameObject("NPCManager");
            npcMgrGO.transform.SetParent(managers.transform, false);
            var npcMgr = npcMgrGO.GetComponent<NPCManager>() ?? npcMgrGO.AddComponent<NPCManager>();
            var npcMgrSO = new SerializedObject(npcMgr);
            var registryProp = npcMgrSO.FindProperty("_npcRegistry");
            registryProp.arraySize = 4;
            registryProp.GetArrayElementAtIndex(0).objectReferenceValue = soMerchant;
            registryProp.GetArrayElementAtIndex(1).objectReferenceValue = soBlacksmith;
            registryProp.GetArrayElementAtIndex(2).objectReferenceValue = soCarpenter;
            registryProp.GetArrayElementAtIndex(3).objectReferenceValue = soTraveling;
            npcMgrSO.ApplyModifiedProperties();

            // ========== G-06: DialogueSystem ==========
            var dlgSysGO = FindInactive("DialogueSystem") ?? new GameObject("DialogueSystem");
            dlgSysGO.transform.SetParent(managers.transform, false);
            if (dlgSysGO.GetComponent<DialogueSystem>() == null)
                dlgSysGO.AddComponent<DialogueSystem>();

            // ========== G-07: TravelingMerchantScheduler ==========
            var schedulerGO = FindInactive("TravelingMerchantScheduler") ?? new GameObject("TravelingMerchantScheduler");
            schedulerGO.transform.SetParent(managers.transform, false);
            var scheduler = schedulerGO.GetComponent<TravelingMerchantScheduler>()
                         ?? schedulerGO.AddComponent<TravelingMerchantScheduler>();
            var schedulerSO = new SerializedObject(scheduler);
            schedulerSO.FindProperty("_merchantNPCData").objectReferenceValue = soTraveling;
            schedulerSO.FindProperty("_shopPool").objectReferenceValue = soPool;
            schedulerSO.ApplyModifiedProperties();

            // ========== G-09: NPC_TravelingMerchant 프리팹 생성 ==========
            string prefabAssetPath = PrefabPath + "NPC_TravelingMerchant.prefab";
            var existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
            if (existingPrefab == null)
            {
                var travelGO = new GameObject("NPC_TravelingMerchant");
                var ctrl = travelGO.AddComponent<NPCController>();
                var ctrlSO = new SerializedObject(ctrl);
                ctrlSO.FindProperty("_npcData").objectReferenceValue = soTraveling;
                ctrlSO.ApplyModifiedProperties();
                var savedPrefab = PrefabUtility.SaveAsPrefabAsset(travelGO, prefabAssetPath);
                Object.DestroyImmediate(travelGO);

                // Scheduler에 프리팹 참조 연결
                schedulerSO.Update();
                schedulerSO.FindProperty("_merchantPrefab").objectReferenceValue = savedPrefab;
                schedulerSO.ApplyModifiedProperties();
            }
            else
            {
                schedulerSO.Update();
                schedulerSO.FindProperty("_merchantPrefab").objectReferenceValue = existingPrefab;
                schedulerSO.ApplyModifiedProperties();
            }

            // 씬 저장
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            Debug.Log("[SeedMind] NPC 씬 배치 완료 (T-4).");
        }

        private static void SetupNPC(string goName, NPCData data, Transform parent,
            System.Func<string, GameObject> findInactive)
        {
            var go = findInactive(goName) ?? new GameObject(goName);
            go.transform.SetParent(parent, false);
            var ctrl = go.GetComponent<NPCController>() ?? go.AddComponent<NPCController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("_npcData").objectReferenceValue = data;
            so.ApplyModifiedProperties();
        }
    }
}
