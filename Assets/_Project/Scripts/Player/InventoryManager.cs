using System;
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Save;

namespace SeedMind.Player
{
    /// <summary>
    /// 플레이어 배낭/툴바 슬롯을 관리하는 싱글톤 매니저.
    /// -> see docs/systems/inventory-architecture.md 섹션 2, 5 for 알고리즘 상세
    /// </summary>
    public class InventoryManager : Singleton<InventoryManager>, ISaveable
    {
        // ── 슬롯 데이터 ─────────────────────────────────────────────
        private ItemSlot[] _backpackSlots;
        private ItemSlot[] _toolbarSlots;

        [SerializeField] private int _maxBackpackSlots = 20;  // -> see docs/systems/inventory-system.md 섹션 2.1
        [SerializeField] private int _toolbarSize = 8;        // -> see docs/systems/inventory-system.md 섹션 2.2

        private int _toolbarSelectedIndex = 0;

        // ── 읽기 전용 프로퍼티 ───────────────────────────────────────
        public IReadOnlyList<ItemSlot> BackpackSlots => _backpackSlots;
        public IReadOnlyList<ItemSlot> ToolbarSlots => _toolbarSlots;
        public int ToolbarSelectedIndex => _toolbarSelectedIndex;

        public int BackpackEmptySlotCount
        {
            get
            {
                int count = 0;
                foreach (var slot in _backpackSlots)
                    if (slot.IsEmpty) count++;
                return count;
            }
        }

        public IInventoryItem SelectedToolbarItem
        {
            get
            {
                if (_toolbarSlots == null || _toolbarSelectedIndex < 0 || _toolbarSelectedIndex >= _toolbarSlots.Length)
                    return null;
                var slot = _toolbarSlots[_toolbarSelectedIndex];
                if (slot.IsEmpty) return null;
                return DataRegistry.Instance.GetInventoryItem(slot.itemId);
            }
        }

        // ── 이벤트 ──────────────────────────────────────────────────
        public event Action<InventoryChangeInfo> OnBackpackChanged;
        public event Action<InventoryChangeInfo> OnToolbarChanged;
        public event Action<int, int> OnToolbarSelectionChanged; // (oldIndex, newIndex)
        public event Action<SlotLocation, int> OnSlotClicked;

        // ── ISaveable ───────────────────────────────────────────────
        public int SaveLoadOrder => 30;

        // ── 초기화 ───────────────────────────────────────────────────
        protected override void Awake()
        {
            base.Awake();
            InitSlots();
        }

        private void InitSlots()
        {
            _backpackSlots = new ItemSlot[_maxBackpackSlots];
            _toolbarSlots  = new ItemSlot[_toolbarSize];
            for (int i = 0; i < _backpackSlots.Length; i++) _backpackSlots[i] = ItemSlot.Empty;
            for (int i = 0; i < _toolbarSlots.Length;  i++) _toolbarSlots[i]  = ItemSlot.Empty;
        }

        // ── 배낭 연산 ────────────────────────────────────────────────

