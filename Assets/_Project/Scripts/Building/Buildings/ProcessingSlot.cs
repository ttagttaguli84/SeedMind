using SeedMind.Building.Data;

namespace SeedMind.Building
{
    /// <summary>
    /// 가공소의 개별 가공 슬롯. ProcessingSaveData와 1:1 대응.
    /// -> see docs/systems/facilities-architecture.md 섹션 7.3
    /// </summary>
    public class ProcessingSlot
    {
        public enum SlotState { Empty, Processing, Completed }

        public SlotState State { get; set; }
        public ProcessingRecipeData Recipe { get; set; }
        public string InputCropId { get; set; }
        public int InputQuantity { get; set; }
        public float RemainingHours { get; set; }
        public float TotalHours { get; set; }

        public bool IsEmpty => State == SlotState.Empty;
        public bool IsCompleted => State == SlotState.Completed;
        public float ProgressRatio => TotalHours > 0 ? 1f - (RemainingHours / TotalHours) : 0f;
    }
}
