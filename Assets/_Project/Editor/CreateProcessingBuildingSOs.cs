#if UNITY_EDITOR
// Editor 전용: 가공소 4종 BuildingData SO 일괄 생성
// -> copied from docs/content/facilities.md 섹션 6.1, 7.1, 8.1, 9.1
using UnityEditor;
using UnityEngine;
using SeedMind.Building.Data;

public static class CreateProcessingBuildingSOs
{
    [MenuItem("SeedMind/Create Processing Building SOs")]
    public static void CreateAll()
    {
        const string folder = "Assets/_Project/Data/Buildings/Processing";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Data/Buildings", "Processing");

        // 가공소(일반): 3,000G, 레벨 7, 4x3, 1일, 슬롯 1
        // -> copied from docs/content/facilities.md 섹션 6.1
        Make(folder, "SO_Building_Processing", "building_processing", "가공소",
             3000, 7, 1, 4, 3, 1f);

        // 제분소: 1,500G, 레벨 5, 3x2, 1일, 슬롯 1
        // -> copied from docs/content/facilities.md 섹션 7.1 + processing-system.md 섹션 2.3.1
        Make(folder, "SO_Building_Mill", "building_mill", "제분소",
             1500, 5, 1, 3, 2, 1f);

        // 발효실: 4,000G, 레벨 8, 3x3, 2일, 슬롯 2
        // -> copied from docs/content/facilities.md 섹션 8.1 + processing-system.md 섹션 2.3.2
        Make(folder, "SO_Building_Fermentation", "building_fermentation", "발효실",
             4000, 8, 2, 3, 3, 2f);

        // 베이커리: 5,000G, 레벨 9, 4x3, 2일, 슬롯 2
        // -> copied from docs/content/facilities.md 섹션 9.1 + processing-system.md 섹션 2.3.3
        Make(folder, "SO_Building_Bakery", "building_bakery", "베이커리",
             5000, 9, 2, 4, 3, 2f);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateProcessingBuildingSOs] 가공소 BuildingData SO 4종 생성 완료.");
    }

    private static void Make(string folder, string assetName,
        string dataId, string displayName,
        int buildCost, int requiredLevel, int buildTimeDays,
        int tileSizeX, int tileSizeY, float effectValue)
    {
        string path = $"{folder}/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<BuildingData>(path) != null)
        {
            Debug.Log($"[CreateProcessingBuildingSOs] 이미 존재, 스킵: {assetName}");
            return;
        }
        var so = ScriptableObject.CreateInstance<BuildingData>();
        so.dataId         = dataId;
        so.displayName    = displayName;
        so.buildCost      = buildCost;
        so.requiredLevel  = requiredLevel;
        so.buildTimeDays  = buildTimeDays;
        so.tileSize       = new Vector2Int(tileSizeX, tileSizeY);
        so.effectType     = BuildingEffectType.Processing;
        so.effectValue    = effectValue;
        so.maxUpgradeLevel = 3;
        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"[CreateProcessingBuildingSOs] 생성: {path}");
    }
}
#endif
