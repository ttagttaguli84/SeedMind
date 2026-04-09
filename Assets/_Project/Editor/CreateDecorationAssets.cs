// Editor 스크립트: DecorationItemData SO 29종 + DecorationConfig SO 1종 일괄 생성
// 모든 itemId, buyPrice, tileSize, category는
//   -> copied from docs/content/decoration-items.md (CON-020) canonical
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SeedMind.Decoration.Data;
using SeedMind.Core;

public static class CreateDecorationAssets
{
    [MenuItem("SeedMind/Create Decoration Assets")]
    public static void CreateAll()
    {
        CreateConfig();
        CreateFenceAssets();
        CreatePathAssets();
        CreateLightAssets();
        CreateOrnamentAssets();
        CreateWaterDecorAssets();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateDecorationAssets] 29 DecorationItemData + 1 DecorationConfig assets created.");
    }

    // ── DecorationConfig ─────────────────────────────────────────────────────

    private static void CreateConfig()
    {
        var config = ScriptableObject.CreateInstance<DecorationConfig>();
        config.validHighlightColor            = new Color(0f, 1f, 0f, 0.4f);
        config.invalidHighlightColor          = new Color(1f, 0f, 0f, 0.4f);
        config.fenceDurabilityDecayPerSeason  = 10f;   // -> see docs/content/decoration-items.md 섹션 1.2
        config.fenceRepairCostRatio           = 0.20f; // -> see docs/content/decoration-items.md 섹션 1.2
        config.pathSpeedBonusEnabled          = true;
        Save(config, "Assets/_Project/Data/Config/SO_DecorationConfig.asset");
    }

    // ── Fence (4종) ───────────────────────────────────────────────────────────
    // -> copied from docs/content/decoration-items.md 섹션 1.1

    private static void CreateFenceAssets()
    {
        MakeFence("FenceWood",   "나무 울타리", 5,  0, false, Season.Spring, 100, 0.20f, "Fence/SO_Deco_FenceWood.asset");
        MakeFence("FenceStone",  "돌 울타리",  15, 3, false, Season.Spring, 0,   0f,    "Fence/SO_Deco_FenceStone.asset");
        MakeFence("FenceIron",   "쇠 울타리",  30, 6, false, Season.Spring, 0,   0f,    "Fence/SO_Deco_FenceIron.asset");
        MakeFence("FenceFloral", "꽃 울타리",  20, 5, true,  Season.Spring, 0,   0f,    "Fence/SO_Deco_FenceFloral.asset");
    }

    private static void MakeFence(string id, string displayName, int price, int level,
        bool hasSeasonLimit, Season season, int durability, float repairRatio, string subPath)
    {
        var so = ScriptableObject.CreateInstance<DecorationItemData>();
        so.itemId          = id;
        so.displayName     = displayName;
        so.category        = DecoCategoryType.Fence;
        so.buyPrice        = price;
        so.isEdgePlaced    = true;
        so.tileWidthX      = 0;
        so.tileHeightZ     = 0;
        so.unlockLevel     = level;
        so.unlockZoneId    = "";
        so.hasSeasonLimit  = hasSeasonLimit;
        so.limitedSeason   = season;
        so.durabilityMax   = durability;
        so.moveSpeedBonus  = 0f;
        so.lightRadius     = 0f;
        Save(so, $"Assets/_Project/Data/Decoration/{subPath}");
    }

    // ── Path (5종) ────────────────────────────────────────────────────────────
    // -> copied from docs/content/decoration-items.md 섹션 2.1

    private static void CreatePathAssets()
    {
        MakePath("PathDirt",   "흙 다짐 경로",  2,  0, "Path/SO_Deco_PathDirt.asset");
        MakePath("PathGravel", "자갈 경로",     5,  2, "Path/SO_Deco_PathGravel.asset");
        MakePath("PathStone",  "돌판 경로",    12,  4, "Path/SO_Deco_PathStone.asset");
        MakePath("PathBrick",  "벽돌 경로",    18,  6, "Path/SO_Deco_PathBrick.asset");
        MakePath("PathWood",   "목판 경로",    10,  3, "Path/SO_Deco_PathWood.asset");
    }

    private static void MakePath(string id, string displayName, int price, int level, string subPath)
    {
        var so = ScriptableObject.CreateInstance<DecorationItemData>();
        so.itemId          = id;
        so.displayName     = displayName;
        so.category        = DecoCategoryType.Path;
        so.buyPrice        = price;
        so.isEdgePlaced    = false;
        so.tileWidthX      = 1;
        so.tileHeightZ     = 1;
        so.unlockLevel     = level;
        so.unlockZoneId    = "";
        so.hasSeasonLimit  = false;
        so.moveSpeedBonus  = 0.1f; // +10% -> see docs/content/decoration-items.md 섹션 2.1
        so.lightRadius     = 0f;
        so.durabilityMax   = 0;
        Save(so, $"Assets/_Project/Data/Decoration/{subPath}");
    }

    // ── Light (4종) ───────────────────────────────────────────────────────────
    // -> copied from docs/content/decoration-items.md 섹션 3.1

    private static void CreateLightAssets()
    {
        MakeLight("LightTorch",   "횃불",            30,  2, 1, 1, 2f,  "Light/SO_Deco_LightTorch.asset");
        MakeLight("LightLantern", "등롱",             80,  4, 1, 1, 3f,  "Light/SO_Deco_LightLantern.asset");
        MakeLight("LightStreet",  "가로등",           200, 6, 2, 1, 5f,  "Light/SO_Deco_LightStreet.asset");
        MakeLight("LightCrystal", "마법 수정 조명",   500, 8, 1, 1, 4f,  "Light/SO_Deco_LightCrystal.asset");
    }

    private static void MakeLight(string id, string displayName, int price, int level,
        int w, int h, float radius, string subPath)
    {
        var so = ScriptableObject.CreateInstance<DecorationItemData>();
        so.itemId          = id;
        so.displayName     = displayName;
        so.category        = DecoCategoryType.Light;
        so.buyPrice        = price;
        so.isEdgePlaced    = false;
        so.tileWidthX      = w;
        so.tileHeightZ     = h;
        so.unlockLevel     = level;
        so.unlockZoneId    = "";
        so.hasSeasonLimit  = false;
        so.lightRadius     = radius; // -> see docs/content/decoration-items.md 섹션 3.1
        so.moveSpeedBonus  = 0f;
        so.durabilityMax   = 0;
        Save(so, $"Assets/_Project/Data/Decoration/{subPath}");
    }

    // ── Ornament (11종) ───────────────────────────────────────────────────────
    // -> copied from docs/content/decoration-items.md 섹션 4.1

    private static void CreateOrnamentAssets()
    {
        MakeOrna("OrnaScareRaven",    "나무 허수아비",   100, 0, 1, 1, false, Season.Spring,  "Ornament/SO_Deco_OrnaScareRaven.asset");
        MakeOrna("OrnaFlowerPotS",    "꽃 화분 (소)",    40,  2, 1, 1, true,  Season.Summer,  "Ornament/SO_Deco_OrnaFlowerPotS.asset");
        MakeOrna("OrnaFlowerPotL",    "꽃 화분 (대)",    80,  3, 1, 1, true,  Season.Summer,  "Ornament/SO_Deco_OrnaFlowerPotL.asset");
        MakeOrna("OrnaBenchWood",     "나무 벤치",       120, 3, 2, 1, false, Season.Spring,  "Ornament/SO_Deco_OrnaBenchWood.asset");
        MakeOrna("OrnaStatueStone",   "돌 조각상",       300, 5, 1, 1, false, Season.Spring,  "Ornament/SO_Deco_OrnaStatueStone.asset");
        MakeOrna("OrnaWindmillS",     "풍차 (소형)",     400, 5, 2, 2, false, Season.Spring,  "Ornament/SO_Deco_OrnaWindmillS.asset");
        MakeOrna("OrnaWellDecor",     "우물 장식",       250, 4, 2, 2, false, Season.Spring,  "Ornament/SO_Deco_OrnaWellDecor.asset");
        MakeOrna("OrnaSignBoard",     "농장 표지판",     60,  0, 1, 1, false, Season.Spring,  "Ornament/SO_Deco_OrnaSignBoard.asset");
        MakeOrna("OrnaPumpkinLantern","호박 등불",       80,  5, 1, 1, true,  Season.Autumn,  "Ornament/SO_Deco_OrnaPumpkinLantern.asset");
        MakeOrna("OrnaSnowman",       "눈사람",          50,  3, 1, 1, true,  Season.Winter,  "Ornament/SO_Deco_OrnaSnowman.asset");
        MakeOrna("OrnaStatueGold",    "황금 조각상",     2000,9, 2, 2, false, Season.Spring,  "Ornament/SO_Deco_OrnaStatueGold.asset");
    }

    private static void MakeOrna(string id, string displayName, int price, int level,
        int w, int h, bool hasSeasonLimit, Season season, string subPath)
    {
        var so = ScriptableObject.CreateInstance<DecorationItemData>();
        so.itemId          = id;
        so.displayName     = displayName;
        so.category        = DecoCategoryType.Ornament;
        so.buyPrice        = price;
        so.isEdgePlaced    = false;
        so.tileWidthX      = w;
        so.tileHeightZ     = h;
        so.unlockLevel     = level;
        so.unlockZoneId    = "";
        so.hasSeasonLimit  = hasSeasonLimit;
        so.limitedSeason   = season;
        so.lightRadius     = 0f;
        so.moveSpeedBonus  = 0f;
        so.durabilityMax   = 0;
        Save(so, $"Assets/_Project/Data/Decoration/{subPath}");
    }

    // ── WaterDecor (5종) ──────────────────────────────────────────────────────
    // -> copied from docs/content/decoration-items.md 섹션 5.1

    private static void CreateWaterDecorAssets()
    {
        MakeWater("WaterLotus",      "연꽃 군락",  150,  0, 2, 2, "zone_f", "WaterDecor/SO_Deco_WaterLotus.asset");
        MakeWater("WaterBridge",     "나무 다리",  300,  0, 1, 3, "zone_f", "WaterDecor/SO_Deco_WaterBridge.asset");
        MakeWater("WaterFountainS",  "분수 (소)", 500,   6, 2, 2, "zone_f", "WaterDecor/SO_Deco_WaterFountainS.asset");
        MakeWater("WaterFountainL",  "분수 (대)", 1200,  8, 3, 3, "zone_f", "WaterDecor/SO_Deco_WaterFountainL.asset");
        MakeWater("WaterDuck",       "오리 조각",  80,   0, 1, 1, "zone_f", "WaterDecor/SO_Deco_WaterDuck.asset");
    }

    private static void MakeWater(string id, string displayName, int price, int level,
        int w, int h, string zoneId, string subPath)
    {
        var so = ScriptableObject.CreateInstance<DecorationItemData>();
        so.itemId          = id;
        so.displayName     = displayName;
        so.category        = DecoCategoryType.WaterDecor;
        so.buyPrice        = price;
        so.isEdgePlaced    = false;
        so.tileWidthX      = w;
        so.tileHeightZ     = h;
        so.unlockLevel     = level;
        so.unlockZoneId    = zoneId; // zone_f -> see docs/content/decoration-items.md 섹션 5.1
        so.hasSeasonLimit  = false;
        so.lightRadius     = 0f;
        so.moveSpeedBonus  = 0f;
        so.durabilityMax   = 0;
        Save(so, $"Assets/_Project/Data/Decoration/{subPath}");
    }

    // ── 유틸 ─────────────────────────────────────────────────────────────────

    private static void Save(ScriptableObject so, string path)
    {
        AssetDatabase.CreateAsset(so, path);
    }
}
#endif