        /// <summary>
        /// 아이템을 배낭에 추가한다. 도구는 이 메서드로 추가 불가.
        /// -> see docs/systems/inventory-architecture.md 섹션 5.1
        /// </summary>
        public AddResult AddItem(string itemId, int qty,
            CropQuality quality = CropQuality.Normal,
            HarvestOrigin origin = HarvestOrigin.Outdoor)
        {
            var item = DataRegistry.Instance.GetInventoryItem(itemId);
            if (item == null)
            {
                Debug.LogWarning($"[InventoryManager] AddItem: '{itemId}' 아이템을 찾을 수 없습니다.");
                return new AddResult { success = false, addedQuantity = 0, remainingQuantity = qty };
            }
            if (item.ItemType == ItemType.Tool)
            {
                Debug.LogWarning("[InventoryManager] 도구는 SetToolbarItem으로만 추가할 수 있습니다.");
                return new AddResult { success = false, addedQuantity = 0, remainingQuantity = qty };
            }

            int maxStack   = item.MaxStackSize;
            int totalAdded = 0;
            var probe = new ItemSlot { itemId = itemId, quantity = 1, quality = quality, origin = origin };

            // 1) 기존 슬롯에 스택 시도
            for (int i = 0; i < _backpackSlots.Length && qty > 0; i++)
            {
                if (_backpackSlots[i].IsEmpty) continue;
                if (!_backpackSlots[i].CanStackWith(probe)) continue;
                int canAdd = Math.Min(qty, _backpackSlots[i].RemainingCapacity(maxStack));
                if (canAdd <= 0) continue;
                _backpackSlots[i].quantity += canAdd;
                qty        -= canAdd;
                totalAdded += canAdd;
                FireBackpackEvent(i, InventoryAction.Add, itemId);
            }

            // 2) 빈 슬롯에 새 스택 생성
            for (int i = 0; i < _backpackSlots.Length && qty > 0; i++)
            {
                if (!_backpackSlots[i].IsEmpty) continue;
                int canAdd = Math.Min(qty, maxStack);
                _backpackSlots[i] = new ItemSlot { itemId = itemId, quantity = canAdd, quality = quality, origin = origin };
                qty        -= canAdd;
                totalAdded += canAdd;
                FireBackpackEvent(i, InventoryAction.Add, itemId);
            }

            return new AddResult { success = totalAdded > 0, addedQuantity = totalAdded, remainingQuantity = qty };
        }

        /// <summary>
        /// 아이템을 배낭에서 제거한다.
        /// -> see docs/systems/inventory-architecture.md 섹션 5.2
        /// </summary>
        public bool RemoveItem(string itemId, int qty,
            CropQuality quality = CropQuality.Normal,
            HarvestOrigin origin = HarvestOrigin.Outdoor)
        {
            if (GetItemCount(itemId, quality, origin) < qty) return false;

            for (int i = _backpackSlots.Length - 1; i >= 0 && qty > 0; i--)
            {
                var slot = _backpackSlots[i];
                if (slot.IsEmpty || slot.itemId != itemId || slot.quality != quality || slot.origin != origin)
                    continue;
                int canRemove = Math.Min(qty, slot.quantity);
                _backpackSlots[i].quantity -= canRemove;
                qty -= canRemove;
                if (_backpackSlots[i].quantity <= 0) _backpackSlots[i] = ItemSlot.Empty;
                FireBackpackEvent(i, InventoryAction.Remove, itemId);
            }
            return true;
        }

        public bool HasItem(string itemId, int qty = 1)
            => GetItemCount(itemId) >= qty;

        public int GetItemCount(string itemId,
            CropQuality quality = CropQuality.Normal,
            HarvestOrigin origin = HarvestOrigin.Outdoor)
        {
            int total = 0;
            foreach (var slot in _backpackSlots)
                if (!slot.IsEmpty && slot.itemId == itemId && slot.quality == quality && slot.origin == origin)
                    total += slot.quantity;
            return total;
        }

        // ── 슬롯 조작 ────────────────────────────────────────────────

        /// <summary>
        /// 슬롯 이동/교환.
        /// -> see docs/systems/inventory-architecture.md 섹션 5.3
        /// </summary>
        public bool MoveSlot(SlotLocation srcLoc, int srcIdx, SlotLocation dstLoc, int dstIdx)
        {
            var src = ReadSlot(srcLoc, srcIdx);
            var dst = ReadSlot(dstLoc, dstIdx);

            // 도구 슬롯 제약
            if (dstLoc == SlotLocation.Toolbar)
            {
                if (src.IsEmpty) return false;
                var srcItem = DataRegistry.Instance.GetInventoryItem(src.itemId);
                if (srcItem == null || srcItem.ItemType != ItemType.Tool) return false;
            }

            if (dst.IsEmpty)
            {
                WriteSlot(dstLoc, dstIdx, src);
                WriteSlot(srcLoc, srcIdx, ItemSlot.Empty);
            }
            else if (src.CanStackWith(dst))
            {
                var srcItem = DataRegistry.Instance.GetInventoryItem(src.itemId);
                int maxStack = srcItem?.MaxStackSize ?? 99;
                int canMerge = Math.Min(src.quantity, dst.RemainingCapacity(maxStack));
                AddQuantity(dstLoc, dstIdx, canMerge);
                AddQuantity(srcLoc, srcIdx, -canMerge);
                if (ReadSlot(srcLoc, srcIdx).quantity <= 0) WriteSlot(srcLoc, srcIdx, ItemSlot.Empty);
            }
            else
            {
                WriteSlot(srcLoc, srcIdx, dst);
                WriteSlot(dstLoc, dstIdx, src);
            }

            FireEvent(srcLoc, srcIdx, InventoryAction.Move, null);
            FireEvent(dstLoc, dstIdx, InventoryAction.Move, null);
            return true;
        }

