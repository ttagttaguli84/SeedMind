using System.Collections.Generic;

namespace SeedMind.Building
{
    /// <summary>
    /// 창고의 슬롯 관리 및 아이템 입출 로직을 처리하는 서브시스템.
    /// -> see docs/systems/facilities-architecture.md 섹션 6.1
    /// </summary>
    public class StorageSystem
    {
        private readonly Dictionary<BuildingInstance, StorageSlotContainer> _storages
            = new Dictionary<BuildingInstance, StorageSlotContainer>();

        public void RegisterStorage(BuildingInstance storage)
        {
            int maxSlots = (int)storage.Data.effectValue; // -> see docs/pipeline/data-pipeline.md 섹션 2.4
            if (maxSlots <= 0) maxSlots = 20; // 기본값 fallback
            _storages[storage] = new StorageSlotContainer(maxSlots);
        }

        public void UnregisterStorage(BuildingInstance storage)
        {
            _storages.Remove(storage);
        }

        public bool StoreItem(BuildingInstance storage, string itemId, int quantity, string quality)
        {
            if (!_storages.TryGetValue(storage, out var container)) return false;
            bool success = container.AddItem(itemId, quantity, quality);
            if (success) BuildingEvents.RaiseStorageChanged(storage);
            return success;
        }

        public bool RetrieveItem(BuildingInstance storage, string itemId, int quantity, out string quality)
        {
            quality = null;
            if (!_storages.TryGetValue(storage, out var container)) return false;
            bool success = container.RemoveItem(itemId, quantity, out quality);
            if (success) BuildingEvents.RaiseStorageChanged(storage);
            return success;
        }

        public IReadOnlyList<StorageSlot> GetSlots(BuildingInstance storage)
        {
            return _storages.TryGetValue(storage, out var container) ? container.Slots : null;
        }

        public int GetEmptySlotCount(BuildingInstance storage)
        {
            return _storages.TryGetValue(storage, out var container) ? container.EmptySlotCount : 0;
        }
    }
}
