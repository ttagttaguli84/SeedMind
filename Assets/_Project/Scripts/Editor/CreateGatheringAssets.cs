// CreateGatheringAssets — 채집 시스템 SO 에셋 일괄 생성 + GatheringManager 참조 연결
// MCP G-C, G-D-04 우회: SO 배열 참조 설정 불가 → Editor 스크립트로 일괄 처리
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SeedMind.Gathering;

public static class CreateGatheringAssets
{
    private const string DataRoot   = "Assets/_Project/Data/Gathering";
    private const string PointsPath = "Assets/_Project/Data/Gathering/Points";

    // ── 메뉴 아이템 ──────────────────────────────────────────────────────

    [MenuItem("SeedMind/Gathering/Create Config SO")]
    public static void CreateConfig()
    {
        EnsureFolders();

        var configPath = $"{DataRoot}/SO_GatheringConfig_Default.asset";
        if (!AssetDatabase.LoadAssetAtPath<GatheringConfig>(configPath))
        {
            var cfg = ScriptableObject.CreateInstance<GatheringConfig>();
            AssetDatabase.CreateAsset(cfg, configPath);
            Debug.Log("[CreateGatheringAssets] SO_GatheringConfig_Default 생성 완료");
        }
        else Debug.Log("[CreateGatheringAssets] SO_GatheringConfig_Default 이미 존재");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("SeedMind/Gathering/Create Point SOs")]
    public static void CreatePointSOs()
    {
        EnsureFolders();

        // (pointId, displayName, zoneId, respawnDays)
        var points = new (string id, string name, string zone, int respawn)[]
        {
            ("gather_forest_floor",  "숲 바닥",      "zone_d", 3),
            ("gather_bush",          "덤불",          "zone_d", 2),
            ("gather_cave_entrance", "동굴 입구",     "zone_d", 5),
            ("gather_pond_edge",     "연못 가장자리", "zone_f", 3),
            ("gather_meadow",        "초원",          "zone_e", 2),
        };

        var soNames = new string[]
        {
            "SO_GatherPoint_ForestFloor",
            "SO_GatherPoint_Bush",
            "SO_GatherPoint_CaveEntrance",
            "SO_GatherPoint_PondEdge",
            "SO_GatherPoint_Meadow",
        };

        for (int i = 0; i < points.Length; i++)
        {
            var (pid, pname, zone, respawn) = points[i];
            string path = $"{PointsPath}/{soNames[i]}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<GatheringPointData>(path);
            if (existing == null)
            {
                var so = ScriptableObject.CreateInstance<GatheringPointData>();
                so.pointId      = pid;
                so.displayName  = pname;
                so.description  = $"{pname} 채집 포인트";
                so.zoneId       = zone;
                so.requiredZoneUnlocked = true;
                so.respawnDays  = respawn;
                so.respawnVariance = 1;
                AssetDatabase.CreateAsset(so, path);
                Debug.Log($"[CreateGatheringAssets] {soNames[i]} 생성 완료");
            }
            else
            {
                existing.pointId     = pid;
                existing.displayName = pname;
                existing.zoneId      = zone;
                existing.respawnDays = respawn;
                EditorUtility.SetDirty(existing);
                Debug.Log($"[CreateGatheringAssets] {soNames[i]} 업데이트");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateGatheringAssets] GatheringPointData SO 5종 처리 완료");
    }

    [MenuItem("SeedMind/Gathering/Connect GatheringManager Refs")]
    public static void ConnectManagerRefs()
    {
        // Config SO 로드
        var config = AssetDatabase.LoadAssetAtPath<GatheringConfig>(
            $"{DataRoot}/SO_GatheringConfig_Default.asset");
        if (config == null)
        {
            Debug.LogError("[ConnectGatheringManagerRefs] SO_GatheringConfig_Default 없음 — 먼저 Create Config SO 실행");
            return;
        }

        // 씬의 GatheringManager 찾기
        var manager = Object.FindObjectOfType<GatheringManager>();
        if (manager == null)
        {
            Debug.LogError("[ConnectGatheringManagerRefs] 씬에 GatheringManager 없음");
            return;
        }

        // SerializedObject로 _gatheringConfig 설정
        var so = new SerializedObject(manager);
        var cfgProp = so.FindProperty("_gatheringConfig");
        if (cfgProp != null)
        {
            cfgProp.objectReferenceValue = config;
            Debug.Log("[ConnectGatheringManagerRefs] _gatheringConfig 연결 완료");
        }
        else Debug.LogWarning("[ConnectGatheringManagerRefs] _gatheringConfig 프로퍼티 못 찾음");

        // 씬의 GatheringPoint 전체 수집
        var allPoints = Object.FindObjectsOfType<GatheringPoint>();
        var pointsProp = so.FindProperty("_gatheringPoints");
        if (pointsProp != null && allPoints.Length > 0)
        {
            pointsProp.arraySize = allPoints.Length;
            for (int i = 0; i < allPoints.Length; i++)
                pointsProp.GetArrayElementAtIndex(i).objectReferenceValue = allPoints[i];
            Debug.Log($"[ConnectGatheringManagerRefs] _gatheringPoints {allPoints.Length}개 연결 완료");
        }
        else Debug.LogWarning("[ConnectGatheringManagerRefs] GatheringPoint 없거나 프로퍼티 못 찾음");

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        Debug.Log("[ConnectGatheringManagerRefs] GatheringManager 참조 연결 완료");
    }

    [MenuItem("SeedMind/Gathering/Create All Gathering Assets")]
    public static void CreateAll()
    {
        CreateConfig();
        CreatePointSOs();
        ConnectManagerRefs();
        Debug.Log("[CreateGatheringAssets] 전체 채집 에셋 생성 + 연결 완료");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(DataRoot))
            AssetDatabase.CreateFolder("Assets/_Project/Data", "Gathering");
        if (!AssetDatabase.IsValidFolder(PointsPath))
            AssetDatabase.CreateFolder(DataRoot, "Points");
    }
}
