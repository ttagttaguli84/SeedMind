// Editor 스크립트: QuestData SO 20종 일괄 생성
// -> see docs/mcp/quest-tasks.md T-2
// -> 수치 근거: docs/systems/quest-system.md 섹션 3.1, 5.1, 6.1~6.3
using UnityEngine;
using UnityEditor;
using SeedMind.Quest;
using SeedMind.Quest.Data;
using SeedMind.Core;

public class CreateQuestAssets : Editor
{
    [MenuItem("SeedMind/Create Quest Assets")]
    public static void CreateAll()
    {
        // ── 메인 퀘스트 (봄) ──────────────────────────────────────
        CreateMainSpring01();
        CreateMainSpring02();
        CreateMainSpring03();
        CreateMainSpring04();

        // ── 일일 목표 ─────────────────────────────────────────────
        CreateDailyWater();
        CreateDailyHarvest5();
        CreateDailyHarvest10();
        CreateDailySell();
        CreateDailyEarn();
        CreateDailyTill();
        CreateDailyQuality();
        CreateDailyProcess();
        CreateDailyFertilize();
        CreateDailyDiverse();
        CreateDailyGoldQuality();
        CreateDailyEarnLarge();

        // ── 농장 도전 (초반 4종) ──────────────────────────────────
        CreateFCFirstHarvest();
        CreateFCEarn1000();
        CreateFCFirstBuilding();
        CreateFCFirstProcess();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateQuestAssets] QuestData SO 20종 생성 완료.");
    }

    // ── 헬퍼 ──────────────────────────────────────────────────────

    private static QuestData CreateAsset(string path, string questId,
        QuestCategory cat, string title, string desc, string giverId = "system",
        int timeLimitDays = 0, Season season = Season.Spring, bool repeatable = false)
    {
        var so = ScriptableObject.CreateInstance<QuestData>();
        so.questId        = questId;
        so.category       = cat;
        so.titleKR        = title;
        so.descriptionKR  = desc;
        so.giverId        = giverId;
        so.timeLimitDays  = timeLimitDays;
        so.season         = season;
        so.isRepeatable   = repeatable;
        AssetDatabase.CreateAsset(so, path);
        return so;
    }

    private static QuestObjectiveData Obj(ObjectiveType type, string targetId,
        int required, string desc = "", int minQuality = 0)
    {
        return new QuestObjectiveData
        {
            type           = type,
            targetId       = targetId,
            requiredAmount = required,
            descriptionKR  = desc,
            minQuality     = minQuality
        };
    }

    private static QuestRewardData Reward(RewardType type, int amount,
        string targetId = "", bool scaledByLevel = false)
    {
        return new QuestRewardData
        {
            type           = type,
            amount         = amount,
            targetId       = targetId,
            scaledByLevel  = scaledByLevel
        };
    }

    private static QuestUnlockCondition UnlockTutorial()
    {
        return new QuestUnlockCondition
        {
            type       = UnlockConditionType.TutorialComplete,
            intParam   = 1
        };
    }

    private static QuestUnlockCondition UnlockQuest(string questId)
    {
        return new QuestUnlockCondition
        {
            type        = UnlockConditionType.QuestComplete,
            stringParam = questId
        };
    }

    private static QuestUnlockCondition UnlockLevel(int level)
    {
        return new QuestUnlockCondition
        {
            type     = UnlockConditionType.Level,
            intParam = level
        };
    }

    private static void SetFields(QuestData so,
        QuestObjectiveData[] objectives,
        QuestRewardData[] rewards,
        QuestUnlockCondition[] unlockConditions)
    {
        var serializedObj = new SerializedObject(so);
        SetArray(serializedObj, "objectives",  objectives);
        SetArray(serializedObj, "rewards",     rewards);
        SetArray(serializedObj, "unlockConditions", unlockConditions);
        serializedObj.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(so);
    }

    private static void SetArray<T>(SerializedObject sObj,
        string propName, T[] items) where T : class
    {
        var prop = sObj.FindProperty(propName);
        prop.ClearArray();
        for (int i = 0; i < items.Length; i++)
        {
            prop.InsertArrayElementAtIndex(i);
            var elem = prop.GetArrayElementAtIndex(i);
            var json = JsonUtility.ToJson(items[i]);
            JsonUtility.FromJsonOverwrite(json, new object()); // placeholder
            ApplyJsonToSerializedProperty(elem, items[i]);
        }
    }

    private static void ApplyJsonToSerializedProperty(SerializedProperty prop, object obj)
    {
        if (obj == null) return;
        var fields = obj.GetType().GetFields(
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            var childProp = prop.FindPropertyRelative(field.Name);
            if (childProp == null) continue;
            var val = field.GetValue(obj);
            if (val == null) continue;
            switch (childProp.propertyType)
            {
                case SerializedPropertyType.Integer:
                    childProp.intValue = (int)val; break;
                case SerializedPropertyType.Boolean:
                    childProp.boolValue = (bool)val; break;
                case SerializedPropertyType.Float:
                    childProp.floatValue = (float)val; break;
                case SerializedPropertyType.String:
                    childProp.stringValue = (string)val; break;
                case SerializedPropertyType.Enum:
                    childProp.enumValueIndex = (int)val; break;
            }
        }
    }

    // ── 메인 퀘스트 ───────────────────────────────────────────────

    private static void CreateMainSpring01()
    {
        // 봄의 첫 수확: 감자 또는 당근 10개 수확 | 50G + 5XP | 튜토리얼 완료
        // -> see docs/systems/quest-system.md 섹션 3.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring01.asset",
            "main_spring_01", QuestCategory.MainQuest,
            "봄의 첫 수확", "감자 또는 당근 10개를 수확하세요");
        SetFields(so,
            new[] { Obj(ObjectiveType.Harvest, "", 10, "감자 또는 당근 10개 수확") },
            new[] { Reward(RewardType.Gold, 50), Reward(RewardType.XP, 5) },
            new[] { UnlockTutorial() });
    }

    private static void CreateMainSpring02()
    {
        // 농장 확장의 시작: 경작지 20타일 | 100G + 10XP + 고급비료x5 | main_spring_01 완료
        // -> see docs/systems/quest-system.md 섹션 3.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring02.asset",
            "main_spring_02", QuestCategory.MainQuest,
            "농장 확장의 시작", "경작지를 20타일 이상 보유하세요");
        SetFields(so,
            new[] { Obj(ObjectiveType.Till, "", 20, "경작지 20타일 이상 보유") },
            new[] { Reward(RewardType.Gold, 100), Reward(RewardType.XP, 10),
                    Reward(RewardType.Item, 5, "fertilizer_advanced") },
            new[] { UnlockQuest("main_spring_01") });
    }

    private static void CreateMainSpring03()
    {
        // 다양한 작물 재배: 서로 다른 작물 2종 이상 동시 재배 | 80G + 8XP | 레벨 2
        // -> see docs/systems/quest-system.md 섹션 3.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring03.asset",
            "main_spring_03", QuestCategory.MainQuest,
            "다양한 작물 재배", "서로 다른 작물 2종 이상 동시 재배");
        SetFields(so,
            new[] { Obj(ObjectiveType.Harvest, "diverse_2", 2, "서로 다른 작물 2종 이상 수확") },
            new[] { Reward(RewardType.Gold, 80), Reward(RewardType.XP, 8) },
            new[] { UnlockLevel(2) });
    }

    private static void CreateMainSpring04()
    {
        // 첫 번째 출하 목표: 총 판매 수익 500G | 150G + 13XP + 딸기씨앗x3 | main_spring_02 완료
        // -> see docs/systems/quest-system.md 섹션 3.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring04.asset",
            "main_spring_04", QuestCategory.MainQuest,
            "첫 번째 출하 목표", "총 판매 수익 500G를 달성하세요");
        SetFields(so,
            new[] { Obj(ObjectiveType.EarnGold, "", 500, "총 판매 수익 500G 달성") },
            new[] { Reward(RewardType.Gold, 150), Reward(RewardType.XP, 13),
                    Reward(RewardType.Item, 3, "crop_strawberry_seed") },
            new[] { UnlockQuest("main_spring_02") });
    }

    // ── 일일 목표 ─────────────────────────────────────────────────

    private static void CreateDailyWater()
    {
        // 부지런한 농부: 작물 15개 물주기 | 30G + 1XP | scaledByLevel
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyWater.asset",
            "daily_water", QuestCategory.DailyChallenge,
            "부지런한 농부", "작물 15개에 물을 주세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.Water, "", 15, "작물 15개에 물주기") },
            new[] { Reward(RewardType.Gold, 30, scaledByLevel: true),
                    Reward(RewardType.XP, 1, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyHarvest5()
    {
        // 오늘의 수확: 작물 5개 수확 | 40G + 2XP
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyHarvest5.asset",
            "daily_harvest_5", QuestCategory.DailyChallenge,
            "오늘의 수확", "작물 5개를 수확하세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.Harvest, "", 5, "작물 5개 수확") },
            new[] { Reward(RewardType.Gold, 40, scaledByLevel: true),
                    Reward(RewardType.XP, 2, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyHarvest10()
    {
        // 풍성한 하루: 작물 10개 수확 | 80G + 2XP
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyHarvest10.asset",
            "daily_harvest_10", QuestCategory.DailyChallenge,
            "풍성한 하루", "작물 10개를 수확하세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.Harvest, "", 10, "작물 10개 수확") },
            new[] { Reward(RewardType.Gold, 80, scaledByLevel: true),
                    Reward(RewardType.XP, 2, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailySell()
    {
        // 출하의 날: 아이템 3개 이상 판매 | 50G + 1XP
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailySell.asset",
            "daily_sell", QuestCategory.DailyChallenge,
            "출하의 날", "아이템 3개 이상 판매하세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.Sell, "", 3, "아이템 3개 이상 판매") },
            new[] { Reward(RewardType.Gold, 50, scaledByLevel: true),
                    Reward(RewardType.XP, 1, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyEarn()
    {
        // 오늘의 매출 목표: 하루에 200G 이상 벌기 | 60G + 2XP
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyEarn.asset",
            "daily_earn", QuestCategory.DailyChallenge,
            "오늘의 매출 목표", "하루에 200G 이상 버세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.EarnGold, "", 200, "하루에 200G 이상 벌기") },
            new[] { Reward(RewardType.Gold, 60, scaledByLevel: true),
                    Reward(RewardType.XP, 2, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyTill()
    {
        // 새 땅 개간: 새 경작지 5타일 만들기 | 30G + 1XP
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyTill.asset",
            "daily_till", QuestCategory.DailyChallenge,
            "새 땅 개간", "새 경작지 5타일을 만드세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.Till, "", 5, "새 경작지 5타일 만들기") },
            new[] { Reward(RewardType.Gold, 30, scaledByLevel: true),
                    Reward(RewardType.XP, 1, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyQuality()
    {
        // 품질 사냥: Silver(1) 이상 품질 작물 3개 수확 | 60G + 2XP | 레벨 3 이상
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyQuality.asset",
            "daily_quality", QuestCategory.DailyChallenge,
            "품질 사냥", "Silver 이상 품질 작물 3개를 수확하세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.QualityHarvest, "", 3, "Silver 이상 품질 3개 수확", minQuality: 1) },
            new[] { Reward(RewardType.Gold, 60, scaledByLevel: true),
                    Reward(RewardType.XP, 2, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyProcess()
    {
        // 오늘의 가공: 가공품 2개 완성 | 50G + 2XP
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyProcess.asset",
            "daily_process", QuestCategory.DailyChallenge,
            "오늘의 가공", "가공품 2개를 완성하세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.Process, "", 2, "가공품 2개 완성") },
            new[] { Reward(RewardType.Gold, 50, scaledByLevel: true),
                    Reward(RewardType.XP, 2, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyFertilize()
    {
        // 비료 전문가: 비료 5개 사용 | 30G + 1XP
        // -> see docs/systems/quest-system.md 섹션 5.1
        // ObjectiveType에 Fertilize 없음 -> Water 로 대체(비료 사용은 farming 이벤트로 추적 예정)
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyFertilize.asset",
            "daily_fertilize", QuestCategory.DailyChallenge,
            "비료 전문가", "비료 5개를 사용하세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.Harvest, "fertilize_5", 5, "비료 5개 사용") },
            new[] { Reward(RewardType.Gold, 30, scaledByLevel: true),
                    Reward(RewardType.XP, 1, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyDiverse()
    {
        // 다양한 수확: 서로 다른 종류의 작물 3종 수확 | 70G + 2XP
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyDiverse.asset",
            "daily_diverse", QuestCategory.DailyChallenge,
            "다양한 수확", "서로 다른 종류의 작물 3종을 수확하세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.Harvest, "diverse_3", 3, "서로 다른 작물 3종 수확") },
            new[] { Reward(RewardType.Gold, 70, scaledByLevel: true),
                    Reward(RewardType.XP, 2, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyGoldQuality()
    {
        // 황금빛 수확: Gold(2) 이상 품질 작물 1개 수확 | 80G + 3XP | 레벨 5 이상
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyGoldQuality.asset",
            "daily_gold_quality", QuestCategory.DailyChallenge,
            "황금빛 수확", "Gold 이상 품질 작물 1개를 수확하세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.QualityHarvest, "", 1, "Gold 이상 품질 1개 수확", minQuality: 2) },
            new[] { Reward(RewardType.Gold, 80, scaledByLevel: true),
                    Reward(RewardType.XP, 3, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateDailyEarnLarge()
    {
        // 큰 장사: 하루에 500G 이상 벌기 | 100G + 3XP | 레벨 5 이상
        // -> see docs/systems/quest-system.md 섹션 5.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyEarnLarge.asset",
            "daily_earn_large", QuestCategory.DailyChallenge,
            "큰 장사", "하루에 500G 이상 버세요",
            timeLimitDays: 1, repeatable: true);
        SetFields(so,
            new[] { Obj(ObjectiveType.EarnGold, "", 500, "하루에 500G 이상 벌기") },
            new[] { Reward(RewardType.Gold, 100, scaledByLevel: true),
                    Reward(RewardType.XP, 3, scaledByLevel: true) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    // ── 농장 도전 ─────────────────────────────────────────────────

    private static void CreateFCFirstHarvest()
    {
        // 첫 수확의 기쁨: 첫 번째 작물 수확 | 20G + 5XP
        // -> see docs/systems/quest-system.md 섹션 6.1
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstHarvest.asset",
            "fc_first_harvest", QuestCategory.FarmChallenge,
            "첫 수확의 기쁨", "처음으로 작물을 수확하세요");
        SetFields(so,
            new[] { Obj(ObjectiveType.Harvest, "", 1, "첫 번째 작물 수확") },
            new[] { Reward(RewardType.Gold, 20), Reward(RewardType.XP, 5) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateFCEarn1000()
    {
        // 첫 1,000G: 누적 판매 수익 1,000G | 100G + 10XP
        // -> see docs/systems/quest-system.md 섹션 6.2
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCEarn1000.asset",
            "fc_earn_1000", QuestCategory.FarmChallenge,
            "첫 1,000G", "누적 판매 수익 1,000G를 달성하세요");
        SetFields(so,
            new[] { Obj(ObjectiveType.EarnGold, "", 1000, "누적 판매 수익 1,000G") },
            new[] { Reward(RewardType.Gold, 100), Reward(RewardType.XP, 10) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateFCFirstBuilding()
    {
        // 첫 번째 건축: 시설 1개 건설 | 100G + 10XP
        // -> see docs/systems/quest-system.md 섹션 6.3
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstBuilding.asset",
            "fc_first_building", QuestCategory.FarmChallenge,
            "첫 번째 건축", "시설 1개를 건설하세요");
        SetFields(so,
            new[] { Obj(ObjectiveType.Build, "", 1, "시설 1개 건설") },
            new[] { Reward(RewardType.Gold, 100), Reward(RewardType.XP, 10) },
            System.Array.Empty<QuestUnlockCondition>());
    }

    private static void CreateFCFirstProcess()
    {
        // 첫 가공품: 가공품 1개 제작 | 50G + 8XP
        // -> see docs/systems/quest-system.md 섹션 6.3
        var so = CreateAsset(
            "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstProcess.asset",
            "fc_first_process", QuestCategory.FarmChallenge,
            "첫 가공품", "가공품 1개를 제작하세요");
        SetFields(so,
            new[] { Obj(ObjectiveType.Process, "", 1, "가공품 1개 제작") },
            new[] { Reward(RewardType.Gold, 50), Reward(RewardType.XP, 8) },
            System.Array.Empty<QuestUnlockCondition>());
    }
}
