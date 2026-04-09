using System.Collections.Generic;

namespace SeedMind.Building
{
    /// <summary>
    /// 창고의 슬롯 배열을 관리하는 컨테이너.
    /// -> see docs/systems/facilities-architecture.md 섹션 6.2
    /// </summary>
    public class StorageSlotContainer
    {
        private readonly StorageSlot[] _slots;

        public IReadOnlyList<StorageSlot> Slots => _slots;
        public int MaxSlots => _slots.Length;
        public int EmptySlotCount { get; private set; }

        public StorageSlotContainer(int maxSlots)
        {
            _slots = new StorageSlot[maxSlots];
            for (int i = 0; i < maxSlots; i++)
                _slots[i] = new StorageSlot();
            EmptySlotCount = maxSlots;
        }

        public bool AddItem(string itemId, int quantity, string quality)
        {
            // 이미 같은 아이템이 있는 슬롯에 스택
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.ItemId == itemId && slot.Quality == quality)
                {
                    slot.Quantity += quantity;
                    return true;
                }
            }
            // 빈 슬롯에 추가
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    slot.ItemId = itemId;
                    slot.Quantity = quantity;
                    slot.Quality = quality;
                    EmptySlotCount--;
                    return true;
                }
            }
            return false;
        }

        public bool RemoveItem(string itemId, int quantity, out string quality)
        {
            quality = null;
            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.ItemId == itemId)
                {
                    if (slot.Quantity < quantity) return false;
                    quality = slot.Quality;
                    slot.Quantity -= quantity;
                    if (slot.Quantity <= 0)
                    {
                        slot.ItemId = null;
                        slot.Quality = null;
                        EmptySlotCount++;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
