// ConnectAnimalManagerRefs — AnimalManager SO 참조 일괄 연결
// -> see docs/mcp/livestock-tasks.md L-4
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SeedMind.Livestock;
using SeedMind.Livestock.Data;

namespace SeedMind.Editor
{
    public static class ConnectAnimalManagerRefs
    {
        private const string ScenePath = "Assets/_Project/Scenes/Main/SCN_Farm.unity";

        [MenuItem("SeedMind/Livestock/Connect AnimalManager Refs")]
        public static void ConnectRefs()
        {
            // 현재 활성 씬 사용 (별도 OpenScene 불필요)
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            var manager = Object.FindFirstObjectByType<AnimalManager>();
            if (manager == null)
            {
                Debug.LogError("[ConnectAnimalManagerRefs] AnimalManager를 씬에서 찾을 수 없음.");
                return;
            }

            var so = manager.GetType();

            // LivestockConfig 연결
            var config = AssetDatabase.LoadAssetAtPath<LivestockConfig>(
                "Assets/_Project/Data/Livestock/SO_LivestockConfig.asset");
            if (config != null)
                SetPrivateField(so, manager, "_livestockConfig", config);
            else
                Debug.LogWarning("[ConnectAnimalManagerRefs] SO_LivestockConfig.asset 없음");

            // AnimalData 배열 연결
            var guids = AssetDatabase.FindAssets("t:AnimalData", new[] { "Assets/_Project/Data/Livestock/Animals" });
            var dataList = new System.Collections.Generic.List<AnimalData>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<AnimalData>(path);
                if (data != null) dataList.Add(data);
            }
            SetPrivateField(so, manager, "_animalDataRegistry", dataList.ToArray());
            Debug.Log($"[ConnectAnimalManagerRefs] AnimalData {dataList.Count}종 연결 완료.");

            EditorUtility.SetDirty(manager);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ConnectAnimalManagerRefs] AnimalManager 참조 연결 및 씬 저장 완료.");
        }

        private static void SetPrivateField(System.Type type, object obj, string fieldName, object value)
        {
            var field = type.GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(obj, value);
            else
                Debug.LogWarning($"[ConnectAnimalManagerRefs] 필드 없음: {fieldName}");
        }
    }
}
