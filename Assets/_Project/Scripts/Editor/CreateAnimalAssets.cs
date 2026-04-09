// CreateAnimalAssets — Editor 전용: AnimalData SO 4종 + LivestockConfig + FeedItem SO 4종 일괄 생성
// -> see docs/content/livestock-system.md 섹션 1.1, 2.2, 3.1
using UnityEngine;
using UnityEditor;
using SeedMind.Livestock;
using SeedMind.Livestock.Data;
using SeedMind.Economy;
using SeedMind.Player.Data;

namespace SeedMind.Editor
{
    public static class CreateAnimalAssets
    {
        private const string AnimalPath   = "Assets/_Project/Data/Livestock/Animals";
        private const string ConfigPath   = "Assets/_Project/Data/Livestock";
        private const string FeedItemPath = "Assets/_Project/Data/Items/FeedItems";

        [MenuItem("SeedMind/Livestock/Create All Animal Assets")]
        public static void CreateAll()
        {
            System.IO.Directory.CreateDirectory(AnimalPath);
            System.IO.Directory.CreateDirectory(ConfigPath);
            System.IO.Directory.CreateDirectory(FeedItemPath);

            CreateAnimalSOs();
            CreateLivestockConfigSO();
            CreateFeedItemSOs();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateAnimalAssets] 전체 축산 에셋 생성 완료.");
        }

        // ────────────────────────────────────────────────
        // Animal Data SOs
        // ────────────────────────────────────────────────

        private static void CreateAnimalSOs()
        {
            CreateAnimalSO(
                soName:          "SO_Animal_Chicken",
                animalId:        "animal_chicken",
                animalName:      "닭",
                animalType:      AnimalType.Poultry,
                purchasePrice:   800,
                unlockLevel:     1,
                requiredFeedId:  "item_poultry_feed",
                dailyFeedAmount: 1,
                productItemId:   "item_egg",
                productionIntervalDays: 1,
                baseProductAmount: 1,
                baseHappinessDecay: 0f,
                feedHappinessGain: 3f,
                petHappinessGain: 5f
            );

            CreateAnimalSO(
                soName:          "SO_Animal_Goat",
                animalId:        "animal_goat",
                animalName:      "염소",
                animalType:      AnimalType.SmallAnimal,
                purchasePrice:   2000,
                unlockLevel:     3,
                requiredFeedId:  "item_hay",
                dailyFeedAmount: 1,
                productItemId:   "item_goat_milk",
                productionIntervalDays: 2,
                baseProductAmount: 1,
                baseHappinessDecay: 0f,
                feedHappinessGain: 3f,
                petHappinessGain: 5f
            );

            CreateAnimalSO(
                soName:          "SO_Animal_Cow",
                animalId:        "animal_cow",
                animalName:      "소",
                animalType:      AnimalType.Cattle,
                purchasePrice:   4000,
                unlockLevel:     5,
                requiredFeedId:  "item_premium_hay",
                dailyFeedAmount: 2,
                productItemId:   "item_milk",
                productionIntervalDays: 2,
                baseProductAmount: 2,
                baseHappinessDecay: 0f,
                feedHappinessGain: 3f,
                petHappinessGain: 5f
            );

            CreateAnimalSO(
                soName:          "SO_Animal_Sheep",
                animalId:        "animal_sheep",
                animalName:      "양",
                animalType:      AnimalType.SmallAnimal,
                purchasePrice:   3000,
                unlockLevel:     4,
                requiredFeedId:  "item_pasture_grass",
                dailyFeedAmount: 1,
                productItemId:   "item_wool",
                productionIntervalDays: 3,
                baseProductAmount: 1,
                baseHappinessDecay: 0f,
                feedHappinessGain: 3f,
                petHappinessGain: 5f
            );
        }

