using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Building.Data
{
    /// <summary>
    /// 시설(건물) 데이터를 정의하는 ScriptableObject.
    /// GameDataSO를 상속하여 dataId, displayName, icon은 부모에서 제공.
    /// -> see docs/pipeline/data-pipeline.md Part II 섹션 2.4 for canonical 필드 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Bldg_New", menuName = "SeedMind/Building/BuildingData")]
    public class BuildingData : GameDataSO
    {
        // --- GameDataSO 상속 필드 ---
        // public string dataId;        (부모에서 제공)
        // public string displayName;   (부모에서 제공)
        // public Sprite icon;          (부모에서 제공)

        [Header("설명")]
        public string description;

        [Header("건설")]
        public int buildCost;           // -> see docs/design.md 섹션 4.6
        public int requiredLevel;       // -> see docs/design.md 섹션 4.6
        public int buildTimeDays;       // -> see docs/pipeline/data-pipeline.md 섹션 2.4

        [Header("배치")]
        public Vector2Int tileSize;
        public PlacementRule placementRules;

        [Header("비주얼")]
        public GameObject prefab;
        public GameObject constructionPrefab;

        [Header("효과")]
        public BuildingEffectType effectType;
        public int effectRadius;        // -> see docs/pipeline/data-pipeline.md 섹션 2.4
        public float effectValue;       // -> see docs/pipeline/data-pipeline.md 섹션 2.4

        [Header("업그레이드")]
        public int maxUpgradeLevel;     // -> see docs/pipeline/data-pipeline.md 섹션 2.4
        public int[] upgradeCosts;      // -> see docs/pipeline/data-pipeline.md 섹션 2.4
    }
}
