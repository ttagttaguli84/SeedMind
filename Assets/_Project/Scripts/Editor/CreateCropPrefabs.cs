using UnityEngine;
using UnityEditor;
using SeedMind.Farm.Data;

/// <summary>
/// 나머지 작물(Corn, Strawberry, Pumpkin, Sunflower, Watermelon, 겨울 3종)의
/// placeholder 프리팹과 머티리얼을 일괄 생성하고, 각 CropData SO에 연결한다.
/// -> see docs/mcp/crop-content-tasks.md Phase C, Phase D
/// </summary>
public class CreateCropPrefabs : Editor
{
    private struct CropDef
    {
        public string name;
        public string soName;
        public Color color;
        public bool hasGiant;

        public CropDef(string name, string soName, Color color, bool hasGiant = false)
        {
            this.name = name;
            this.soName = soName;
            this.color = color;
            this.hasGiant = hasGiant;
        }
    }

    private static readonly CropDef[] Crops = new CropDef[]
    {
        new CropDef("Corn",        "SO_Crop_Corn",        new Color(1.00f, 0.85f, 0.10f)),
        new CropDef("Strawberry",  "SO_Crop_Strawberry",  new Color(0.90f, 0.15f, 0.25f)),
        new CropDef("Pumpkin",     "SO_Crop_Pumpkin",     new Color(0.95f, 0.50f, 0.05f), true),
        new CropDef("Sunflower",   "SO_Crop_Sunflower",   new Color(1.00f, 0.90f, 0.00f)),
        new CropDef("Watermelon",  "SO_Crop_Watermelon",  new Color(0.10f, 0.55f, 0.15f), true),
        new CropDef("WinterRadish","SO_Crop_WinterRadish",new Color(0.95f, 0.95f, 0.95f)),
        new CropDef("Shiitake",    "SO_Crop_Shiitake",    new Color(0.40f, 0.25f, 0.10f)),
        new CropDef("Spinach",     "SO_Crop_Spinach",     new Color(0.10f, 0.50f, 0.20f)),
    };

    private static readonly float[] StageScales = { 0.1f, 0.2f, 0.3f, 0.4f };
    private const string PrefabFolder   = "Assets/_Project/Prefabs/Crops";
    private const string MaterialFolder = "Assets/_Project/Materials/Crops";
    private const string SoFolder       = "Assets/_Project/Data/Crops";

    [MenuItem("SeedMind/Create Crop Prefabs &C")]
    public static void CreateAll()
    {
        EnsureFolder(PrefabFolder);
        EnsureFolder(MaterialFolder);

        int created = 0;

        foreach (var def in Crops)
        {
            // 1. 머티리얼 생성 (이미 있으면 로드)
            string matPath = $"{MaterialFolder}/M_Crop_{def.name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = def.color;
                AssetDatabase.CreateAsset(mat, matPath);
            }

            // 2. Stage 프리팹 생성 (0~3)
            var stagePrefabs = new GameObject[4];
            for (int stage = 0; stage < 4; stage++)
            {
                string prefabName = $"PFB_Crop_{def.name}_Stage{stage}";
                string prefabPath = $"{PrefabFolder}/{prefabName}.prefab";

                // 이미 존재하면 로드만
                GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (existing != null)
                {
                    stagePrefabs[stage] = existing;
                    continue;
                }

                // 씬에 GO 생성
                var go = GameObject.CreatePrimitive(stage == 0 ? PrimitiveType.Sphere : PrimitiveType.Capsule);
                go.name = prefabName;
                float s = StageScales[stage];
                go.transform.localScale = (stage == 0)
                    ? new Vector3(s, s, s)
                    : new Vector3(s, s * 1.5f, s);

                // 머티리얼 적용
                go.GetComponent<Renderer>().sharedMaterial = mat;

                // 프리팹 저장
                var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                Object.DestroyImmediate(go);

                stagePrefabs[stage] = prefab;
                created++;
            }

            // 3. Giant 프리팹 생성 (호박, 수박)
            if (def.hasGiant)
            {
                string giantPath = $"{PrefabFolder}/PFB_Crop_{def.name}_Giant.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(giantPath) == null)
                {
                    var giant = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    giant.name = $"PFB_Crop_{def.name}_Giant";
                    giant.transform.localScale = new Vector3(3f, 3f, 3f);
                    giant.GetComponent<Renderer>().sharedMaterial = mat;
                    PrefabUtility.SaveAsPrefabAsset(giant, giantPath);
                    Object.DestroyImmediate(giant);
                    created++;
                }
            }

            // 4. CropData SO 업데이트
            string soPath = $"{SoFolder}/{def.soName}.asset";
            var cropData = AssetDatabase.LoadAssetAtPath<CropData>(soPath);
            if (cropData != null)
            {
                var so = new SerializedObject(cropData);
                var prefabsArr = so.FindProperty("growthStagePrefabs");
                bool needsUpdate = prefabsArr.arraySize == 0;
                if (needsUpdate)
                {
                    prefabsArr.arraySize = 4;
                    for (int i = 0; i < 4; i++)
                        prefabsArr.GetArrayElementAtIndex(i).objectReferenceValue = stagePrefabs[i];
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(cropData);
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SeedMind] CreateCropPrefabs: {created}개 에셋 생성 완료.");
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
