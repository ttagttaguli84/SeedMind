using UnityEngine;
using UnityEditor;
using SeedMind.Building.Data;

namespace SeedMind.Editor
{
    /// <summary>
    /// 시설 BuildingData SO 에셋을 일괄 생성하는 Editor 스크립트.
    /// F-1 태스크: 시설 7종 SO 생성.
    /// -> see docs/mcp/facilities-tasks.md F-1
    /// -> see docs/content/facilities.md for 수치 canonical
    /// </summary>
    public static class CreateBuildingAssets
    {
        [MenuItem("SeedMind/Create Building Assets")]
        public static void CreateAll()
        {
            string folder = "Assets/_Project/Data/Buildings";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Buildings");

            // 물탱크 (-> see docs/content/facilities.md 섹션 3)
            CreateBuildingData(folder, "SO_Bldg_WaterTank",
                dataId: "building_water_tank",
                displayName: "물탱크",
                description: "인접 경작지에 매일 아침 자동으로 물을 준다.",
                buildCost: 1500,
                requiredLevel: 2,
                buildTimeDays: 2,
                tileSize: new Vector2Int(2, 2),
                effectType: BuildingEffectType.AutoWater,
                effectRadius: 3,
                effectValue: 0f,
                maxUpgradeLevel: 3,
                upgradeCosts: new int[] { 800, 1500, 3000 });

            // 창고 (-> see docs/content/facilities.md 섹션 5)
            CreateBuildingData(folder, "SO_Bldg_Storage",
                dataId: "building_storage",
                displayName: "창고",
                description: "작물을 저장하여 가격 변동에 대응할 수 있다.",
                buildCost: 2000,
                requiredLevel: 3,
                buildTimeDays: 3,
                tileSize: new Vector2Int(3, 2),
                effectType: BuildingEffectType.Storage,
                effectRadius: 0,
                effectValue: 20f,
                maxUpgradeLevel: 3,
                upgradeCosts: new int[] { 1000, 2000, 4000 });

            // 온실 (-> see docs/content/facilities.md 섹션 4)
            CreateBuildingData(folder, "SO_Bldg_Greenhouse",
                dataId: "building_greenhouse",
                displayName: "온실",
                description: "계절에 관계없이 모든 작물을 재배할 수 있다.",
                buildCost: 5000,
                requiredLevel: 5,
                buildTimeDays: 5,
                tileSize: new Vector2Int(6, 6),
                effectType: BuildingEffectType.SeasonBypass,
                effectRadius: 0,
                effectValue: 16f,
                maxUpgradeLevel: 2,
                upgradeCosts: new int[] { 3000, 6000 });

            // 가공소 (-> see docs/content/facilities.md 섹션 6)
            CreateBuildingData(folder, "SO_Bldg_Processor",
                dataId: "building_processing",
                displayName: "가공소",
                description: "작물을 가공하여 더 높은 가격에 판매할 수 있다.",
                buildCost: 3000,
                requiredLevel: 4,
                buildTimeDays: 4,
                tileSize: new Vector2Int(4, 3),
                effectType: BuildingEffectType.Processing,
                effectRadius: 0,
                effectValue: 2f,
                maxUpgradeLevel: 3,
                upgradeCosts: new int[] { 1500, 3000, 6000 });

            // 제분소 (-> see docs/content/facilities.md 섹션 7)
            CreateBuildingData(folder, "SO_Bldg_Mill",
                dataId: "building_mill",
                displayName: "제분소",
                description: "곡물과 작물을 분쇄하여 가루와 분말을 만든다.",
                buildCost: 4000,
                requiredLevel: 6,
                buildTimeDays: 4,
                tileSize: new Vector2Int(3, 3),
                effectType: BuildingEffectType.Processing,
                effectRadius: 0,
                effectValue: 2f,
                maxUpgradeLevel: 2,
                upgradeCosts: new int[] { 2000, 4000 });

            // 발효실 (-> see docs/content/facilities.md 섹션 8)
            CreateBuildingData(folder, "SO_Bldg_Fermentation",
                dataId: "building_fermentation",
                displayName: "발효실",
                description: "작물을 발효시켜 와인, 식초, 발효식품을 만든다.",
                buildCost: 4500,
                requiredLevel: 7,
                buildTimeDays: 5,
                tileSize: new Vector2Int(3, 3),
                effectType: BuildingEffectType.Processing,
                effectRadius: 0,
                effectValue: 2f,
                maxUpgradeLevel: 2,
                upgradeCosts: new int[] { 2500, 5000 });

            // 베이커리 (-> see docs/content/facilities.md 섹션 9)
            CreateBuildingData(folder, "SO_Bldg_Bakery",
                dataId: "building_bakery",
                displayName: "베이커리",
                description: "가공 중간재를 사용하여 고급 요리를 만든다. 연료(장작) 소모.",
                buildCost: 6000,
                requiredLevel: 8,
                buildTimeDays: 6,
                tileSize: new Vector2Int(4, 3),
                effectType: BuildingEffectType.Processing,
                effectRadius: 0,
                effectValue: 2f,
                maxUpgradeLevel: 2,
                upgradeCosts: new int[] { 3000, 6000 });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateBuildingAssets] 시설 SO 7종 생성 완료.");
        }

        private static void CreateBuildingData(string folder, string assetName,
            string dataId, string displayName, string description,
            int buildCost, int requiredLevel, int buildTimeDays,
            Vector2Int tileSize, BuildingEffectType effectType,
            int effectRadius, float effectValue,
            int maxUpgradeLevel, int[] upgradeCosts)
        {
            string path = $"{folder}/{assetName}.asset";
            if (AssetDatabase.LoadAssetAtPath<BuildingData>(path) != null)
            {
                Debug.Log($"[CreateBuildingAssets] {assetName} 이미 존재, 스킵.");
                return;
            }

            var so = ScriptableObject.CreateInstance<BuildingData>();
            so.dataId = dataId;
            so.displayName = displayName;
            so.description = description;
            so.buildCost = buildCost;
            so.requiredLevel = requiredLevel;
            so.buildTimeDays = buildTimeDays;
            so.tileSize = tileSize;
            so.placementRules = PlacementRule.FarmOnly;
            so.effectType = effectType;
            so.effectRadius = effectRadius;
            so.effectValue = effectValue;
            so.maxUpgradeLevel = maxUpgradeLevel;
            so.upgradeCosts = upgradeCosts;

            AssetDatabase.CreateAsset(so, path);
            Debug.Log($"[CreateBuildingAssets] {assetName} 생성 완료.");
        }
    }
}
