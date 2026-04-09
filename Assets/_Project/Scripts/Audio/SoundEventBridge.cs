using System;
using UnityEngine;
using SeedMind.Farm;
using SeedMind.Farm.Data;
using SeedMind.Core;
using SeedMind.Level;
using SeedMind.Quest;
using SeedMind.Achievement;
using SeedMind.Achievement.Data;
using SeedMind.Building;
using SeedMind.NPC;
using SeedMind.Player;
using SeedMind.Player.Data;
using SeedMind.UI;

namespace SeedMind.Audio
{
    /// <summary>
    /// 게임 이벤트 → SoundManager SFX/BGM 요청 변환 브릿지.
    /// FishingEvents는 Phase F 구현 이후 추가 예정.
    /// -> see docs/systems/sound-architecture.md 섹션 8.1
    /// </summary>
    public class SoundEventBridge : MonoBehaviour
    {
        // 브릿지 내부 이벤트 (외부 구독용)
        public static Action<SoundEvent> OnSFXRequested;
        public static Action<BGMTrack, float> OnBGMRequested;

        private void OnEnable()
        {
            // 경작
            FarmEvents.OnTileStateChanged  += HandleTileStateChanged;
            FarmEvents.OnCropHarvested     += HandleCropHarvested;
            FarmEvents.OnCropWithered      += HandleCropWithered;

            // 시간
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.RegisterOnDayChanged(90, HandleDayChanged);
                TimeManager.Instance.OnDayPhaseChanged += HandleDayPhaseChanged;
            }

            // 진행
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnLevelUp   += HandleLevelUp;
                ProgressionManager.Instance.OnExpGained += HandleExpGained;
            }

            // 경제
            if (Economy.EconomyManager.Instance != null)
                Economy.EconomyManager.Instance.OnGoldChanged += HandleGoldChanged;

            // NPC
            NPCEvents.OnShopOpened      += HandleShopOpened;
            NPCEvents.OnShopClosed      += HandleShopClosed;
            NPCEvents.OnDialogueStarted += HandleDialogueStarted;

            // 퀘스트
            QuestEvents.OnQuestCompleted += HandleQuestCompleted;
            QuestEvents.OnQuestRewarded  += HandleQuestRewarded;

            // 업적
            AchievementEvents.OnAchievementUnlocked += HandleAchievementUnlocked;

            // 건물/가공
            BuildingEvents.OnBuildingPlaced     += HandleBuildingPlaced;
            BuildingEvents.OnBuildingCompleted  += HandleBuildingCompleted;
            BuildingEvents.OnProcessingStarted  += HandleProcessingStarted;
            BuildingEvents.OnProcessingComplete += HandleProcessingComplete;

            // 도구 업그레이드
            ToolUpgradeEvents.OnUpgradeStarted   += HandleUpgradeStarted;
            ToolUpgradeEvents.OnUpgradeCompleted += HandleUpgradeCompleted;

