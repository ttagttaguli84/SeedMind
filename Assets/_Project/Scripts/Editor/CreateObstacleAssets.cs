// Editor 전용: 장애물 머티리얼 4종 + 프리팹 7종 일괄 생성
// -> see docs/mcp/farm-expansion-tasks.md Z-6
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class CreateObstacleAssets
{
    [MenuItem("SeedMind/Create Obstacle Assets")]
    public static void CreateAll()
    {
        CreateMaterials();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        CreatePrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateObstacleAssets] Materials 4 + Prefabs 7 created.");
    }

    static void CreateMaterials()
    {
        string folder = "Assets/_Project/Materials/Obstacles";
        EnsureFolder(folder);

        CreateMaterial(folder + "/M_Obstacle_Weed.mat",   new Color(0.2f, 0.6f, 0.1f));
        CreateMaterial(folder + "/M_Obstacle_Rock.mat",   new Color(0.5f, 0.5f, 0.5f));
        CreateMaterial(folder + "/M_Obstacle_Wood.mat",   new Color(0.5f, 0.3f, 0.1f));
        CreateMaterial(folder + "/M_Obstacle_Bush.mat",   new Color(0.1f, 0.4f, 0.05f));
    }

    static void CreateMaterial(string path, Color color)
    {
        if (AssetDatabase.LoadAssetAtPath<Material>(path) != null) return;
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
    }

    static void CreatePrefabs()
    {
        string matFolder = "Assets/_Project/Materials/Obstacles";
        string pfbFolder = "Assets/_Project/Prefabs/Obstacles";
        EnsureFolder(pfbFolder);

        var matWeed = AssetDatabase.LoadAssetAtPath<Material>(matFolder + "/M_Obstacle_Weed.mat");
        var matRock = AssetDatabase.LoadAssetAtPath<Material>(matFolder + "/M_Obstacle_Rock.mat");
        var matWood = AssetDatabase.LoadAssetAtPath<Material>(matFolder + "/M_Obstacle_Wood.mat");
        var matBush = AssetDatabase.LoadAssetAtPath<Material>(matFolder + "/M_Obstacle_Bush.mat");

        // (name, primitiveType, scale, material)
        var defs = new (string name, PrimitiveType prim, Vector3 scale, Material mat)[]
        {
            ("PFB_Obstacle_Weed",      PrimitiveType.Quad,     new Vector3(0.3f, 0.3f, 0.3f), matWeed),
            ("PFB_Obstacle_SmallRock", PrimitiveType.Sphere,   new Vector3(0.4f, 0.3f, 0.4f), matRock),
            ("PFB_Obstacle_LargeRock", PrimitiveType.Sphere,   new Vector3(0.8f, 0.6f, 0.8f), matRock),
            ("PFB_Obstacle_Stump",     PrimitiveType.Cylinder, new Vector3(0.5f, 0.3f, 0.5f), matWood),
            ("PFB_Obstacle_SmallTree", PrimitiveType.Cylinder, new Vector3(0.3f, 0.8f, 0.3f), matWood),
            ("PFB_Obstacle_LargeTree", PrimitiveType.Cylinder, new Vector3(0.5f, 1.5f, 0.5f), matWood),
            ("PFB_Obstacle_Bush",      PrimitiveType.Sphere,   new Vector3(0.5f, 0.4f, 0.5f), matBush),
        };

        foreach (var (name, prim, scale, mat) in defs)
        {
            string path = pfbFolder + "/" + name + ".prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) continue;

            var go = GameObject.CreatePrimitive(prim);
            go.name = name;
            go.transform.localScale = scale;

            if (mat != null)
            {
                var renderer = go.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = mat;
            }

            // Collider 제거 (장애물은 논리적 처리만)
            var col = go.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }
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
