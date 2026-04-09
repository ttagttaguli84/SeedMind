// ProgressionManager — XP/레벨/해금/마일스톤 통합 관리자
// -> see docs/systems/progression-architecture.md
using System;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Farm;
using SeedMind.Level.Data;
using SeedMind.Livestock;

namespace SeedMind.Level
{
    public class ProgressionManager : Singleton<ProgressionManager>
    {
        // --- 직렬화 필드 ---
        [SerializeField] private ProgressionData _progressionData;

        // --- 상태 ---
        private int _currentLevel = 1;
        private int _currentExp = 0;
        private int _totalExpEarned = 0;

        // --- 내부 시스템 ---
        private UnlockRegistry _unlockRegistry;
        private MilestoneTracker _milestoneTracker;

        // --- 읽기 전용 프로퍼티 ---
        public int CurrentLevel => _currentLevel;
        public int CurrentExp => _currentExp;
        public int TotalExpEarned => _totalExpEarned;
        public bool IsMaxLevel => _progressionData != null && _currentLevel >= _progressionData.maxLevel;

        public int ExpToNextLevel
        {
            get
            {
                if (_progressionData == null || IsMaxLevel) return 0;
                int idx = _currentLevel - 1;
                if (idx < 0 || idx >= _progressionData.expPerLevel.Length) return 0;
                return _progressionData.expPerLevel[idx] - _currentExp;
            }
        }

        public float ExpProgress
        {
            get
            {
                if (_progressionData == null || IsMaxLevel) return 1f;
                int idx = _currentLevel - 1;
                if (idx < 0 || idx >= _progressionData.expPerLevel.Length) return 0f;
                int needed = _progressionData.expPerLevel[idx];
                return needed > 0 ? Mathf.Clamp01((float)_currentExp / needed) : 0f;
            }
        }

        // --- 이벤트 ---
        public event Action<ExpGainInfo> OnExpGained;
        public event Action<LevelUpInfo> OnLevelUp;
        public event Action<UnlockInfo> OnUnlockAcquired;
        public event Action<MilestoneData> OnMilestoneComplete;

        // ============================================================
        // Unity 생명주기
        // ============================================================

        protected override void Awake()
        {
            base.Awake();
            _unlockRegistry = new UnlockRegistry();
            _milestoneTracker = new MilestoneTracker();
        }

        private void Start()
        {
            if (_progressionData != null)
                Initialize(_progressionData);
        }

        private void OnEnable()
        {
            FarmEvents.OnCropHarvested += HandleCropHarvested;

            // TimeManager 구독 — 일일 마일스톤 체크 (우선순위 60)
            if (TimeManager.Instance != null)
                TimeManager.Instance.RegisterOnDayChanged(60, OnDayChanged);

            // TODO: 이하 이벤트는 해당 시스템 구현 후 활성화
            // BuildingManager.OnBuildingConstructed      += HandleBuilding;
            // BlacksmithEvents.OnUpgradeCompleted        += HandleToolUpgrade;
            // QuestEvents.OnQuestRewarded                += HandleQuestXP;
            // AchievementEvents.OnAchievementUnlocked    += HandleAchievementXP;
            SeedMind.Fishing.FishingEvents.OnFishCaught += HandleFishCaught;
            // GatheringEvents.OnGatheringCompleted       += HandleGatheringXP;
            LivestockEvents.OnAnimalFed                += HandleAnimalCareXP;
            LivestockEvents.OnAnimalPetted             += HandleAnimalCareXP;
            LivestockEvents.OnProductCollected         += HandleAnimalHarvestXP;
        }

        private void OnDisable()
        {
            FarmEvents.OnCropHarvested -= HandleCropHarvested;
            SeedMind.Fishing.FishingEvents.OnFishCaught -= HandleFishCaught;
            LivestockEvents.OnAnimalFed      -= HandleAnimalCareXP;
            LivestockEvents.OnAnimalPetted   -= HandleAnimalCareXP;
            LivestockEvents.OnProductCollected -= HandleAnimalHarvestXP;

            if (TimeManager.Instance != null)
                TimeManager.Instance.UnregisterOnDayChanged(OnDayChanged);
        }

        // ============================================================
        // 공개 API
        // ============================================================

        public void Initialize(ProgressionData data)
        {
            _progressionData = data;
            _unlockRegistry.Initialize(data.unlockTable, _currentLevel);
            _milestoneTracker.Initialize(data.milestones);
        }

        public void AddExp(int amount, XPSource source)
        {
            if (amount <= 0 || IsMaxLevel) return;

            _currentExp += amount;
            _totalExpEarned += amount;

            OnExpGained?.Invoke(new ExpGainInfo
            {
                amount = amount,
                source = source,
                totalExp = _totalExpEarned,
                currentLevel = _currentLevel
            });

            RunLevelUp();
            RunMilestoneCheck();
        }

