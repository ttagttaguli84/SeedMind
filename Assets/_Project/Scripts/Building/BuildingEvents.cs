using System;

namespace SeedMind.Building
{
    /// <summary>
    /// 시설 시스템의 이벤트 허브.
    /// -> see docs/systems/facilities-architecture.md 섹션 8.3
    /// </summary>
    public static class BuildingEvents
    {
        // 건설 관련
        public static event Action<BuildingInstance> OnBuildingPlaced;
        public static event Action<BuildingInstance> OnBuildingCompleted;
        public static event Action<BuildingInstance, int> OnBuildingUpgraded;
        public static event Action<string> OnBuildingRemoved;

        // 가공 관련
        public static event Action<BuildingInstance, int> OnProcessingStarted;
        public static event Action<BuildingInstance, int> OnProcessingComplete;
        public static event Action<BuildingInstance, int, string> OnProcessingCollected;
        public static event Action<BuildingInstance, int, string, int> OnProcessingCancelled;

        // 창고 관련
        public static event Action<BuildingInstance> OnStorageChanged;

        internal static void RaiseBuildingPlaced(BuildingInstance inst) => OnBuildingPlaced?.Invoke(inst);
        internal static void RaiseBuildingCompleted(BuildingInstance inst) => OnBuildingCompleted?.Invoke(inst);
        internal static void RaiseBuildingUpgraded(BuildingInstance inst, int newLevel) => OnBuildingUpgraded?.Invoke(inst, newLevel);
        internal static void RaiseBuildingRemoved(string buildingId) => OnBuildingRemoved?.Invoke(buildingId);
        internal static void RaiseProcessingStarted(BuildingInstance proc, int slotIndex) => OnProcessingStarted?.Invoke(proc, slotIndex);
        internal static void RaiseProcessingComplete(BuildingInstance proc, int slotIndex) => OnProcessingComplete?.Invoke(proc, slotIndex);
        internal static void RaiseProcessingCollected(BuildingInstance proc, int slotIndex, string outputItemId) => OnProcessingCollected?.Invoke(proc, slotIndex, outputItemId);
        internal static void RaiseProcessingCancelled(BuildingInstance proc, int slotIndex, string inputCropId, int qty) => OnProcessingCancelled?.Invoke(proc, slotIndex, inputCropId, qty);
        internal static void RaiseStorageChanged(BuildingInstance storage) => OnStorageChanged?.Invoke(storage);
    }
}