            // UI
            UIEvents.OnScreenOpened    += HandleScreenOpened;
            UIEvents.OnScreenClosed    += HandleScreenClosed;
            UIEvents.OnNotificationShown += HandleNotificationShown;
        }

        private void OnDisable()
        {
            FarmEvents.OnTileStateChanged  -= HandleTileStateChanged;
            FarmEvents.OnCropHarvested     -= HandleCropHarvested;
            FarmEvents.OnCropWithered      -= HandleCropWithered;

            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.UnregisterOnDayChanged(HandleDayChanged);
                TimeManager.Instance.OnDayPhaseChanged -= HandleDayPhaseChanged;
            }

            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnLevelUp   -= HandleLevelUp;
                ProgressionManager.Instance.OnExpGained -= HandleExpGained;
            }

            if (Economy.EconomyManager.Instance != null)
                Economy.EconomyManager.Instance.OnGoldChanged -= HandleGoldChanged;

            NPCEvents.OnShopOpened      -= HandleShopOpened;
            NPCEvents.OnShopClosed      -= HandleShopClosed;
            NPCEvents.OnDialogueStarted -= HandleDialogueStarted;

            QuestEvents.OnQuestCompleted -= HandleQuestCompleted;
            QuestEvents.OnQuestRewarded  -= HandleQuestRewarded;

            AchievementEvents.OnAchievementUnlocked -= HandleAchievementUnlocked;

            BuildingEvents.OnBuildingPlaced     -= HandleBuildingPlaced;
            BuildingEvents.OnBuildingCompleted  -= HandleBuildingCompleted;
            BuildingEvents.OnProcessingStarted  -= HandleProcessingStarted;
            BuildingEvents.OnProcessingComplete -= HandleProcessingComplete;

            ToolUpgradeEvents.OnUpgradeStarted   -= HandleUpgradeStarted;
            ToolUpgradeEvents.OnUpgradeCompleted -= HandleUpgradeCompleted;

            UIEvents.OnScreenOpened    -= HandleScreenOpened;
            UIEvents.OnScreenClosed    -= HandleScreenClosed;
            UIEvents.OnNotificationShown -= HandleNotificationShown;
        }

        // ── 핸들러 ───────────────────────────────────────────

        private void HandleTileStateChanged(FarmTile tile, TileState state)
        {
            switch (state)
            {
                case TileState.Tilled:  Play(SFXId.HoeTillBasic, tile.transform.position); break;
                case TileState.Planted: Play(SFXId.SeedPlant,    tile.transform.position); break;
                case TileState.Watered: Play(SFXId.WaterBasic,   tile.transform.position); break;
            }
        }

        private void HandleCropHarvested(FarmTile tile) =>
            Play(SFXId.Harvest, tile.transform.position);

        private void HandleCropWithered(FarmTile tile) =>
            Play(SFXId.CropWither, tile.transform.position);

        private void HandleDayChanged(int day) =>
            Play(SFXId.MorningChime);

        private void HandleDayPhaseChanged(DayPhase phase)
        {
            if (phase == DayPhase.Evening) Play(SFXId.EveningBell);
        }

        private void HandleLevelUp(LevelUpInfo info) =>
            Play(SFXId.LevelUp);

        private void HandleExpGained(ExpGainInfo info) =>
            Play(SFXId.XPGain);

        private void HandleGoldChanged(int oldGold, int newGold)
        {
            if (newGold > oldGold) Play(SFXId.GoldGain);
            else if (newGold < oldGold) Play(SFXId.GoldSpend);
        }

        private void HandleShopOpened(string npcId) => Play(SFXId.ShopOpen);
        private void HandleShopClosed(string npcId) => Play(SFXId.ShopClose);
        private void HandleDialogueStarted(string npcId, NPC.Data.DialogueData data) =>
            Play(SFXId.DialogueStart);

        private void HandleQuestCompleted(QuestInstance quest) => Play(SFXId.QuestComplete);
        private void HandleQuestRewarded(QuestInstance quest) => Play(SFXId.QuestReward);

        private void HandleAchievementUnlocked(AchievementData data) =>
            Play(SFXId.AchievementToast);

        private void HandleBuildingPlaced(BuildingInstance b) => Play(SFXId.ConstructStart);
        private void HandleBuildingCompleted(BuildingInstance b) => Play(SFXId.ConstructComplete);
        private void HandleProcessingStarted(BuildingInstance b, int slot) => Play(SFXId.ProcessStart);
        private void HandleProcessingComplete(BuildingInstance b, int slot) => Play(SFXId.ProcessComplete);

        private void HandleUpgradeStarted(ToolUpgradeInfo info) => Play(SFXId.ToolUpgradeStart);
        private void HandleUpgradeCompleted(ToolUpgradeInfo info) => Play(SFXId.ToolUpgradeComplete);

        private void HandleScreenOpened(ScreenType screen)
        {
            switch (screen)
            {
                case ScreenType.Inventory: Play(SFXId.InventoryOpen); break;
                case ScreenType.Shop:      Play(SFXId.ShopOpen);      break;
            }
        }

        private void HandleScreenClosed(ScreenType screen)
        {
            switch (screen)
            {
                case ScreenType.Inventory: Play(SFXId.InventoryClose); break;
                case ScreenType.Shop:      Play(SFXId.ShopClose);      break;
            }
        }

        private void HandleNotificationShown(NotificationData data) =>
            Play(SFXId.Notification);

        // ── 헬퍼 ─────────────────────────────────────────────

        private static void Play(SFXId id, Vector3? position = null)
        {
            SoundManager.Instance?.PlaySFX(id, position);
        }
    }
}
