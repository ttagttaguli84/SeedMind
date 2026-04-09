// MaterialItemData: 업그레이드 재료 아이템 ScriptableObject
// -> see docs/systems/tool-upgrade.md 섹션 2.2
using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Player.Data
{
    /// <summary>
    /// 도구 업그레이드에 사용되는 재료 아이템 SO.
    /// 예: 철 조각(iron_scrap), 정제 강철(refined_steel)
    /// -> see docs/systems/tool-upgrade.md 섹션 2.2
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Material", menuName = "SeedMind/Player/MaterialItemData")]
    public class MaterialItemData : GameDataSO, SeedMind.IInventoryItem
    {
        [Header("아이템 속성")]
        public bool isStackable = true;
        public int maxStack = 99;
        public bool isSellable = false;
        public int baseSellPrice = 0;
        public int buyPrice;           // 대장간 구매 가격 -> see docs/systems/tool-upgrade.md 섹션 2.2

        [Header("설명")]
        public string description;

        // ── IInventoryItem 구현 ─────────────────────────────────────
        public string ItemId => dataId;
        public string ItemName => displayName;
        public SeedMind.ItemType ItemType => SeedMind.ItemType.Misc; // 재료 = Misc
        public Sprite Icon => icon;
        public int MaxStackSize => isStackable ? maxStack : 1;
        public bool Sellable => isSellable;
    }
}
