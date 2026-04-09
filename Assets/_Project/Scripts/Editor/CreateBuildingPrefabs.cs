using UnityEngine;
using UnityEditor;
using SeedMind.Building.Data;

namespace SeedMind.Editor
{
    /// <summary>
    /// 시설 프리팹을 일괄 생성하는 Editor 스크립트.
    /// F-3 태스크: 시설 7종 + 건설 중 공통 프리팹 생성.
    /// -> see docs/mcp/facilities-tasks.md F-3
    /// </summary>
    public static class CreateBuildingPrefabs
    {
        private static readonly (string soName, string prefabName, Vector3 scale)[] _buildings =
        {
            ("SO_Bldg_WaterTank",    "PFB_Bldg_WaterTank",    new Vector3(2f, 1.5f, 2f)),
            ("SO_Bldg_Storage",      "PFB_Bldg_Storage",      new Vector3(3f, 2f,   2f)),
            ("SO_Bldg_Greenhouse",   "PFB_Bldg_Greenhouse",   new Vector3(6f, 3f,   6f)),
            ("SO_Bldg_Processor",    "PFB_Bldg_Processor",    new Vector3(4f, 2.5f, 3f)),
            ("SO_Bldg_Mill",         "PFB_Bldg_Mill",         new Vector3(3f, 3f,   3f)),
            ("SO_Bldg_Fermentation", "PFB_Bldg_Fermentation", new Vector3(3f, 2.5f, 3f)),
            ("SO_Bldg_Bakery",       "PFB_Bldg_Bakery",       new Vector3(4f, 3f,   3f)),
        };

        [MenuItem("SeedMind/Create Building Prefabs")]
        public static void CreateAll()
        {
            string folder = "Assets/_Project/Prefabs/Buildings";
            string constructionFolder = folder + "/Construction";
            string dataFolder = "Assets/_Project/Data/Buildings";

            if (!AssetDatabase.IsValidFolder("Assets/_Project/Prefabs"))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "Buildings");
            if (!AssetDatabase.IsValidFolder(constructionFolder))
                AssetDatabase.CreateFolder(folder, "Construction");

            // 시설 프리팹 7종 생성
            foreach (var (soName, prefabName, scale) in _buildings)
            {
                string prefabPath = $"{folder}/{prefabName}.prefab";
                if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                {
                    Debug.Log($"[CreateBuildingPrefabs] {prefabName} 이미 존재, 스킵.");
                    continue;
                }

                // 루트 GO
                var root = new GameObject(prefabName);

                // 본체 Cube (placeholder)
                var model = GameObject.CreatePrimitive(PrimitiveType.Cube);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                model.transform.localScale = scale;
                model.transform.localPosition = new Vector3(0f, scale.y * 0.5f, 0f);
                // Collider는 루트에서 따로 관리하므로 모델 Collider 제거
                Object.DestroyImmediate(model.GetComponent<BoxCollider>());

                // 루트 BoxCollider (물리 충돌)
                var col = root.AddComponent<BoxCollider>();
                col.isTrigger = false;
                col.size = scale;
                col.center = new Vector3(0f, scale.y * 0.5f, 0f);

                // InteractionTrigger 자식
                var interactGO = new GameObject("InteractionTrigger");
                interactGO.transform.SetParent(root.transform, false);
                var triggerCol = interactGO.AddComponent<BoxCollider>();
                triggerCol.isTrigger = true;
                triggerCol.size = scale + new Vector3(1f, 0.5f, 1f); // 1타일 확장
                triggerCol.center = new Vector3(0f, scale.y * 0.5f, 0f);

                // SO 참조 연결 (prefab 필드 자기 자신으로)
                var so = AssetDatabase.LoadAssetAtPath<BuildingData>($"{dataFolder}/{soName}.asset");

                // 프리팹 저장
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);

                // SO prefab 필드 연결
                if (so != null)
                {
                    var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    so.prefab = savedPrefab;
                    EditorUtility.SetDirty(so);
                }

                Object.DestroyImmediate(root);
                Debug.Log($"[CreateBuildingPrefabs] {prefabName} 생성 완료.");
            }

            // 건설 중 공통 프리팹
            string constructionPrefabPath = $"{constructionFolder}/PFB_Bldg_Construction.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(constructionPrefabPath) == null)
            {
                var root = new GameObject("PFB_Bldg_Construction");
                var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
                frame.name = "FrameModel";
                frame.transform.SetParent(root.transform, false);
                frame.transform.localScale = Vector3.one; // 런타임에 tileSize로 조정
                frame.transform.localPosition = new Vector3(0f, 0.5f, 0f);

                // 반투명 머티리얼 적용 (URP/Lit 기반)
                var renderer = frame.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                    if (mat.shader.name != "Hidden/InternalErrorShader")
                    {
                        mat.SetFloat("_Surface", 1f); // Transparent
                        var color = Color.yellow;
                        color.a = 0.4f;
                        mat.color = color;
                    }
                    renderer.sharedMaterial = mat;
                }

                PrefabUtility.SaveAsPrefabAsset(root, constructionPrefabPath);

                // SO constructionPrefab 필드 연결
                var savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(constructionPrefabPath);
                foreach (var (soName, _, _) in _buildings)
                {
                    var so = AssetDatabase.LoadAssetAtPath<BuildingData>($"{dataFolder}/{soName}.asset");
                    if (so != null)
                    {
                        so.constructionPrefab = savedPrefab;
                        EditorUtility.SetDirty(so);
                    }
                }

                Object.DestroyImmediate(root);
                Debug.Log("[CreateBuildingPrefabs] PFB_Bldg_Construction 생성 완료.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateBuildingPrefabs] 시설 프리팹 생성 완료.");
        }
    }
}
