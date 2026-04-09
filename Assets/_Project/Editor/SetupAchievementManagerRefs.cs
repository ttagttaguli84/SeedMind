// Editor 전용: AchievementManager _allAchievements 배열 자동 연결
// -> see docs/systems/achievement-architecture.md 섹션 T-4-02
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using SeedMind.Achievement;
using SeedMind.Achievement.Data;

public static class SetupAchievementManagerRefs
{
    [MenuItem("SeedMind/Setup AchievementManager References")]
    public static void Setup()
    {
        // 씬에서 AchievementManager 탐색
        var manager = Object.FindFirstObjectByType<AchievementManager>();
        if (manager == null)
        {
            Debug.LogError("[SetupAchievementManagerRefs] AchievementManager not found in scene.");
            return;
        }

        // 모든 AchievementData SO 로드
        var guids = AssetDatabase.FindAssets("t:AchievementData", new[] { "Assets/_Project/Data/Achievements" });
        var list = new List<AchievementData>();
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var so = AssetDatabase.LoadAssetAtPath<AchievementData>(path);
            if (so != null) list.Add(so);
        }

        // sortOrder 기준 정렬
        list.Sort((a, b) => a.sortOrder.CompareTo(b.sortOrder));

        // SerializedObject로 배열 설정
        var serialized = new SerializedObject(manager);
        var prop = serialized.FindProperty("_allAchievements");
        prop.arraySize = list.Count;
        for (int i = 0; i < list.Count; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
        serialized.ApplyModifiedProperties();

        EditorUtility.SetDirty(manager);
        Debug.Log($"[SetupAchievementManagerRefs] Linked {list.Count} AchievementData SOs to AchievementManager.");
    }
}
#endif
