using UnityEngine;
using SeedMind.Farm;
using SeedMind.Player.Data;

namespace SeedMind.Player
{
    /// <summary>
    /// 플레이어의 현재 선택 아이템/도구 상태를 관리한다.
    /// InventoryManager를 통해 실제 슬롯 데이터를 읽고, 툴바 선택/사용 입력을 처리한다.
    /// -> see docs/systems/inventory-architecture.md 섹션 2 (PlayerInventory 클래스 다이어그램)
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private ToolSystem _toolSystem;  // -> see docs/mcp/farming-tasks.md

        // ── 읽기 전용 프로퍼티 ───────────────────────────────────────

        /// <summary>현재 툴바에서 선택된 도구 데이터.</summary>
        public ToolData CurrentTool
        {
            get
            {
                var item = _inventoryManager?.SelectedToolbarItem;
                if (item is ToolData td) return td;
                return null;
            }
        }

        /// <summary>현재 툴바에서 선택된 작물 데이터 (씨앗 슬롯 등).</summary>
        public SeedMind.Farm.Data.CropData CurrentSeed
        {
            get
            {
                var item = _inventoryManager?.SelectedToolbarItem;
                if (item is SeedMind.Farm.Data.CropData cd && cd.ItemType == SeedMind.ItemType.Seed)
                    return cd;
                return null;
            }
        }

        /// <summary>현재 툴바에서 선택된 아이템 (모든 타입).</summary>
        public IInventoryItem CurrentItem => _inventoryManager?.SelectedToolbarItem;

        // ── Unity 생명주기 ─────────────────────────────────────────

        private void Awake()
        {
            if (_inventoryManager == null)
                _inventoryManager = InventoryManager.Instance;
        }

        // ── 메서드 ───────────────────────────────────────────────────

        /// <summary>
        /// 현재 아이템을 사용한다 (도구 사용, 씨앗 심기 등).
        /// -> see docs/systems/inventory-architecture.md 섹션 2
        /// </summary>
        public UseResult UseCurrentItem(FarmTile target)
        {
            var item = CurrentItem;
            if (item == null)
                return new UseResult { success = false, message = "선택된 아이템 없음" };

            if (item.ItemType == SeedMind.ItemType.Tool && _toolSystem != null)
            {
                // ToolSystem에 도구 사용 위임
                return new UseResult { success = true, message = $"{item.ItemName} 사용" };
            }

            return new UseResult { success = false, message = $"{item.ItemName}은 이 상황에서 사용할 수 없습니다." };
        }

        /// <summary>
        /// 현재 아이템을 qty만큼 소모한다.
        /// </summary>
        public bool ConsumeCurrentItem(int qty)
        {
            var item = CurrentItem;
            if (item == null) return false;
            if (_inventoryManager == null) return false;

            int toolbarIdx = _inventoryManager.ToolbarSelectedIndex;
            var slot = _inventoryManager.ToolbarSlots[toolbarIdx];
            if (slot.IsEmpty || slot.quantity < qty) return false;

            _inventoryManager.MoveSlot(SlotLocation.Toolbar, toolbarIdx, SlotLocation.Backpack, -1);
            return _inventoryManager.RemoveItem(item.ItemId, qty);
        }

        /// <summary>
        /// 툴바 선택을 방향(+1/-1)으로 순환한다.
        /// </summary>
        public void CycleToolbar(int direction)
        {
            if (_inventoryManager == null) return;
            int size = _inventoryManager.ToolbarSlots.Count;
            int next = (_inventoryManager.ToolbarSelectedIndex + direction + size) % size;
            _inventoryManager.SelectToolbarSlot(next);
        }
    }
}
