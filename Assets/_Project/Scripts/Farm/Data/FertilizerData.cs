using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Farm.Data
{
    /// <summary>
    /// 비료 데이터 ScriptableObject.
    /// GameDataSO 상속, IInventoryItem 구현.
    /// -> see docs/systems/inventory-architecture.md 섹션 4.3
    /// -> see docs/systems/farming-architecture.md 섹션 4.2 for 필드 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Fertilizer", menuName = "SeedMind/Farm/FertilizerData")]
    public class FertilizerData : GameDataSO, IInventoryItem
    {
        // dataId, displayName, icon 은 GameDataSO에서 상속

        [Header("비료 효과")]
        public float qualityMultiplier;      // 품질 향상 배수 -> see docs/systems/farming-system.md
        public float growthSpeedMultiplier;  // 성장 속도 배수 -> see docs/systems/farming-system.md
        public int price;                    // 구매가 -> see docs/design.md 섹션 4.2

        // ── IInventoryItem 구현 ─────────────────────────────────────
        // -> see docs/systems/inventory-architecture.md 섹션 4.3
        public string ItemId => dataId;
        public string ItemName => displayName;
        public ItemType ItemType => SeedMind.ItemType.Fertilizer;
        public Sprite Icon => icon;
        public int MaxStackSize => 30;  // -> see docs/systems/inventory-system.md 섹션 1.1
        public bool Sellable => true;
    }
}
