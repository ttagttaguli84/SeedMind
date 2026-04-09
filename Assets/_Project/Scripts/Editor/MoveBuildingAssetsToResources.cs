using UnityEngine;
using UnityEditor;

namespace SeedMind.Editor
{
    /// <summary>
    /// 시설 SO를 Data/Buildings에서 Resources/Data/Buildings로 이동.
    /// BuildingManager가 Resources.LoadAll로 자동 로드하기 위한 1회성 마이그레이션.
    /// </summary>
    public static class MoveBuildingAssetsToResources
    {
        private static readonly string[] _assetNames =
        {
            "SO_Bldg_WaterTank",
            "SO_Bldg_Storage",
            "SO_Bldg_Greenhouse",
            "SO_Bldg_Processor",
            "SO_Bldg_Mill",
            "SO_Bldg_Fermentation",
            "SO_Bldg_Bakery",
        };

        [MenuItem("SeedMind/Move Building SOs to Resources")]
        public static void MoveAll()
        {
            string srcFolder = "Assets/_Project/Data/Buildings";
            string dstFolder = "Assets/_Project/Resources/Data/Buildings";

            // 폴더 생성 (이미 존재하면 skip)
            EnsureFolder("Assets/_Project/Resources");
            EnsureFolder("Assets/_Project/Resources/Data");
            EnsureFolder("Assets/_Project/Resources/Data/Buildings");

            foreach (var name in _assetNames)
            {
                string srcPath = $"{srcFolder}/{name}.asset";
                string dstPath = $"{dstFolder}/{name}.asset";

                if (AssetDatabase.LoadAssetAtPath<Object>(dstPath) != null)
                {
                    Debug.Log($"[MoveBuildingAssets] {name} 이미 Resources에 존재, 스킵.");
                    continue;
                }
                if (AssetDatabase.LoadAssetAtPath<Object>(srcPath) == null)
                {
                    Debug.LogWarning($"[MoveBuildingAssets] {name} 원본 없음, 스킵.");
                    continue;
                }

                string error = AssetDatabase.MoveAsset(srcPath, dstPath);
                if (string.IsNullOrEmpty(error))
                    Debug.Log($"[MoveBuildingAssets] {name} 이동 완료.");
                else
                    Debug.LogError($"[MoveBuildingAssets] {name} 이동 실패: {error}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MoveBuildingAssets] 완료.");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                int lastSlash = path.LastIndexOf('/');
                string parent = path.Substring(0, lastSlash);
                string child = path.Substring(lastSlash + 1);
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
