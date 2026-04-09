using SeedMind.Economy;

namespace SeedMind.Player
{
    // ── 보조 enum / struct ───────────────────────────────────────────

    /// <summary>
    /// 슬롯이 속한 인벤토리 영역.
    /// -> see docs/systems/inventory-architecture.md 섹션 3.4
    /// </summary>
    public enum SlotLocation
    {
        Backpack,   // 배낭 슬롯
        Toolbar,    // 툴바 슬롯
        Storage     // 창고 슬롯 (창고 건설 후 활성화)
    }

    /// <summary>
    /// 인벤토리 슬롯에서 발생하는 조작 유형.
    /// -> see docs/systems/inventory-architecture.md 섹션 3.5
    /// </summary>
    public enum InventoryAction
    {
        Add,
        Remove,
        Move,
        Split,
        Merge,
        Sort,
        Expand
    }

    /// <summary>
    /// AddItem 결과. remainingQuantity > 0이면 배낭이 가득 찬 것.
    /// -> see docs/systems/inventory-architecture.md 섹션 3.5
    /// </summary>
    public struct AddResult
    {
        public bool success;            // 1개라도 추가되었으면 true
        public int addedQuantity;       // 실제 추가된 수량
        public int remainingQuantity;   // 추가하지 못한 잔여 수량
    }

    /// <summary>
    /// 인벤토리 변경 이벤트 정보.
    /// -> see docs/systems/inventory-architecture.md 섹션 3.5
    /// </summary>
    public struct InventoryChangeInfo
    {
        public SlotLocation location;   // 변경된 슬롯 위치
        public int slotIndex;           // 변경된 슬롯 인덱스 (-1이면 전체 갱신)
        public string itemId;           // 관련 아이템 ID (빈 슬롯이면 null)
        public InventoryAction action;  // 변경 유형
    }

    /// <summary>
    /// 아이템 사용 결과.
    /// -> see docs/systems/inventory-architecture.md 섹션 3.5
    /// </summary>
    public struct UseResult
    {
        public bool success;
        public string message;  // 실패 사유 또는 성공 메시지 (디버그용)
    }

    // ── ItemSlot ─────────────────────────────────────────────────────

    /// <summary>
    /// 인벤토리 슬롯 하나를 표현하는 값 타입.
    /// -> see docs/systems/inventory-architecture.md 섹션 3.3 for 전체 정의
    /// </summary>
    [System.Serializable]
    public struct ItemSlot
    {
        public string itemId;           // 빈 슬롯이면 null 또는 ""
        public int quantity;            // 0이면 빈 슬롯
        public CropQuality quality;     // 작물일 때만 의미 있음 (Tool/Seed 등은 Normal 고정)
        public HarvestOrigin origin;    // 수확 출처 (Crop 타입만 의미 있음, 나머지는 Outdoor 고정)
                                        // -> see docs/systems/economy-architecture.md 섹션 3.10

        public bool IsEmpty => string.IsNullOrEmpty(itemId) || quantity <= 0;

        /// <summary>
        /// 같은 아이템으로 스택 가능한지 판정.
        /// 조건: 같은 itemId, 같은 quality, 같은 origin.
        /// </summary>
        public bool CanStackWith(ItemSlot other)
        {
            if (IsEmpty || other.IsEmpty) return false;
            return itemId == other.itemId
                && quality == other.quality
                && origin == other.origin;
        }

        /// <summary>
        /// 현재 슬롯의 잔여 스택 용량.
        /// </summary>
        public int RemainingCapacity(int maxStackSize)
        {
            return maxStackSize - quantity;
        }

        public static ItemSlot Empty => new ItemSlot
        {
            itemId = null,
            quantity = 0,
            quality = CropQuality.Normal,
            origin = HarvestOrigin.Outdoor
        };
    }
}