        public int GetExpForSource(XPSource source, object context = null)
        {
            if (_progressionData == null) return 0;

            switch (source)
            {
                case XPSource.ToolUse:         return _progressionData.toolUseExp;
                case XPSource.FacilityBuild:   return _progressionData.buildingConstructExp;
                case XPSource.FacilityProcess: return _progressionData.facilityProcessExp;
                case XPSource.ToolUpgrade:     return _progressionData.toolUpgradeExp;
                case XPSource.AnimalCare:      return _progressionData.animalCareExp;
                case XPSource.AnimalHarvest:   return _progressionData.animalHarvestBaseExp;

                case XPSource.MilestoneReward:
                    return context is MilestoneData ms ? ms.reward.expReward : 0;

                case XPSource.QuestComplete:
                case XPSource.AchievementReward:
                    return context is int xp ? xp : 0;

                case XPSource.FishingCatch:
                    // FishData.expReward 값을 context로 전달
                    // -> see docs/systems/fishing-architecture.md 섹션 6.2
                    return context is int fishXp ? fishXp : 0;

                default: return 0;
            }
        }

        public bool IsUnlocked(UnlockType type, string itemId)
            => _unlockRegistry?.IsUnlocked(type, itemId) ?? false;

        public string[] GetUnlockedItems(UnlockType type)
            => _unlockRegistry?.GetUnlockedItems(type) ?? Array.Empty<string>();

        // ============================================================
        // 이벤트 핸들러
        // ============================================================

        private void HandleCropHarvested(Farm.FarmTile tile)
        {
            if (tile == null || tile.cropInstance == null || tile.cropInstance.cropData == null)
                return;

            var crop = tile.cropInstance.cropData;
            int exp = CalcHarvestExp(crop, 0); // Normal 품질 (0)
            AddExp(exp, XPSource.CropHarvest);

            _milestoneTracker.UpdateProgress(MilestoneConditionType.TotalHarvest, null, 1);
            _milestoneTracker.UpdateProgress(MilestoneConditionType.CropHarvestCount, crop.cropId, 1);
            _milestoneTracker.UpdateProgress(MilestoneConditionType.FirstHarvest, crop.cropId, 1);
        }

        private void HandleFishCaught(SeedMind.Fishing.Data.FishData fish, SeedMind.Economy.CropQuality quality)
        {
            if (fish == null) return;
            // -> see docs/systems/fishing-architecture.md 섹션 6.3
            int xp = GetExpForSource(XPSource.FishingCatch, fish.expReward);
            AddExp(xp, XPSource.FishingCatch);
        }

        private void HandleAnimalCareXP(AnimalInstance animal)
        {
            if (animal == null) return;
            AddExp(GetExpForSource(XPSource.AnimalCare), XPSource.AnimalCare);
        }

        private void HandleAnimalHarvestXP(AnimalInstance animal, AnimalProductInfo info)
        {
            if (animal == null) return;
            AddExp(GetExpForSource(XPSource.AnimalHarvest), XPSource.AnimalHarvest);
        }

        private void OnDayChanged(int newDay)
        {
            if (TimeManager.Instance != null)
                _milestoneTracker.SetProgress(
                    MilestoneConditionType.DaysPlayed, null,
                    TimeManager.Instance.TotalElapsedDays);
            RunMilestoneCheck();
        }

        // ============================================================
        // 내부 로직 — 이름이 겹치지 않도록 Run* 접두사 사용
        // ============================================================

        private void RunLevelUp()
        {
            if (_progressionData == null) return;

            while (!IsMaxLevel)
            {
                int idx = _currentLevel - 1;
                if (idx < 0 || idx >= _progressionData.expPerLevel.Length) break;

                int needed = _progressionData.expPerLevel[idx];
                if (_currentExp < needed) break;

                _currentExp -= needed;
                int prevLevel = _currentLevel;
                _currentLevel++;

                var newUnlocks = _unlockRegistry.ApplyLevelUnlocks(
                    _progressionData.unlockTable, _currentLevel);

                foreach (var unlock in newUnlocks)
                    OnUnlockAcquired?.Invoke(unlock);

                OnLevelUp?.Invoke(new LevelUpInfo
                {
                    previousLevel = prevLevel,
                    newLevel = _currentLevel,
                    newUnlocks = newUnlocks
                });

                _milestoneTracker.SetProgress(
                    MilestoneConditionType.LevelReached, null, _currentLevel);
            }
        }

        private void RunMilestoneCheck()
        {
            var completed = _milestoneTracker.CheckCompletions();
            foreach (var ms in completed)
            {
                OnMilestoneComplete?.Invoke(ms);
                if (ms.reward != null && ms.reward.expReward > 0)
                    AddExp(ms.reward.expReward, XPSource.MilestoneReward);
                // TODO: EconomyManager.Instance?.AddGold(ms.reward.goldReward);
            }
        }

        private int CalcHarvestExp(Farm.Data.CropData crop, int qualityIndex)
        {
            if (_progressionData == null || crop == null) return 0;

            int baseExp = _progressionData.harvestExpBase
                        + Mathf.FloorToInt(crop.growthDays * _progressionData.harvestExpPerGrowthDay);

            float qualityMult = 1f;
            if (_progressionData.qualityExpBonus != null
                && qualityIndex >= 0
                && qualityIndex < _progressionData.qualityExpBonus.Length)
                qualityMult = _progressionData.qualityExpBonus[qualityIndex];

            return Mathf.RoundToInt(baseExp * qualityMult);
        }
    }

    // --- 이벤트 데이터 구조체 ---

    public struct ExpGainInfo
    {
        public int amount;
        public XPSource source;
        public int totalExp;
        public int currentLevel;
    }

    public struct LevelUpInfo
    {
        public int previousLevel;
        public int newLevel;
        public UnlockInfo[] newUnlocks;
    }
}
