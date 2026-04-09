// Editor 전용: 업적 SO 에셋 일괄 생성 (T-2-ALT + T-7 통합)
// 모든 업적 데이터는 docs/systems/achievement-system.md 섹션 3의 canonical 정의를 기반으로 함
// -> copied from docs/systems/achievement-system.md 섹션 3.1~3.7, docs/content/achievements.md
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SeedMind.Achievement;
using SeedMind.Achievement.Data;

public static class CreateAchievementAssets
{
    [MenuItem("SeedMind/Create Achievement Assets")]
    public static void CreateAll()
    {
        CreateFarming();
        CreateEconomy();
        CreateFacility();
        CreateTool();
        CreateExplorer();
        CreateQuest();
        CreateHidden();
        CreateGatherer();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateAchievementAssets] 36 achievement assets created (30 base + 5 gatherer + hidden_07).");
    }

    // ──────────────────────────────────────────────────────────────
    // 헬퍼
    // ──────────────────────────────────────────────────────────────

    private static AchievementData MakeSingle(
        string path, string id, string displayName, string description,
        AchievementCategory category, AchievementConditionType condType,
        string targetId, int targetValue,
        AchievementRewardType rewardType, int rewardAmount,
        string rewardItemId, string rewardTitleId,
        bool isHidden, int sortOrder)
    {
        var so = ScriptableObject.CreateInstance<AchievementData>();
        so.achievementId  = id;
        so.displayName    = displayName;
        so.description    = description;
        so.category       = category;
        so.type           = AchievementType.Single;
        so.conditionType  = condType;
        so.targetId       = targetId ?? "";
        so.targetValue    = targetValue;
        so.rewardType     = rewardType;
        so.rewardAmount   = rewardAmount;
        so.rewardItemId   = rewardItemId ?? "";
        so.rewardTitleId  = rewardTitleId ?? "";
        so.isHidden       = isHidden;
        so.sortOrder      = sortOrder;

        AssetDatabase.CreateAsset(so, path);
        return so;
    }

    private static AchievementTierData Tier(
        string tierName, AchievementConditionType condType,
        int targetValue, AchievementRewardType rewardType, int rewardAmount,
        string rewardItemId = "", string rewardTitleId = "")
    {
        return new AchievementTierData
        {
            tierName      = tierName,
            conditionType = condType,
            targetId      = "",
            targetValue   = targetValue,
            rewardType    = rewardType,
            rewardAmount  = rewardAmount,
            rewardItemId  = rewardItemId ?? "",
            rewardTitleId = rewardTitleId ?? ""
        };
    }

    private static AchievementData MakeTiered(
        string path, string id, string displayName, string description,
        AchievementCategory category, bool isHidden, int sortOrder,
        AchievementTierData[] tiers)
    {
        var so = ScriptableObject.CreateInstance<AchievementData>();
        so.achievementId = id;
        so.displayName   = displayName;
        so.description   = description;
        so.category      = category;
        so.type          = AchievementType.Tiered;
        so.isHidden      = isHidden;
        so.sortOrder     = sortOrder;
        so.tiers         = tiers;
        so.targetId      = "";
        so.rewardItemId  = "";
        so.rewardTitleId = "";

        AssetDatabase.CreateAsset(so, path);
        return so;
    }

    // ──────────────────────────────────────────────────────────────
    // Farming (Category=0)
    // -> copied from docs/systems/achievement-system.md 섹션 3.1
    // ──────────────────────────────────────────────────────────────
    private static void CreateFarming()
    {
        const string dir = "Assets/_Project/Data/Achievements/Farming/";

        // ach_farming_01: 씨앗의 시작 (Single)
        MakeSingle(dir + "SO_Ach_Farming01.asset",
            "ach_farming_01", "씨앗의 시작", "첫 번째 작물을 수확하세요.",
            AchievementCategory.Farming, AchievementConditionType.HarvestCount,
            "", 1,
            AchievementRewardType.Title, 0, "", "title_sprout_farmer",
            false, 1);

        // ach_farming_02: 수확의 대가 (Tiered)
        MakeTiered(dir + "SO_Ach_Farming02.asset",
            "ach_farming_02", "수확의 대가", "작물을 많이 수확하세요.",
            AchievementCategory.Farming, false, 2,
            new[]
            {
                Tier("Bronze", AchievementConditionType.HarvestCount, 50,
                    AchievementRewardType.Gold, 50),
                // -> copied from docs/systems/achievement-system.md 섹션 3.1 ach_farming_02 Bronze
                Tier("Silver", AchievementConditionType.HarvestCount, 200,
                    AchievementRewardType.Gold, 150, "", "title_skilled_farmer"),
                // -> copied from docs/systems/achievement-system.md 섹션 3.1 ach_farming_02 Silver
                Tier("Gold", AchievementConditionType.HarvestCount, 1000,
                    AchievementRewardType.Item, 10, "item_compost_booster", "title_harvest_master")
                // -> copied from docs/systems/achievement-system.md 섹션 3.1 ach_farming_02 Gold
            });

        // ach_farming_03: 사계절 농부 (Single)
        MakeSingle(dir + "SO_Ach_Farming03.asset",
            "ach_farming_03", "사계절 농부", "4계절에 각 1종 이상 작물을 수확하세요.",
            AchievementCategory.Farming, AchievementConditionType.SeasonCompleted,
            "", 4,
            AchievementRewardType.Title, 0, "", "title_four_seasons",
            false, 3);

        // ach_farming_04: 작물 도감 완성 (Single)
        // targetValue=11 -> see docs/design.md 섹션 4.2 for 전체 작물 수
        MakeSingle(dir + "SO_Ach_Farming04.asset",
            "ach_farming_04", "작물 도감 완성", "게임 내 모든 작물 종류를 1개 이상 수확하세요.",
            AchievementCategory.Farming, AchievementConditionType.SpecificCropHarvested,
            "", 11,
            AchievementRewardType.Item, 1, "item_golden_seed", "title_crop_doctor",
            false, 4);

        // ach_farming_05: 품질의 끝 (Single)
        MakeSingle(dir + "SO_Ach_Farming05.asset",
            "ach_farming_05", "품질의 끝", "Iridium 품질 작물 1개를 수확하세요.",
            AchievementCategory.Farming, AchievementConditionType.QualityHarvestCount,
            "", 1,
            AchievementRewardType.Title, 0, "", "title_legendary_farmer",
            false, 5);
    }

    // ──────────────────────────────────────────────────────────────
    // Economy (Category=1)
    // -> copied from docs/systems/achievement-system.md 섹션 3.2
    // ──────────────────────────────────────────────────────────────
    private static void CreateEconomy()
    {
        const string dir = "Assets/_Project/Data/Achievements/Economy/";

        // ach_economy_01: 첫 수익 (Single)
        MakeSingle(dir + "SO_Ach_Economy01.asset",
            "ach_economy_01", "첫 수익", "첫 번째 출하/판매를 수행하세요.",
            AchievementCategory.Economy, AchievementConditionType.TotalItemsSold,
            "", 1,
            AchievementRewardType.Title, 0, "", "title_novice_merchant",
            false, 6);

        // ach_economy_02: 부의 축적 (Tiered)
        MakeTiered(dir + "SO_Ach_Economy02.asset",
            "ach_economy_02", "부의 축적", "누적 판매 수익을 쌓으세요.",
            AchievementCategory.Economy, false, 7,
            new[]
            {
                Tier("Bronze", AchievementConditionType.GoldEarned, 5000,
                    AchievementRewardType.Gold, 50),
                // -> copied from docs/systems/achievement-system.md 섹션 3.2 ach_economy_02 Bronze
                Tier("Silver", AchievementConditionType.GoldEarned, 25000,
                    AchievementRewardType.Gold, 150, "", "title_successful_farmer"),
                // -> copied from docs/systems/achievement-system.md 섹션 3.2 ach_economy_02 Silver
                Tier("Gold", AchievementConditionType.GoldEarned, 100000,
                    AchievementRewardType.Item, 1, "item_safe_decoration", "title_wealth_accumulator")
                // -> copied from docs/systems/achievement-system.md 섹션 3.2 ach_economy_02 Gold
            });

        // ach_economy_03: 대박 거래 (Single)
        MakeSingle(dir + "SO_Ach_Economy03.asset",
            "ach_economy_03", "대박 거래", "단일 출하에서 1,000G 이상 수익을 달성하세요.",
            AchievementCategory.Economy, AchievementConditionType.GoldEarned,
            "", 1000,
            AchievementRewardType.Title, 0, "", "title_trade_genius",
            false, 8);

        // ach_economy_04: 가공의 연금술 (Single)
        MakeSingle(dir + "SO_Ach_Economy04.asset",
            "ach_economy_04", "가공의 연금술", "가공품 판매로 누적 5,000G 수익을 달성하세요.",
            AchievementCategory.Economy, AchievementConditionType.GoldEarned,
            "", 5000,
            AchievementRewardType.Title, 0, "", "title_processing_master",
            false, 9);
    }

    // ──────────────────────────────────────────────────────────────
    // Facility (Category=2)
    // -> copied from docs/systems/achievement-system.md 섹션 3.3
    // ──────────────────────────────────────────────────────────────
    private static void CreateFacility()
    {
        const string dir = "Assets/_Project/Data/Achievements/Facility/";

        // ach_facility_01: 첫 건축 (Single)
        MakeSingle(dir + "SO_Ach_Facility01.asset",
            "ach_facility_01", "첫 건축", "첫 번째 시설을 건설하세요.",
            AchievementCategory.Facility, AchievementConditionType.BuildingCount,
            "", 1,
            AchievementRewardType.Title, 0, "", "title_builder_novice",
            false, 10);

        // ach_facility_02: 시설 왕국 (Single)
        MakeSingle(dir + "SO_Ach_Facility02.asset",
            "ach_facility_02", "시설 왕국", "기본 4종 시설(물탱크, 창고, 온실, 가공소)을 모두 건설하세요.",
            AchievementCategory.Facility, AchievementConditionType.SpecificBuildingBuilt,
            "", 4,
            AchievementRewardType.Title, 0, "", "title_facility_king",
            false, 11);

        // ach_facility_03: 가공 제국 (Single)
        MakeSingle(dir + "SO_Ach_Facility03.asset",
            "ach_facility_03", "가공 제국", "4종 가공소를 모두 건설하세요.",
            AchievementCategory.Facility, AchievementConditionType.SpecificBuildingBuilt,
            "", 4,
            AchievementRewardType.Item, 20, "item_premium_fuel", "title_processing_empire",
            false, 12);

        // ach_facility_04: 가공의 달인 (Tiered)
        MakeTiered(dir + "SO_Ach_Facility04.asset",
            "ach_facility_04", "가공의 달인", "누적 가공품을 제작하세요.",
            AchievementCategory.Facility, false, 13,
            new[]
            {
                Tier("Bronze", AchievementConditionType.ProcessingCount, 20,
                    AchievementRewardType.Gold, 50),
                // -> copied from docs/systems/achievement-system.md 섹션 3.3 ach_facility_04 Bronze
                Tier("Silver", AchievementConditionType.ProcessingCount, 100,
                    AchievementRewardType.Gold, 150, "", "title_processing_artisan"),
                // -> copied from docs/systems/achievement-system.md 섹션 3.3 ach_facility_04 Silver
                Tier("Gold", AchievementConditionType.ProcessingCount, 300,
                    AchievementRewardType.Item, 1, "item_special_recipe", "title_processing_master_gold")
                // -> copied from docs/systems/achievement-system.md 섹션 3.3 ach_facility_04 Gold
            });
    }

    // ──────────────────────────────────────────────────────────────
    // Tool (Category=3)
    // -> copied from docs/systems/achievement-system.md 섹션 3.4
    // ──────────────────────────────────────────────────────────────
    private static void CreateTool()
    {
        const string dir = "Assets/_Project/Data/Achievements/Tool/";

        // ach_tool_01: 첫 강화 (Single)
        MakeSingle(dir + "SO_Ach_Tool01.asset",
            "ach_tool_01", "첫 강화", "도구 1개를 Reinforced로 업그레이드하세요.",
            AchievementCategory.Tool, AchievementConditionType.ToolUpgradeCount,
            "", 1,
            AchievementRewardType.Title, 0, "", "title_apprentice_smith",
            false, 14);

        // ach_tool_02: 완벽한 도구 세트 (Single)
        MakeSingle(dir + "SO_Ach_Tool02.asset",
            "ach_tool_02", "완벽한 도구 세트", "3종 도구를 모두 Reinforced 이상 달성하세요.",
            AchievementCategory.Tool, AchievementConditionType.ToolUpgradeCount,
            "", 3,
            AchievementRewardType.Title, 0, "", "title_tool_master",
            false, 15);

        // ach_tool_03: 전설의 장비 (Single)
        // targetValue=6 = 3종 도구 × 2단계(Reinforced→Legendary)
        MakeSingle(dir + "SO_Ach_Tool03.asset",
            "ach_tool_03", "전설의 장비", "3종 도구를 모두 Legendary 달성하세요.",
            AchievementCategory.Tool, AchievementConditionType.ToolUpgradeCount,
            "", 6,
            AchievementRewardType.Item, 1, "item_hammer_decoration", "title_legendary_smith",
            false, 16);
    }

    // ──────────────────────────────────────────────────────────────
    // Explorer (Category=4)
    // -> copied from docs/systems/achievement-system.md 섹션 3.5
    // ──────────────────────────────────────────────────────────────
    private static void CreateExplorer()
    {
        const string dir = "Assets/_Project/Data/Achievements/Explorer/";

        // ach_explorer_01: 마을 인사 (Single)
        MakeSingle(dir + "SO_Ach_Explorer01.asset",
            "ach_explorer_01", "마을 인사", "4명의 NPC 모두와 첫 대화를 완료하세요.",
            AchievementCategory.Explorer, AchievementConditionType.NPCMet,
            "", 4,
            AchievementRewardType.Title, 0, "", "title_social_farmer",
            false, 17);

        // ach_explorer_02: 바람이의 단골 (Single)
        MakeSingle(dir + "SO_Ach_Explorer02.asset",
            "ach_explorer_02", "바람이의 단골", "바람이에게서 누적 5회 이상 물건을 구매하세요.",
            AchievementCategory.Explorer, AchievementConditionType.PurchaseCount,
            "merchant_baramyi", 5,
            AchievementRewardType.Item, 1, "item_discount_voucher", "title_traveler_friend",
            false, 18);

        // ach_explorer_03: 사계절의 기억 (Single)
        MakeSingle(dir + "SO_Ach_Explorer03.asset",
            "ach_explorer_03", "사계절의 기억", "4계절을 모두 경험하세요 (1년 완주).",
            AchievementCategory.Explorer, AchievementConditionType.SeasonCompleted,
            "", 4,
            AchievementRewardType.Title, 0, "", "title_season_witness",
            false, 19);

        // ach_explorer_04: 쇼핑 마니아 (Tiered)
        MakeTiered(dir + "SO_Ach_Explorer04.asset",
            "ach_explorer_04", "쇼핑 마니아", "상점에서 누적 물건을 구매하세요.",
            AchievementCategory.Explorer, false, 20,
            new[]
            {
                Tier("Bronze", AchievementConditionType.PurchaseCount, 30,
                    AchievementRewardType.Gold, 50),
                // -> copied from docs/systems/achievement-system.md 섹션 3.5 ach_explorer_04 Bronze
                Tier("Silver", AchievementConditionType.PurchaseCount, 100,
                    AchievementRewardType.Gold, 150, "", "title_shopping_lover"),
                // -> copied from docs/systems/achievement-system.md 섹션 3.5 ach_explorer_04 Silver
                Tier("Gold", AchievementConditionType.PurchaseCount, 300,
                    AchievementRewardType.Item, 1, "item_merchant_badge", "title_shopping_maniac")
                // -> copied from docs/systems/achievement-system.md 섹션 3.5 ach_explorer_04 Gold
            });
    }

    // ──────────────────────────────────────────────────────────────
    // Quest (Category=5)
    // -> copied from docs/systems/achievement-system.md 섹션 3.6
    // ──────────────────────────────────────────────────────────────
    private static void CreateQuest()
    {
        const string dir = "Assets/_Project/Data/Achievements/Quest/";

        // ach_quest_01: 첫 임무 완수 (Single)
        MakeSingle(dir + "SO_Ach_Quest01.asset",
            "ach_quest_01", "첫 임무 완수", "첫 번째 퀘스트를 완료하세요.",
            AchievementCategory.Quest, AchievementConditionType.QuestCompleted,
            "", 1,
            AchievementRewardType.Title, 0, "", "title_adventure_start",
            false, 21);

        // ach_quest_02: 퀘스트 수집가 (Tiered)
        MakeTiered(dir + "SO_Ach_Quest02.asset",
            "ach_quest_02", "퀘스트 수집가", "누적 퀘스트 완료 수를 쌓으세요.",
            AchievementCategory.Quest, false, 22,
            new[]
            {
                Tier("Bronze", AchievementConditionType.QuestCompleted, 10,
                    AchievementRewardType.Gold, 50),
                // -> copied from docs/systems/achievement-system.md 섹션 3.6 ach_quest_02 Bronze
                Tier("Silver", AchievementConditionType.QuestCompleted, 30,
                    AchievementRewardType.Gold, 150, "", "title_quest_hunter"),
                // -> copied from docs/systems/achievement-system.md 섹션 3.6 ach_quest_02 Silver
                Tier("Gold", AchievementConditionType.QuestCompleted, 100,
                    AchievementRewardType.Item, 1, "item_hero_badge", "title_quest_hero")
                // -> copied from docs/systems/achievement-system.md 섹션 3.6 ach_quest_02 Gold
            });

        // ach_quest_03: NPC의 신뢰 (Single)
        MakeSingle(dir + "SO_Ach_Quest03.asset",
            "ach_quest_03", "NPC의 신뢰", "모든 NPC의 의뢰를 각 1개 이상 완료하세요.",
            AchievementCategory.Quest, AchievementConditionType.QuestCompleted,
            "", 4,
            AchievementRewardType.Title, 0, "", "title_village_solver",
            false, 23);

        // ach_quest_04: 꾸준한 일꾼 (Single)
        MakeSingle(dir + "SO_Ach_Quest04.asset",
            "ach_quest_04", "꾸준한 일꾼", "일일 목표를 연속 7일 완료하세요.",
            AchievementCategory.Quest, AchievementConditionType.Custom,
            "", 7,
            AchievementRewardType.Title, 0, "", "title_diligent_farmer",
            false, 24);
    }

    // ──────────────────────────────────────────────────────────────
    // Hidden (Category=6)
    // -> copied from docs/systems/achievement-system.md 섹션 3.7
    // ──────────────────────────────────────────────────────────────
    private static void CreateHidden()
    {
        const string dir = "Assets/_Project/Data/Achievements/Hidden/";

        // ach_hidden_01: 비 오는 날의 수확 (Single, isHidden=true)
        MakeSingle(dir + "SO_Ach_Hidden01.asset",
            "ach_hidden_01", "비 오는 날의 수확", "비가 오는 날 작물을 수확하세요.",
            AchievementCategory.Hidden, AchievementConditionType.Custom,
            "", 10,
            AchievementRewardType.Title, 0, "", "title_rain_farmer",
            true, 25);

        // ach_hidden_02: 밤의 방랑자 (Single, isHidden=true)
        MakeSingle(dir + "SO_Ach_Hidden02.asset",
            "ach_hidden_02", "밤의 방랑자", "자정까지 잠들지 마세요.",
            AchievementCategory.Hidden, AchievementConditionType.Custom,
            "", 0,
            AchievementRewardType.Title, 0, "", "title_nocturnal_farmer",
            true, 26);

        // ach_hidden_03: 물만 주는 농부 (Single, isHidden=true)
        MakeSingle(dir + "SO_Ach_Hidden03.asset",
            "ach_hidden_03", "물만 주는 농부", "물만 주는 행동을 반복하세요.",
            AchievementCategory.Hidden, AchievementConditionType.Custom,
            "", 30,
            AchievementRewardType.Title, 0, "", "title_water_gardener",
            true, 27);

        // ach_hidden_04: 빈손의 부자 (Single, isHidden=true)
        MakeSingle(dir + "SO_Ach_Hidden04.asset",
            "ach_hidden_04", "빈손의 부자", "골드를 0으로 만든 후 500G 이상 회복하세요.",
            AchievementCategory.Hidden, AchievementConditionType.Custom,
            "", 500,
            AchievementRewardType.Title, 0, "", "title_comeback_farmer",
            true, 28);

        // ach_hidden_05: 거대 작물의 주인 (Single, isHidden=true)
        MakeSingle(dir + "SO_Ach_Hidden05.asset",
            "ach_hidden_05", "거대 작물의 주인", "거대 작물을 수확하세요.",
            AchievementCategory.Hidden, AchievementConditionType.Custom,
            "", 1,
            AchievementRewardType.Item, 1, "item_giant_seed", "title_giant_crop_owner",
            true, 29);

        // ach_hidden_06: 전부 다 팔아! (Single, isHidden=true)
        MakeSingle(dir + "SO_Ach_Hidden06.asset",
            "ach_hidden_06", "전부 다 팔아!", "인벤토리의 모든 아이템을 판매하세요.",
            AchievementCategory.Hidden, AchievementConditionType.Custom,
            "", 0,
            AchievementRewardType.Title, 0, "", "title_generous_farmer",
            true, 30);

        // ach_hidden_07: 통합 수집 마스터 (Single, isHidden=true, CON-017)
        // 연쇄 해금: ach_fish_04 AND ach_gather_03 모두 달성 시 HandleAchievementChain으로 해금
        // -> see docs/systems/achievement-architecture.md 섹션 3.2 for HandleAchievementChain 로직
        // -> see docs/content/achievements.md 섹션 7.3 for 보상 수치
        MakeSingle(dir + "SO_Ach_Hidden07.asset",
            "ach_hidden_07", "통합 수집 마스터", "낚시 도감과 채집 도감을 모두 완성하세요.",
            AchievementCategory.Hidden, AchievementConditionType.Custom,
            "", 0,
            AchievementRewardType.Item, 1, "item_dex_background_legendary", "title_collection_master",
            true, 31);
    }

    // ──────────────────────────────────────────────────────────────
    // Gatherer (Category=8) — T-7
    // -> copied from docs/content/achievements.md 섹션 9.5
    // ──────────────────────────────────────────────────────────────
    private static void CreateGatherer()
    {
        const string dir = "Assets/_Project/Data/Achievements/Gatherer/";

        // ach_gather_01: 첫 채집 (Single)
        MakeSingle(dir + "SO_Ach_Gather01.asset",
            "ach_gather_01", "첫 채집", "채집물 1개를 수집하세요.",
            AchievementCategory.Gatherer, AchievementConditionType.GatherCount,
            "", 1,
            AchievementRewardType.Title, 0, "", "title_novice_gatherer",
            false, 36);

        // ach_gather_02: 채집 애호가 (Tiered)
        MakeTiered(dir + "SO_Ach_Gather02.asset",
            "ach_gather_02", "채집 애호가", "누적 채집물을 수집하세요.",
            AchievementCategory.Gatherer, false, 37,
            new[]
            {
                Tier("Bronze", AchievementConditionType.GatherCount, 20,
                    AchievementRewardType.Gold, 50),
                // -> copied from docs/content/achievements.md 섹션 9.5 ach_gather_02 Bronze
                Tier("Silver", AchievementConditionType.GatherCount, 100,
                    AchievementRewardType.Gold, 200, "", "title_gathering_lover"),
                // -> copied from docs/content/achievements.md 섹션 9.5 ach_gather_02 Silver
                Tier("Gold", AchievementConditionType.GatherCount, 500,
                    AchievementRewardType.Item, 1, "item_gathering_xp_bonus", "title_skilled_gatherer")
                // -> copied from docs/content/achievements.md 섹션 9.5 ach_gather_02 Gold
            });

        // ach_gather_03: 채집 도감 완성 (Single)
        // targetValue=27 -> see docs/systems/gathering-system.md for 채집물 총 종류 수
        MakeSingle(dir + "SO_Ach_Gather03.asset",
            "ach_gather_03", "채집 도감 완성", "채집물 도감 27종을 모두 완성하세요.",
            AchievementCategory.Gatherer, AchievementConditionType.GatherSpeciesCollected,
            "", 27,
            AchievementRewardType.Item, 1, "item_gather_dex_decoration", "title_gathering_doctor",
            false, 38);

        // ach_gather_04: 전설의 채집가 (Single)
        MakeSingle(dir + "SO_Ach_Gather04.asset",
            "ach_gather_04", "전설의 채집가", "Legendary 채집물을 누적 5개 수집하세요.",
            AchievementCategory.Gatherer, AchievementConditionType.GatherCount,
            "", 5,
            AchievementRewardType.Title, 0, "", "title_legendary_gatherer",
            false, 39);

        // ach_gather_05: 채집 낫의 진화 (Single)
        MakeSingle(dir + "SO_Ach_Gather05.asset",
            "ach_gather_05", "채집 낫의 진화", "채집 낫을 Legendary 등급으로 업그레이드하세요.",
            AchievementCategory.Gatherer, AchievementConditionType.GatherSickleUpgraded,
            "", 1,
            AchievementRewardType.Title, 0, "", "title_sickle_master",
            false, 40);
    }
}
#endif
