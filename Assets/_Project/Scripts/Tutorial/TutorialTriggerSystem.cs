// S-13: 튜토리얼 트리거 시스템 — 이벤트 버스 구독으로 단계 완료 판정
// -> see docs/systems/tutorial-architecture.md 섹션 5.1
using UnityEngine;
using SeedMind.Farm;
using SeedMind.Farm.Data;
using SeedMind.Building;
using SeedMind.NPC;
using SeedMind.NPC.Data;
using SeedMind.Player;
using SeedMind.Player.Data;
using SeedMind.Tutorial.Data;

namespace SeedMind.Tutorial
{
    public class TutorialTriggerSystem : MonoBehaviour
    {
        [SerializeField] private TutorialManager _manager;

        private void OnEnable()
        {
            FarmEvents.OnTileStateChanged += HandleTileStateChanged;
            FarmEvents.OnCropHarvested += HandleCropHarvested;
            FarmEvents.OnTileWatered += HandleTileWatered;

            BuildingEvents.OnBuildingPlaced += HandleBuildingPlaced;
            BuildingEvents.OnBuildingCompleted += HandleBuildingCompleted;
            BuildingEvents.OnProcessingComplete += HandleProcessingComplete;

            NPCEvents.OnShopOpened += HandleShopOpened;
            NPCEvents.OnDialogueStarted += HandleDialogueStarted;

            ToolUpgradeEvents.OnUpgradeStarted += HandleUpgradeStarted;
            ToolUpgradeEvents.OnUpgradeCompleted += HandleUpgradeCompleted;
        }

        private void OnDisable()
        {
            FarmEvents.OnTileStateChanged -= HandleTileStateChanged;
            FarmEvents.OnCropHarvested -= HandleCropHarvested;
            FarmEvents.OnTileWatered -= HandleTileWatered;

            BuildingEvents.OnBuildingPlaced -= HandleBuildingPlaced;
            BuildingEvents.OnBuildingCompleted -= HandleBuildingCompleted;
            BuildingEvents.OnProcessingComplete -= HandleProcessingComplete;

            NPCEvents.OnShopOpened -= HandleShopOpened;
            NPCEvents.OnDialogueStarted -= HandleDialogueStarted;

            ToolUpgradeEvents.OnUpgradeStarted -= HandleUpgradeStarted;
            ToolUpgradeEvents.OnUpgradeCompleted -= HandleUpgradeCompleted;
        }

        // --- 이벤트 핸들러 ---

        private void HandleTileStateChanged(FarmTile tile, TileState newState)
        {
            if (newState == TileState.Tilled)
            {
                TryCompleteActiveStep("FarmEvents.OnTileTilled");
                TryTriggerSequence(TutorialTriggerType.EventFired, "FarmEvents.OnTileTilled");
            }
            else if (newState == TileState.Planted)
            {
                TryCompleteActiveStep("FarmEvents.OnCropPlanted");
            }
            else if (newState == TileState.Watered || newState == TileState.Dry)
            {
                TryCompleteActiveStep("FarmEvents.OnTileWatered");
            }
        }

        private void HandleTileWatered(FarmTile tile)
        {
            TryCompleteActiveStep("FarmEvents.OnTileWatered");
        }

        private void HandleCropHarvested(FarmTile tile)
        {
            TryCompleteActiveStep("FarmEvents.OnCropHarvested");
        }

        private void HandleBuildingPlaced(BuildingInstance inst)
        {
            TryCompleteActiveStep("BuildingEvents.OnBuildingPlaced");
            TryTriggerSequence(TutorialTriggerType.EventFired, "BuildingEvents.OnBuildingPlaced");
        }

        private void HandleBuildingCompleted(BuildingInstance inst)
        {
            TryCompleteActiveStep("BuildingEvents.OnBuildingCompleted");
        }

        private void HandleProcessingComplete(BuildingInstance inst, int slotIndex)
        {
            TryCompleteActiveStep("BuildingEvents.OnProcessingComplete");
        }

        private void HandleShopOpened(string npcId)
        {
            TryCompleteActiveStep("NPCEvents.OnShopOpened");
        }

        private void HandleDialogueStarted(string npcId, DialogueData data)
        {
            TryCompleteActiveStep("NPCEvents.OnDialogueStarted");
        }

        private void HandleUpgradeStarted(ToolUpgradeInfo info)
        {
            TryCompleteActiveStep("ToolUpgradeEvents.OnUpgradeStarted");
            TryTriggerSequence(TutorialTriggerType.EventFired, "ToolUpgradeEvents.OnUpgradeStarted");
        }

        private void HandleUpgradeCompleted(ToolUpgradeInfo info)
        {
            TryCompleteActiveStep("ToolUpgradeEvents.OnUpgradeCompleted");
        }

        // --- 판정 로직 ---

        private void TryCompleteActiveStep(string eventType)
        {
            if (_manager == null) return;
            var activeStep = _manager.GetActiveStep();
            if (activeStep == null) return;
            if (activeStep.completionType != StepCompletionType.EventBased) return;
            if (activeStep.completionEventType != eventType) return;

            _manager.OnStepCompleted();
        }

        private void TryTriggerSequence(TutorialTriggerType triggerType, string param)
        {
            if (_manager == null) return;
            _manager.TryStartSequenceByTrigger(triggerType, param);
        }
    }
}
