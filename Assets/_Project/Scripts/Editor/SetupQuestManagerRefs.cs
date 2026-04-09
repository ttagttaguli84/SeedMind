// Editor 스크립트: QuestManager SO 배열 참조 연결
// -> see docs/mcp/quest-tasks.md T-4-02 ~ T-4-03
using UnityEngine;
using UnityEditor;
using SeedMind.Quest;
using SeedMind.Quest.Data;

public class SetupQuestManagerRefs : Editor
{
    [MenuItem("SeedMind/Setup QuestManager References")]
    public static void Setup()
    {
        var qm = FindObjectOfType<QuestManager>();
        if (qm == null)
        {
            Debug.LogError("[SetupQuestManagerRefs] QuestManager not found in scene.");
            return;
        }

        var so = new SerializedObject(qm);

        // _allQuests: 메인 4 + 농장도전 4 = 8종
        string[] allQuestPaths = new[]
        {
            "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring01.asset",
            "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring02.asset",
            "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring03.asset",
            "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring04.asset",
            "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstHarvest.asset",
            "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCEarn1000.asset",
            "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstBuilding.asset",
            "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstProcess.asset",
        };

        SetAssetArray(so, "_allQuests", allQuestPaths);

        // _dailyQuestPool: 일일 목표 12종
        string[] dailyPaths = new[]
        {
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyWater.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyHarvest5.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyHarvest10.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailySell.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyEarn.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyTill.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyQuality.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyProcess.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyFertilize.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyDiverse.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyGoldQuality.asset",
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyEarnLarge.asset",
        };

        SetAssetArray(so, "_dailyQuestPool", dailyPaths);
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(qm);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            qm.gameObject.scene);

        Debug.Log("[SetupQuestManagerRefs] QuestManager 참조 연결 완료.");
    }

    private static void SetAssetArray(SerializedObject so,
        string propName, string[] paths)
    {
        var prop = so.FindProperty(propName);
        prop.ClearArray();
        for (int i = 0; i < paths.Length; i++)
        {
            var asset = AssetDatabase.LoadAssetAtPath<QuestData>(paths[i]);
            if (asset == null)
            {
                Debug.LogWarning($"[SetupQuestManagerRefs] Asset not found: {paths[i]}");
                continue;
            }
            prop.InsertArrayElementAtIndex(i);
            prop.GetArrayElementAtIndex(i).objectReferenceValue = asset;
        }
    }
}
