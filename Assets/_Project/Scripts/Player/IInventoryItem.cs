using UnityEngine;

namespace SeedMind
{
    /// <summary>
    /// 인벤토리에 들어갈 수 있는 모든 아이템의 공통 인터페이스.
    /// -> see docs/systems/inventory-architecture.md 섹션 3.1
    /// </summary>
    public interface IInventoryItem
    {
        string ItemId { get; }          // GameDataSO.dataId와 동일
        string ItemName { get; }        // GameDataSO.displayName과 동일
        ItemType ItemType { get; }      // 아이템 분류
        Sprite Icon { get; }            // GameDataSO.icon과 동일
        int MaxStackSize { get; }       // 최대 스택 수량
        bool Sellable { get; }          // 판매 가능 여부
    }
}
