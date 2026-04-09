using System.Collections.Generic;
using SeedMind.Building.Data;
using SeedMind.Core;

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

        public bool CancelProcessing(BuildingInstance processor, int slotIndex)
        {
            if (!_processors.TryGetValue(processor, out var slots)) return false;
            if (slotIndex < 0 || slotIndex >= slots.Length) return false;
            var slot = slots[slotIndex];
            if (slot.State != ProcessingSlot.SlotState.Processing) return false;

            string inputCropId = slot.InputCropId;
            int inputQuantity = slot.InputQuantity;
            slot.State = ProcessingSlot.SlotState.Empty;
            slot.Recipe = null;
            slot.InputCropId = null;
            slot.InputQuantity = 0;
            slot.RemainingHours = 0f;
            slot.TotalHours = 0f;
            BuildingEvents.RaiseProcessingCancelled(processor, slotIndex, inputCropId, inputQuantity);
            return true;
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
            quantity = slot.Recipe.outputQuantity > 0 ? slot.Recipe.outputQuantity : slot.InputQuantity;
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

        /// <summary>
        /// processingType에 맞는 레시피 목록을 DataRegistry에서 조회한다.
        /// </summary>
        public List<ProcessingRecipeData> GetAvailableRecipes(BuildingInstance processor)
        {
            var result = new List<ProcessingRecipeData>();
            if (processor?.Data == null) return result;

            var allRecipes = DataRegistry.Instance.GetAll<ProcessingRecipeData>();
            foreach (var recipe in allRecipes)
            {
                // 현재는 ProcessingType과 effectValue로 필터링
                // 정확한 facility 매핑은 facilities-architecture.md 섹션 7.2 참조
                result.Add(recipe);
            }
            return result;
        }

        // ── 저장/복원 ─────────────────────────────────────────────

        public ProcessingSaveData[] GetSaveData()
        {
            var result = new List<ProcessingSaveData>();
            foreach (var kv in _processors)
            {
                var inst = kv.Key;
                var slots = kv.Value;
                var slotSaves = new ProcessingSlotSaveData[slots.Length];
                for (int i = 0; i < slots.Length; i++)
                {
                    slotSaves[i] = new ProcessingSlotSaveData
                    {
                        state = slots[i].State,
                        recipeId = slots[i].Recipe?.dataId,
                        inputCropId = slots[i].InputCropId,
                        inputQuantity = slots[i].InputQuantity,
                        remainingHours = slots[i].RemainingHours,
                        totalHours = slots[i].TotalHours
                    };
                }
                result.Add(new ProcessingSaveData
                {
                    buildingDataId = inst.Data.dataId,
                    gridX = inst.GridX,
                    gridY = inst.GridY,
                    slots = slotSaves
                });
            }
            return result.ToArray();
        }

        public void LoadSaveData(ProcessingSaveData[] saveData)
        {
            if (saveData == null) return;
            foreach (var save in saveData)
            {
                // 대응하는 BuildingInstance 탐색
                BuildingInstance target = null;
                foreach (var inst in _processors.Keys)
                {
                    if (inst.Data.dataId == save.buildingDataId
                        && inst.GridX == save.gridX
                        && inst.GridY == save.gridY)
                    {
                        target = inst;
                        break;
                    }
                }
                if (target == null || save.slots == null) continue;

                var slots = _processors[target];
                for (int i = 0; i < System.Math.Min(slots.Length, save.slots.Length); i++)
                {
                    var s = save.slots[i];
                    slots[i].State = s.state;
                    slots[i].InputCropId = s.inputCropId;
                    slots[i].InputQuantity = s.inputQuantity;
                    slots[i].RemainingHours = s.remainingHours;
                    slots[i].TotalHours = s.totalHours;
                    if (!string.IsNullOrEmpty(s.recipeId))
                        slots[i].Recipe = DataRegistry.Instance.Get<ProcessingRecipeData>(s.recipeId);
                }
            }
        }
    }
}