        public bool SplitSlot(SlotLocation loc, int idx, int splitQty)
        {
            var slot = ReadSlot(loc, idx);
            if (slot.IsEmpty || slot.quantity <= splitQty) return false;
            if (BackpackEmptySlotCount <= 0) return false;
            AddQuantity(loc, idx, -splitQty);
            AddItem(slot.itemId, splitQty, slot.quality, slot.origin);
            FireEvent(loc, idx, InventoryAction.Split, slot.itemId);
            return true;
        }

        /// <summary>
        /// 배낭 정렬.
        /// -> see docs/systems/inventory-architecture.md 섹션 5.4
        /// </summary>
        public void SortBackpack()
        {
            // 스택 합산
            var counts = new Dictionary<(string id, CropQuality q, HarvestOrigin o), int>();
            foreach (var slot in _backpackSlots)
            {
                if (slot.IsEmpty) continue;
                var key = (slot.itemId, slot.quality, slot.origin);
                counts[key] = counts.TryGetValue(key, out int v) ? v + slot.quantity : slot.quantity;
            }

            // 정렬 리스트 구성
            var sorted = new List<(string id, CropQuality q, HarvestOrigin o, int qty)>();
            foreach (var kv in counts)
                sorted.Add((kv.Key.id, kv.Key.q, kv.Key.o, kv.Value));

            sorted.Sort((a, b) =>
            {
                var ia = DataRegistry.Instance.GetInventoryItem(a.id);
                var ib = DataRegistry.Instance.GetInventoryItem(b.id);
                int tc = ((int)(ia?.ItemType ?? 0)).CompareTo((int)(ib?.ItemType ?? 0));
                if (tc != 0) return tc;
                int ic = string.Compare(a.id, b.id, StringComparison.Ordinal);
                if (ic != 0) return ic;
                int oc = a.o.CompareTo(b.o);
                if (oc != 0) return oc;
                return b.q.CompareTo(a.q); // 역순 (Gold 우선)
            });

            // 슬롯 재배치
            for (int i = 0; i < _backpackSlots.Length; i++) _backpackSlots[i] = ItemSlot.Empty;
            int slotIdx = 0;
            foreach (var entry in sorted)
            {
                var item = DataRegistry.Instance.GetInventoryItem(entry.id);
                int maxStack = item?.MaxStackSize ?? 99;
                int remaining = entry.qty;
                while (remaining > 0 && slotIdx < _backpackSlots.Length)
                {
                    int place = Math.Min(remaining, maxStack);
                    _backpackSlots[slotIdx] = new ItemSlot { itemId = entry.id, quantity = place, quality = entry.q, origin = entry.o };
                    remaining -= place;
                    slotIdx++;
                }
            }

            FireBackpackEvent(-1, InventoryAction.Sort, null);
        }

        public void ExpandBackpack(int additionalSlots)
        {
            int oldLen = _backpackSlots.Length;
            var newSlots = new ItemSlot[oldLen + additionalSlots];
            Array.Copy(_backpackSlots, newSlots, oldLen);
            for (int i = oldLen; i < newSlots.Length; i++) newSlots[i] = ItemSlot.Empty;
            _backpackSlots = newSlots;
            _maxBackpackSlots = newSlots.Length;
            FireBackpackEvent(-1, InventoryAction.Expand, null);
        }

        // ── 툴바 연산 ────────────────────────────────────────────────

