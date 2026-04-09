using SeedMind.Core;
using SeedMind.Farm.Data;
using UnityEngine;

namespace SeedMind.Building.Data
{
    /// <summary>
    /// 가공 레시피를 정의하는 ScriptableObject.
    /// GameDataSO를 상속하여 dataId, displayName, icon은 부모에서 제공.
    /// -> see docs/pipeline/data-pipeline.md 섹션 2.5 for canonical 필드 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Recipe_New", menuName = "SeedMind/Building/ProcessingRecipeData")]
    public class ProcessingRecipeData : GameDataSO, SeedMind.IInventoryItem
    {
        // --- GameDataSO 상속 필드 ---
        // public string dataId;        (부모)
        // public string displayName;   (부모)
        // public Sprite icon;          (부모)

        [Header("가공")]
        public ProcessingType processingType;
        public CropCategory inputCategory;
        public float priceMultiplier;       // -> see docs/systems/economy-system.md 섹션 2.5
        public int priceBonus;              // -> see docs/systems/economy-system.md 섹션 2.5
        public float processingTimeHours;   // -> see docs/pipeline/data-pipeline.md 섹션 2.5
        public string outputItemId;

        // ── IInventoryItem 구현 (가공 결과물로서의 인벤토리 항목) ────
        // -> see docs/systems/inventory-architecture.md 섹션 4.5
        public string ItemId => dataId;
        public string ItemName => displayName;
        public SeedMind.ItemType ItemType => SeedMind.ItemType.Processed;
        public UnityEngine.Sprite Icon => icon;
        public int MaxStackSize => 30;  // -> see docs/systems/inventory-system.md 섹션 1.1
        public bool Sellable => true;
    }
}
