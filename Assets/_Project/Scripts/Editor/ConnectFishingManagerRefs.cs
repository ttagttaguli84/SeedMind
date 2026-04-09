// Editor 스크립트: FishingManager SO/GO 배열 참조 자동 연결
// -> see docs/mcp/fishing-tasks.md F-4-02, F-4-04
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using SeedMind.Fishing;
using SeedMind.Fishing.Data;

public class ConnectFishingManagerRefs : Editor
{
    [MenuItem("SeedMind/Fishing/Connect FishingManager Refs")]
    public static void ConnectAll()
    {
        // FishingManager 찾기
        var manager = FindObjectOfType<FishingManager>();
        if (manager == null)
        {
            Debug.LogError("[ConnectFishingManagerRefs] FishingManager를 씬에서 찾을 수 없습니다.");
            return;
        }

        var so = new SerializedObject(manager);

        // 1) FishingConfig 연결
        string configPath = "Assets/_Project/Data/Fish/SO_FishingConfig.asset";
        var config = AssetDatabase.LoadAssetAtPath<FishingConfig>(configPath);
        if (config != null)
        {
            so.FindProperty("_fishingConfig").objectReferenceValue = config;
            Debug.Log("[ConnectFishingManagerRefs] FishingConfig 연결 완료");
        }
        else
        {
            Debug.LogWarning($"[ConnectFishingManagerRefs] FishingConfig 없음: {configPath}");
        }

        // 2) FishDataRegistry 배열 연결 (15종)
        var guids = AssetDatabase.FindAssets("t:FishData", new[] { "Assets/_Project/Data/Fish" });
        var fishProp = so.FindProperty("_fishDataRegistry");
        fishProp.arraySize = guids.Length;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var fishData = AssetDatabase.LoadAssetAtPath<FishData>(path);
            fishProp.GetArrayElementAtIndex(i).objectReferenceValue = fishData;
        }
        Debug.Log($"[ConnectFishingManagerRefs] FishDataRegistry {guids.Length}종 연결 완료");

        // 3) FishingPoints 배열 연결 (씬 내 FishingPoint GO 탐색)
        var points = FindObjectsOfType<FishingPoint>();
        // pointId 기준으로 정렬 (fp_01, fp_02, fp_03)
        System.Array.Sort(points, (a, b) => string.Compare(a.pointId, b.pointId, System.StringComparison.Ordinal));
        var pointsProp = so.FindProperty("_fishingPoints");
        pointsProp.arraySize = points.Length;
        for (int i = 0; i < points.Length; i++)
            pointsProp.GetArrayElementAtIndex(i).objectReferenceValue = points[i];
        Debug.Log($"[ConnectFishingManagerRefs] FishingPoints {points.Length}개 연결 완료");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
        EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        Debug.Log("[ConnectFishingManagerRefs] 모든 참조 연결 완료");
    }
}