        public void SelectToolbarSlot(int index)
        {
            if (index < 0 || index >= _toolbarSlots.Length) return;
            int old = _toolbarSelectedIndex;
            _toolbarSelectedIndex = index;
            OnToolbarSelectionChanged?.Invoke(old, index);
        }

        public bool SetToolbarItem(int index, string toolId)
        {
            if (index < 0 || index >= _toolbarSlots.Length) return false;
            var item = DataRegistry.Instance.GetInventoryItem(toolId);
            if (item == null || item.ItemType != ItemType.Tool) return false;
            _toolbarSlots[index] = new ItemSlot { itemId = toolId, quantity = 1, quality = CropQuality.Normal, origin = HarvestOrigin.Outdoor };
            FireToolbarEvent(index, InventoryAction.Add, toolId);
            return true;
        }

        // ── ISaveable ────────────────────────────────────────────────

        public object GetSaveData()
        {
            // -> see docs/systems/inventory-architecture.md 섹션 6.2
            var slots = new List<object>();
            foreach (var slot in _backpackSlots)
            {
                if (slot.IsEmpty) continue;
                slots.Add(new { itemId = slot.itemId, quantity = slot.quantity, quality = slot.quality.ToString(), origin = slot.origin.ToString() });
            }
            var toolSlots = new List<object>();
            foreach (var slot in _toolbarSlots)
                toolSlots.Add(new { itemId = slot.itemId ?? "", quantity = slot.quantity });
            return new { slots, maxSlots = _maxBackpackSlots, toolbarSlots = toolSlots, toolbarSelectedIndex = _toolbarSelectedIndex };
        }

        public void LoadSaveData(object data)
        {
            // -> see docs/systems/inventory-architecture.md 섹션 6.3
            // SaveManager 연동 시 완성 예정
            Debug.Log("[InventoryManager] LoadSaveData called (SaveManager 연동 후 완성 예정)");
        }

        // ── 내부 헬퍼 ────────────────────────────────────────────────

        private ItemSlot ReadSlot(SlotLocation loc, int idx)
        {
            if (loc == SlotLocation.Backpack && idx >= 0 && idx < _backpackSlots.Length)
                return _backpackSlots[idx];
            if (loc == SlotLocation.Toolbar && idx >= 0 && idx < _toolbarSlots.Length)
                return _toolbarSlots[idx];
            return ItemSlot.Empty;
        }

        private void WriteSlot(SlotLocation loc, int idx, ItemSlot slot)
        {
            if (loc == SlotLocation.Backpack && idx >= 0 && idx < _backpackSlots.Length)
                _backpackSlots[idx] = slot;
            else if (loc == SlotLocation.Toolbar && idx >= 0 && idx < _toolbarSlots.Length)
                _toolbarSlots[idx] = slot;
        }

        private void AddQuantity(SlotLocation loc, int idx, int delta)
        {
            if (loc == SlotLocation.Backpack && idx >= 0 && idx < _backpackSlots.Length)
                _backpackSlots[idx].quantity += delta;
            else if (loc == SlotLocation.Toolbar && idx >= 0 && idx < _toolbarSlots.Length)
                _toolbarSlots[idx].quantity += delta;
        }

        private void FireBackpackEvent(int idx, InventoryAction action, string itemId)
        {
            OnBackpackChanged?.Invoke(new InventoryChangeInfo
            {
                location = SlotLocation.Backpack, slotIndex = idx, itemId = itemId, action = action
            });
        }

        private void FireToolbarEvent(int idx, InventoryAction action, string itemId)
        {
            OnToolbarChanged?.Invoke(new InventoryChangeInfo
            {
                location = SlotLocation.Toolbar, slotIndex = idx, itemId = itemId, action = action
            });
        }

        private void FireEvent(SlotLocation loc, int idx, InventoryAction action, string itemId)
        {
            if (loc == SlotLocation.Backpack) FireBackpackEvent(idx, action, itemId);
            else if (loc == SlotLocation.Toolbar) FireToolbarEvent(idx, action, itemId);
        }
    }
}
