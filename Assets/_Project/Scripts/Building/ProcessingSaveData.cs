using System;

namespace SeedMind.Building
{
    /// <summary>
    /// 가공 슬롯 1개의 저장 데이터.
    /// -> see docs/systems/processing-architecture.md 섹션 5.1
    /// </summary>
    [Serializable]
    public class ProcessingSlotSaveData
    {
        public ProcessingSlot.SlotState state;
        public string recipeId;
        public string inputCropId;
        public int inputQuantity;
        public float remainingHours;
        public float totalHours;
    }

    /// <summary>
    /// 가공소 1기(BuildingInstance)의 전체 저장 데이터.
    /// -> see docs/systems/processing-architecture.md 섹션 5.1
    /// </summary>
    [Serializable]
    public class ProcessingSaveData
    {
        public string buildingDataId;   // BuildingInstance.Data.dataId
        public int gridX;
        public int gridY;
        public ProcessingSlotSaveData[] slots;
    }
}
