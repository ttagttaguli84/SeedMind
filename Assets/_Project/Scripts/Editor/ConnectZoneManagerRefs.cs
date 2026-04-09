// Editor 전용: FarmZoneManager의 _zones 배열과 _farmGrid 참조를 자동 연결
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using SeedMind.Farm;
using System.Linq;

public static class ConnectZoneManagerRefs
{
    [MenuItem("SeedMind/Connect ZoneManager Refs")]
    public static void Connect()
    {
        var manager = Object.FindAnyObjectByType<FarmZoneManager>();
        if (manager == null)
        {
            Debug.LogError("[ConnectZoneManagerRefs] FarmZoneManager not found in scene!");
            return;
        }

        // _zones 배열: sortOrder 순서로 로드
        var guids = AssetDatabase.FindAssets("t:ZoneData", new[] { "Assets/_Project/Data/Zones" });
        var zones = guids
            .Select(g => AssetDatabase.LoadAssetAtPath<ZoneData>(AssetDatabase.GUIDToAssetPath(g)))
            .Where(z => z != null)
            .OrderBy(z => z.sortOrder)
            .ToArray();

        var so = new SerializedObject(manager);
        var zonesProp = so.FindProperty("_zones");
        zonesProp.arraySize = zones.Length;
        for (int i = 0; i < zones.Length; i++)
            zonesProp.GetArrayElementAtIndex(i).objectReferenceValue = zones[i];

        // _farmGrid 참조
        var farmGrid = Object.FindAnyObjectByType<FarmGrid>();
        if (farmGrid != null)
        {
            var gridProp = so.FindProperty("_farmGrid");
            if (gridProp != null)
                gridProp.objectReferenceValue = farmGrid;
            else
                Debug.LogWarning("[ConnectZoneManagerRefs] _farmGrid property not found.");
        }
        else
        {
            Debug.LogWarning("[ConnectZoneManagerRefs] FarmGrid not found in scene!");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);

        Debug.Log($"[ConnectZoneManagerRefs] Connected {zones.Length} zones + FarmGrid to FarmZoneManager.");
    }
}
#endif
