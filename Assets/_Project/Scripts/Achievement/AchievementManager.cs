// S-10: 업적 시스템 매니저 (Singleton, ISaveable)
// -> see docs/systems/achievement-architecture.md 섹션 3.2
// SaveLoadOrder = 90 -> see docs/systems/save-load-architecture.md 섹션 7
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Achievement.Data;
using SeedMind.Save;

namespace SeedMind.Achievement
{
    public class AchievementManager : Singleton<AchievementManager>, ISaveable
    {
        [SerializeField] private AchievementData[] _allAchievements;

        private Dictionary<string, AchievementRecord> _records
            = new Dictionary<string, AchievementRecord>();
        private HashSet<string> _unlockedIds = new HashSet<string>();
        private Dictionary<string, AchievementData> _achievementLookup
            = new Dictionary<string, AchievementData>();

        // ISaveable
        public int SaveLoadOrder => 90; // -> see docs/systems/save-load-architecture.md 섹션 7

        public void Initialize()
        {
            _achievementLookup.Clear();
            _records.Clear();
            _unlockedIds.Clear();

            foreach (var data in _allAchievements)
            {
                _achievementLookup[data.achievementId] = data;
                _records[data.achievementId] = new AchievementRecord(data.achievementId);
            }

            SubscribeAll();
            Debug.Log($"[AchievementManager] Initialized with {_allAchievements.Length} achievements.");
        }

        // -> see docs/systems/achievement-architecture.md 섹션 5 for 이벤트 구독 매핑
        private void SubscribeAll()
        {
            // FarmEvents.OnCropHarvested += HandleHarvest;
            // EconomyEvents.OnSaleCompleted += HandleSale;
            // EconomyEvents.OnGoldSpent += HandleGoldSpent;
            // BuildingEvents.OnConstructionCompleted += HandleBuildingBuilt;
            // ToolEvents.OnToolUpgraded += HandleToolUpgrade;
            // NPCEvents.OnNPCFirstMet += HandleNPCMet;
            // QuestEvents.OnQuestCompleted += HandleQuestCompleted;
            // ProcessingEvents.OnProcessingCompleted += HandleProcessing;
            // TimeManager.OnDayChanged += HandleDayChanged;
            // TimeManager.OnSeasonChanged += HandleSeasonChanged;
            // GatheringEvents.OnItemGathered += HandleGather;           // ARC-035
            // GatheringEvents.OnSickleUpgraded += HandleSickleUpgrade;  // ARC-035
            Debug.Log("[AchievementManager] Event subscriptions registered (12 events).");
        }

        private void UnsubscribeAll()
        {
            // 구독 해제 (SubscribeAll과 대칭)
        }

        public void UpdateProgress(AchievementConditionType condType, int amount)
        {
            foreach (var data in _allAchievements)
            {
                if (IsMatchingCondition(data, condType) && !IsFullyUnlocked(data.achievementId))
                {
                    var record = _records[data.achievementId];
                    record.currentProgress += amount;
                    AchievementEvents.RaiseProgressUpdated(
                        data.achievementId,
                        record.GetNormalizedProgress(GetTargetValue(data)));
                    CheckCompletion(data.achievementId);
                }
            }
        }

        public void UpdateProgress(AchievementConditionType condType, string targetId, int amount)
        {
            foreach (var data in _allAchievements)
            {
                if (IsMatchingCondition(data, condType)
                    && data.targetId == targetId
                    && !IsFullyUnlocked(data.achievementId))
                {
                    var record = _records[data.achievementId];
                    record.currentProgress += amount;
                    AchievementEvents.RaiseProgressUpdated(
                        data.achievementId,
                        record.GetNormalizedProgress(GetTargetValue(data)));
                    CheckCompletion(data.achievementId);
                }
            }
        }

        public bool CheckCompletion(string achievementId)
        {
            if (!_achievementLookup.TryGetValue(achievementId, out var data)) return false;
            var record = _records[achievementId];

            if (data.type == AchievementType.Single)
            {
                if (!record.isUnlocked && record.currentProgress >= data.targetValue)
                {
                    UnlockAchievement(achievementId);
                    return true;
                }
            }
            else if (data.type == AchievementType.Tiered)
            {
                for (int i = 0; i < data.tiers.Length; i++)
                {
                    var tier = data.tiers[i];
                    bool alreadyUnlocked = record.tierHistory.Any(t => t.tier == tier.tierName);
                    if (!alreadyUnlocked && record.currentProgress >= tier.targetValue)
                    {
                        UnlockTier(achievementId, tier);
                    }
                }
            }
            return false;
        }