        private static void CreateAnimalSO(
            string soName, string animalId, string animalName,
            AnimalType animalType, int purchasePrice, int unlockLevel,
            string requiredFeedId, int dailyFeedAmount,
            string productItemId, int productionIntervalDays, int baseProductAmount,
            float baseHappinessDecay, float feedHappinessGain, float petHappinessGain)
        {
            string path = $"{AnimalPath}/{soName}.asset";
            if (AssetDatabase.LoadAssetAtPath<AnimalData>(path) != null) return;

            var so = ScriptableObject.CreateInstance<AnimalData>();
            so.animalId               = animalId;
            so.animalName             = animalName;
            so.animalType             = animalType;
            so.purchasePrice          = purchasePrice;
            so.unlockLevel            = unlockLevel;
            so.requiredFeedId         = requiredFeedId;
            so.dailyFeedAmount        = dailyFeedAmount;
            so.productItemId          = productItemId;
            so.productionIntervalDays = productionIntervalDays;
            so.baseProductAmount      = baseProductAmount;
            so.baseHappinessDecay     = baseHappinessDecay;
            so.feedHappinessGain      = feedHappinessGain;
            so.petHappinessGain       = petHappinessGain;

            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[CreateAnimalAssets] 생성: {path}");
        }

        // ────────────────────────────────────────────────
        // LivestockConfig SO
        // ────────────────────────────────────────────────

        private static void CreateLivestockConfigSO()
        {
            string path = $"{ConfigPath}/SO_LivestockConfig.asset";
            if (AssetDatabase.LoadAssetAtPath<LivestockConfig>(path) != null) return;

            var cfg = ScriptableObject.CreateInstance<LivestockConfig>();
            // Coop (닭장)
            cfg.initialCoopCapacity  = 4;
            cfg.coopUpgradeCapacity  = new int[] { 8 };
            cfg.coopUpgradeCost      = new int[] { 3000 };
            // Barn (축사)
            cfg.initialBarnCapacity  = 4;
            cfg.barnUpgradeCapacity  = new int[] { 8, 12 };
            cfg.barnUpgradeCost      = new int[] { 5000, 8000 };
            // Quality thresholds -> see docs/content/livestock-system.md 섹션 5
            cfg.silverQualityThreshold = 150f;
            cfg.goldQualityThreshold   = 175f;
            // Neglect
            cfg.neglectThresholdDays   = 1;
            cfg.neglectPenaltyPerDay   = 10f;
            // Initial
            cfg.initialHappiness = 100f;
            // Production curve: happiness 0→0.5x, 100→1.0x, 200→1.5x
            cfg.productionMultiplierCurve = AnimationCurve.Linear(0f, 0.5f, 200f, 1.5f);

            AssetDatabase.CreateAsset(cfg, path);
            Debug.Log($"[CreateAnimalAssets] 생성: {path}");
        }

        // ────────────────────────────────────────────────
        // Feed Item SOs
        // ────────────────────────────────────────────────

        private static void CreateFeedItemSOs()
        {
            // 사료 아이템은 MaterialItemData SO 타입 재사용 (-> see docs/content/livestock-system.md 섹션 2.2)
            CreateFeedItemSO("SO_Item_PoultryFeed",  "item_poultry_feed",  "모이",        10);
            CreateFeedItemSO("SO_Item_Hay",          "item_hay",           "건초",        20);
            CreateFeedItemSO("SO_Item_PremiumHay",   "item_premium_hay",   "프리미엄건초", 30);
            CreateFeedItemSO("SO_Item_PastureGrass", "item_pasture_grass", "목초",        25);
        }

        private static void CreateFeedItemSO(string soName, string itemId, string displayName, int buyPrice)
        {
            string path = $"{FeedItemPath}/{soName}.asset";
            if (AssetDatabase.LoadAssetAtPath<MaterialItemData>(path) != null) return;

            var so = ScriptableObject.CreateInstance<MaterialItemData>();
            so.dataId        = itemId;
            so.displayName   = displayName;
            so.buyPrice      = buyPrice;
            so.isStackable   = true;
            so.maxStack      = 99;
            so.isSellable    = false;
            so.baseSellPrice = 0;

            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[CreateAnimalAssets] 생성: {path}");
        }
    }
}
