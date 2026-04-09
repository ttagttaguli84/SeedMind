using UnityEngine;
using UnityEditor;

namespace SeedMind.Editor
{
    /// <summary>
    /// 작물/도구 SO를 Data/에서 Resources/Data/로 이동.
    /// DataRegistry가 Resources.LoadAll로 자동 로드하기 위한 1회성 마이그레이션.
    /// -> see docs/mcp/inventory-tasks.md T-2 Phase 3
    /// </summary>
    public static class MoveInventoryAssetsToResources
    {
        private static readonly string[] _cropNames =
        {
            "SO_Crop_Potato", "SO_Crop_Carrot", "SO_Crop_Tomato", "SO_Crop_Corn",
            "SO_Crop_Strawberry", "SO_Crop_Pumpkin", "SO_Crop_Sunflower", "SO_Crop_Watermelon",
            "SO_Crop_WinterRadish", "SO_Crop_Shiitake", "SO_Crop_Spinach"
        };

        private static readonly string[] _toolNames =
        {
            "SO_Tool_Hand", "SO_Tool_Hoe_T1", "SO_Tool_SeedBag",
            "SO_Tool_Sickle_T1", "SO_Tool_WateringCan_T1"
        };

        [MenuItem("SeedMind/Move Inventory SOs to Resources")]
        public static void MoveAll()
        {
            EnsureFolder("Assets/_Project/Resources");
            EnsureFolder("Assets/_Project/Resources/Data");
            EnsureFolder("Assets/_Project/Resources/Data/Crops");
            EnsureFolder("Assets/_Project/Resources/Data/Tools");

            MoveAssets(_cropNames, "Assets/_Project/Data/Crops", "Assets/_Project/Resources/Data/Crops");
            MoveAssets(_toolNames, "Assets/_Project/Data/Tools", "Assets/_Project/Resources/Data/Tools");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[MoveInventoryAssets] 완료.");
        }

        private static void MoveAssets(string[] names, string srcFolder, string dstFolder)
        {
            foreach (var name in names)
            {
                string srcPath = $"{srcFolder}/{name}.asset";
                string dstPath = $"{dstFolder}/{name}.asset";

                if (AssetDatabase.LoadAssetAtPath<Object>(dstPath) != null)
                {
                    Debug.Log($"[MoveInventoryAssets] {name} 이미 Resources에 존재, 스킵.");
                    continue;
                }
                if (AssetDatabase.LoadAssetAtPath<Object>(srcPath) == null)
                {
                    Debug.LogWarning($"[MoveInventoryAssets] {name} 원본 없음, 스킵.");
                    continue;
                }
                string error = AssetDatabase.MoveAsset(srcPath, dstPath);
                if (string.IsNullOrEmpty(error))
                    Debug.Log($"[MoveInventoryAssets] {name} 이동 완료.");
                else
                    Debug.LogError($"[MoveInventoryAssets] {name} 이동 실패: {error}");
            }
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                int lastSlash = path.LastIndexOf('/');
                string parent = path.Substring(0, lastSlash);
                string child  = path.Substring(lastSlash + 1);
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