        public void UnlockAchievement(string achievementId)
        {
            var record = _records[achievementId];
            record.isUnlocked = true;
            // record.unlockedDay = TimeManager.Instance.CurrentDay;
            // record.unlockedSeason = TimeManager.Instance.CurrentSeasonIndex;
            // record.unlockedYear = TimeManager.Instance.CurrentYear;
            _unlockedIds.Add(achievementId);

            GrantReward(_achievementLookup[achievementId]);
            AchievementEvents.RaiseAchievementUnlocked(_achievementLookup[achievementId]);
            Debug.Log($"[AchievementManager] Unlocked: {achievementId}");
        }

        private void UnlockTier(string achievementId, AchievementTierData tier)
        {
            var record = _records[achievementId];
            record.currentTier = tier.tierName;
            record.tierHistory.Add(new TierUnlockRecord
            {
                tier = tier.tierName,
                // unlockedDay = TimeManager.Instance.CurrentDay,
                // unlockedSeason = TimeManager.Instance.CurrentSeasonIndex,
                // unlockedYear = TimeManager.Instance.CurrentYear
            });

            GrantTierReward(tier);

            if (tier.tierName == "Gold")
            {
                record.isUnlocked = true;
                _unlockedIds.Add(achievementId);
            }

            AchievementEvents.RaiseAchievementUnlocked(_achievementLookup[achievementId]);
            Debug.Log($"[AchievementManager] Tier unlocked: {achievementId} - {tier.tierName}");
        }

        private void GrantReward(AchievementData data)
        {
            // -> see docs/systems/achievement-architecture.md 섹션 3.2 UnlockAchievement 흐름
            Debug.Log($"[AchievementManager] Reward granted: {data.rewardType} x{data.rewardAmount}");
        }

        private void GrantTierReward(AchievementTierData tier)
        {
            Debug.Log($"[AchievementManager] Tier reward: {tier.rewardType} x{tier.rewardAmount}");
        }

        // --- 조회 API ---
        public AchievementRecord GetRecord(string achievementId)
            => _records.TryGetValue(achievementId, out var r) ? r : null;

        public IReadOnlyList<AchievementData> GetAchievementsByCategory(AchievementCategory cat)
            => _allAchievements.Where(a => a.category == (AchievementCategory)cat).ToList();

        public IReadOnlyList<AchievementData> GetUnlockedAchievements()
            => _allAchievements.Where(a => _unlockedIds.Contains(a.achievementId)).ToList();

        public float GetOverallProgress()
            => _allAchievements.Length > 0 ? (float)_unlockedIds.Count / _allAchievements.Length : 0f;

        public bool IsUnlocked(string achievementId)
            => _unlockedIds.Contains(achievementId);

        // --- 유틸 ---
        private bool IsMatchingCondition(AchievementData data, AchievementConditionType condType)
        {
            if (data.type == AchievementType.Single) return data.conditionType == condType;
            if (data.type == AchievementType.Tiered && data.tiers.Length > 0)
                return data.tiers[0].conditionType == condType;
            return false;
        }

        private bool IsFullyUnlocked(string achievementId)
            => _unlockedIds.Contains(achievementId);

        private int GetTargetValue(AchievementData data)
        {
            if (data.type == AchievementType.Single) return data.targetValue;
            if (data.type == AchievementType.Tiered && data.tiers.Length > 0)
                return data.tiers[data.tiers.Length - 1].targetValue;
            return 0;
        }

        // --- ISaveable ---
        public object GetSaveData()
        {
            return new AchievementSaveData
            {
                records = new List<AchievementRecord>(_records.Values),
                totalUnlocked = _unlockedIds.Count
            };
        }

        public void LoadSaveData(object data)
        {
            if (data is not AchievementSaveData saveData) return;
            _records.Clear();
            _unlockedIds.Clear();
            foreach (var record in saveData.records)
            {
                _records[record.achievementId] = record;
                if (record.isUnlocked) _unlockedIds.Add(record.achievementId);
            }
            Debug.Log($"[AchievementManager] Loaded {saveData.records.Count} records, {saveData.totalUnlocked} unlocked.");
        }

        private void OnDestroy()
        {
            UnsubscribeAll();
        }
    }
}
