// Editor 전용: ZoneData SO Zone A~G 7개 일괄 생성
// 구역별 수치(비용, 레벨 요건, 타일 수, 장애물 수):
//   -> copied from docs/systems/farm-expansion.md 섹션 2.1, 섹션 3.2 (DES-012 canonical)
// tilePositions Vector2Int[] 배열: 코드로 직접 할당 (MCP set_property 불안정)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SeedMind.Farm;
using System.IO;

public static class CreateZoneAssets
{
    [MenuItem("SeedMind/Create Zone Assets")]
    public static void CreateAll()
    {
        string folder = "Assets/_Project/Data/Zones";
        EnsureFolder(folder);

        // Zone A — 초기 농장 (8x8, 64타일, 오프셋 (0,0))
        // -> see docs/systems/farm-expansion.md 섹션 1.3, 2.1, 3.2
        var zoneA = MakeZone(folder + "/SO_Zone_Home.asset",
            zoneId: "zone_home",
            zoneName: "초기 농장",
            sortOrder: 0,
            requiredLevel: 0,
            unlockCost: 0,
            prereqId: "",
            zoneType: ZoneType.Farmland,
            tilePositions: MakeGrid(0, 0, 8, 8),
            obstacles: new ObstacleEntry[]
            {
                // 잡초 x5 (낫 1회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 1, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(3, 2, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(5, 1, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(2, 5, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(6, 4, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                // 소형 돌 x3 (호미 Basic 2회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 4, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(4, 6, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(6, 2, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
            });

        // Zone B — 남쪽 평야 (8x8, 64타일, 오프셋 (0,-8))
        // -> see docs/systems/farm-expansion.md 섹션 1.3, 2.1, 3.2
        var zoneB = MakeZone(folder + "/SO_Zone_SouthPlain.asset",
            zoneId: "zone_south_plain",
            zoneName: "남쪽 평야",
            sortOrder: 1,
            requiredLevel: 0,
            unlockCost: 500,
            prereqId: "zone_home",
            zoneType: ZoneType.Farmland,
            tilePositions: MakeGrid(0, -8, 8, 8),
            obstacles: new ObstacleEntry[]
            {
                // 잡초 x8 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(0, 0, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(2, 0, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(4, 1, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(6, 0, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(1, 3, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(3, 5, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(5, 4, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(7, 6, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                // 소형 돌 x5 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 1, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(3, 2, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(5, 6, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(6, 3, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(0, 7, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                // 덤불 x3 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(2, 4, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(4, 7, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(7, 2, ObstacleType.Bush, 2, new[]{"debris_bush"}),
            });

        // Zone C — 북쪽 평야 (8x8, 64타일, 오프셋 (0,8))
        // -> see docs/systems/farm-expansion.md 섹션 1.3, 2.1, 3.2
        var zoneC = MakeZone(folder + "/SO_Zone_NorthPlain.asset",
            zoneId: "zone_north_plain",
            zoneName: "북쪽 평야",
            sortOrder: 2,
            requiredLevel: 3,
            unlockCost: 1000,
            prereqId: "zone_south_plain",
            zoneType: ZoneType.Farmland,
            tilePositions: MakeGrid(0, 8, 8, 8),
            obstacles: new ObstacleEntry[]
            {
                // 그루터기 x6 (호미 Basic 3회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(0, 1, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(2, 2, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(4, 0, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(5, 4, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(7, 1, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(3, 6, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                // 소형 돌 x4 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 3, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(3, 5, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(6, 2, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(7, 7, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                // 잡초 x5 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 0, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(2, 7, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(4, 3, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(6, 5, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(0, 6, ObstacleType.Weed, 1, new[]{"debris_weed"}),
            });

        // Zone D — 동쪽 숲 (8x12, 96타일, 오프셋 (8,0))
        // -> see docs/systems/farm-expansion.md 섹션 1.3, 2.1, 3.2
        var zoneD = MakeZone(folder + "/SO_Zone_EastForest.asset",
            zoneId: "zone_east_forest",
            zoneName: "동쪽 숲",
            sortOrder: 3,
            requiredLevel: 5,
            unlockCost: 2500,
            prereqId: "zone_north_plain",
            zoneType: ZoneType.Farmland,
            tilePositions: MakeGrid(8, -4, 8, 12),
            obstacles: new ObstacleEntry[]
            {
                // 대형 나무 x4 (호미 Reinforced+ 6회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(0, 2, ObstacleType.LargeTree, 6, new[]{"debris_large_tree"}),
                MakeObs(3, 5, ObstacleType.LargeTree, 6, new[]{"debris_large_tree"}),
                MakeObs(5, 1, ObstacleType.LargeTree, 6, new[]{"debris_large_tree"}),
                MakeObs(2, 9, ObstacleType.LargeTree, 6, new[]{"debris_large_tree"}),
                // 소형 나무 x8 (호미 Basic 2회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 0, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(4, 3, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(6, 7, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(0, 6, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(7, 4, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(3, 11, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(5, 8, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(1, 10, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                // 대형 바위 x2 (호미 Reinforced+ 5회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(6, 0, ObstacleType.LargeRock, 5, new[]{"debris_large_rock"}),
                MakeObs(4, 10, ObstacleType.LargeRock, 5, new[]{"debris_large_rock"}),
                // 덤불 x6 (낫 2회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(2, 1, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(0, 4, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(7, 9, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(5, 5, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(1, 7, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(6, 11, ObstacleType.Bush, 2, new[]{"debris_bush"}),
            });

        // Zone E — 남쪽 초원 목장 (12x8, 96타일, 오프셋 (0,-16))
        // -> see docs/systems/farm-expansion.md 섹션 1.3, 2.1, 3.2
        var zoneE = MakeZone(folder + "/SO_Zone_SouthMeadow.asset",
            zoneId: "zone_south_meadow",
            zoneName: "남쪽 초원",
            sortOrder: 4,
            requiredLevel: 6,
            unlockCost: 4000,
            prereqId: "zone_north_plain",
            zoneType: ZoneType.Pasture,
            tilePositions: MakeGrid(0, -16, 12, 8),
            obstacles: new ObstacleEntry[]
            {
                // 잡초 x10 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(0, 0, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(2, 1, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(4, 2, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(6, 0, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(8, 3, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(10, 1, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(1, 5, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(5, 6, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(9, 5, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(11, 7, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                // 덤불 x8 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 2, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(3, 4, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(7, 2, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(9, 0, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(11, 4, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(0, 6, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(4, 7, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                MakeObs(8, 6, ObstacleType.Bush, 2, new[]{"debris_bush"}),
                // 소형 돌 x3 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(2, 3, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(6, 5, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(10, 7, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
            });

        // Zone F — 연못 구역 (12x8, 96타일, 오프셋 (0,16))
        // -> see docs/systems/farm-expansion.md 섹션 1.3, 2.1, 3.2
        var zoneF = MakeZone(folder + "/SO_Zone_Pond.asset",
            zoneId: "zone_pond",
            zoneName: "연못 구역",
            sortOrder: 5,
            requiredLevel: 5,
            unlockCost: 3000,
            prereqId: "zone_north_plain",
            zoneType: ZoneType.Pond,
            tilePositions: MakeGrid(0, 16, 12, 8),
            obstacles: new ObstacleEntry[]
            {
                // 잡초 x6 -> see docs/systems/farm-expansion.md 섹션 3.2
                // (연못 30타일은 tilePositions에 포함되나 개간 불가 타일로 FarmTile 상태 관리)
                MakeObs(0, 0, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(2, 1, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(10, 0, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(11, 3, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(0, 6, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(10, 5, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                // 소형 돌 x4 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 3, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(3, 6, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(9, 2, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
                MakeObs(11, 7, ObstacleType.SmallRock, 2, new[]{"debris_small_rock"}),
            });

        // Zone G — 과수원 (8x12, 96타일, 오프셋 (-8,0))
        // -> see docs/systems/farm-expansion.md 섹션 1.3, 2.1, 3.2
        var zoneG = MakeZone(folder + "/SO_Zone_Orchard.asset",
            zoneId: "zone_orchard",
            zoneName: "과수원",
            sortOrder: 6,
            requiredLevel: 7,
            unlockCost: 5000,
            prereqId: "zone_east_forest",
            zoneType: ZoneType.Orchard,
            tilePositions: MakeGrid(-8, -4, 8, 12),
            obstacles: new ObstacleEntry[]
            {
                // 그루터기 x10 (호미 Basic 3회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(0, 0, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(2, 2, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(4, 1, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(6, 4, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(1, 6, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(3, 8, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(5, 10, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(7, 7, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(0, 9, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                MakeObs(2, 11, ObstacleType.Stump, 3, new[]{"debris_stump"}),
                // 소형 나무 x6 (호미 Basic 2회) -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(1, 1, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(5, 3, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(7, 0, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(3, 5, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(6, 9, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                MakeObs(0, 11, ObstacleType.SmallTree, 2, new[]{"debris_small_tree"}),
                // 잡초 x4 -> see docs/systems/farm-expansion.md 섹션 3.2
                MakeObs(4, 6, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(2, 4, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(7, 11, ObstacleType.Weed, 1, new[]{"debris_weed"}),
                MakeObs(1, 10, ObstacleType.Weed, 1, new[]{"debris_weed"}),
            });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateZoneAssets] 7 ZoneData assets created.");
    }

    // -------------------------------------------------------------------
    // 헬퍼
    // -------------------------------------------------------------------

    static ZoneData MakeZone(string assetPath, string zoneId, string zoneName,
        int sortOrder, int requiredLevel, int unlockCost, string prereqId,
        ZoneType zoneType, Vector2Int[] tilePositions, ObstacleEntry[] obstacles)
    {
        var existing = AssetDatabase.LoadAssetAtPath<ZoneData>(assetPath);
        if (existing != null)
        {
            // 이미 존재하면 필드만 갱신
            existing.zoneId = zoneId;
            existing.zoneName = zoneName;
            existing.sortOrder = sortOrder;
            existing.requiredLevel = requiredLevel;
            existing.unlockCost = unlockCost;
            existing.prerequisiteZoneId = prereqId;
            existing.zoneType = zoneType;
            existing.tilePositions = tilePositions;
            existing.obstacleMap = obstacles;
            EditorUtility.SetDirty(existing);
            return existing;
        }

        var so = ScriptableObject.CreateInstance<ZoneData>();
        so.zoneId = zoneId;
        so.zoneName = zoneName;
        so.sortOrder = sortOrder;
        so.requiredLevel = requiredLevel;
        so.unlockCost = unlockCost;
        so.prerequisiteZoneId = prereqId;
        so.zoneType = zoneType;
        so.tilePositions = tilePositions;
        so.obstacleMap = obstacles;
        AssetDatabase.CreateAsset(so, assetPath);
        return so;
    }

    static ObstacleEntry MakeObs(int lx, int ly, ObstacleType type, int maxHP, string[] lootIds)
    {
        return new ObstacleEntry
        {
            localPosition = new Vector2Int(lx, ly),
            type = type,
            maxHP = maxHP,
            lootDropIds = lootIds,
            obstaclePrefab = null // 프리팹 참조는 Inspector에서 수동 설정
        };
    }

    /// <summary>originX, originY에서 시작하는 width x height 타일 좌표 배열 생성.</summary>
    static Vector2Int[] MakeGrid(int originX, int originY, int width, int height)
    {
        var positions = new Vector2Int[width * height];
        int idx = 0;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                positions[idx++] = new Vector2Int(originX + x, originY + y);
        return positions;
    }

    static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string name = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
