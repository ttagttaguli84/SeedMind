using System.Collections.Generic;
using SeedMind.Building.Data;

namespace SeedMind.Building
{
    /// <summary>
    /// 가공소의 레시피 처리, 큐 관리, 시간 경과 처리를 담당하는 서브시스템.
    /// -> see docs/systems/facilities-architecture.md 섹션 7.2
    /// </summary>
    public class ProcessingSystem
    {
        private readonly Dictionary<BuildingInstance, ProcessingSlot[]> _processors
            = new Dictionary<BuildingInstance, ProcessingSlot[]>();

        public void RegisterProcessor(BuildingInstance processor)
        {
            int slotCount = (int)processor.Data.effectValue; // -> see docs/pipeline/data-pipeline.md 섹션 2.4
            if (slotCount <= 0) slotCount = 1;
            var slots = new ProcessingSlot[slotCount];
            for (int i = 0; i < slotCount; i++)
                slots[i] = new ProcessingSlot();
            _processors[processor] = slots;
        }

        public void UnregisterProcessor(BuildingInstance processor)
        {
            _processors.Remove(processor);
        }

        public bool StartProcessing(BuildingInstance processor,
                                    ProcessingRecipeData recipe,
                                    string inputCropId,
                                    int inputQuantity)
        {
            if (!_processors.TryGetValue(processor, out var slots)) return false;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].IsEmpty)
                {
                    slots[i].Recipe = recipe;
                    slots[i].InputCropId = inputCropId;
                    slots[i].InputQuantity = inputQuantity;
                    slots[i].TotalHours = recipe.processingTimeHours;
                    slots[i].RemainingHours = recipe.processingTimeHours;
                    slots[i].State = ProcessingSlot.SlotState.Processing;
                    BuildingEvents.RaiseProcessingStarted(processor, i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 시간 경과에 따른 가공 진행 처리. TimeManager.OnHourChanged에서 호출.
        /// </summary>
        public void ProcessTimeAdvance(float elapsedHours)
        {
            foreach (var kv in _processors)
            {
                var processor = kv.Key;
                var slots = kv.Value;
                for (int i = 0; i < slots.Length; i++)
                {
                    if (slots[i].State != ProcessingSlot.SlotState.Processing) continue;
                    slots[i].RemainingHours -= elapsedHours;
                    if (slots[i].RemainingHours <= 0f)
                    {
                        slots[i].RemainingHours = 0f;
                        slots[i].State = ProcessingSlot.SlotState.Completed;
                        BuildingEvents.RaiseProcessingComplete(processor, i);
                    }
                }
            }
        }

        public bool CollectOutput(BuildingInstance processor, int slotIndex,
                                   out string outputItemId, out int quantity)
        {
            outputItemId = null;
            quantity = 0;
            if (!_processors.TryGetValue(processor, out var slots)) return false;
            if (slotIndex < 0 || slotIndex >= slots.Length) return false;
            var slot = slots[slotIndex];
            if (!slot.IsCompleted) return false;

            outputItemId = slot.Recipe.outputItemId;
            quantity = slot.InputQuantity;
            BuildingEvents.RaiseProcessingCollected(processor, slotIndex, outputItemId);
            slot.State = ProcessingSlot.SlotState.Empty;
            slot.Recipe = null;
            slot.InputCropId = null;
            slot.InputQuantity = 0;
            return true;
        }

        public IReadOnlyList<ProcessingSlot> GetSlots(BuildingInstance processor)
        {
            return _processors.TryGetValue(processor, out var slots) ? slots : null;
        }
    }
}
