// S-14: 이벤트 구독 기반 퀘스트 진행도 추적
// -> see docs/systems/quest-architecture.md 섹션 3.3, 6.3~6.4
using SeedMind.Quest.Data;

namespace SeedMind.Quest
{
    public class QuestTracker
    {
        private QuestManager _manager;
        private CumulativeStatsSaveData _cumulativeStats;

        public QuestTracker(QuestManager manager)
        {
            _manager = manager;
            _cumulativeStats = new CumulativeStatsSaveData();
        }

        public void SubscribeAll()
        {
            // 이벤트 구독 매핑:
            // -> see docs/systems/quest-architecture.md 섹션 6.3
            // FarmEvents.OnCropHarvested += OnCropHarvested;
            // FarmEvents.OnTileTilled += OnTileTilled;
            // FarmEvents.OnCropWatered += OnCropWatered;
            // EconomyEvents.OnItemSold += OnItemSold;
            // BuildingEvents.OnConstructionCompleted += OnBuildingCompleted;
            // ProgressionEvents.OnLevelUp += OnLevelReached;
            // ToolUpgradeEvents.OnUpgradeCompleted += OnToolUpgraded;
            // ProcessingEvents.OnProcessingCompleted += OnItemProcessed;
            // NPCEvents.OnItemDelivered += OnItemDelivered;
            // FishingEvents.OnFishCaught += OnFishCaught;           // ARC-034
            // GatheringEvents.OnItemGathered += OnItemGathered;     // ARC-034
        }

        public void UnsubscribeAll() { /* 전체 구독 해제 */ }

        public void UpdateObjective(ObjectiveType type,
            string targetId, int delta, int quality = 0)
        {
            // -> see docs/systems/quest-architecture.md 섹션 6.4
            // 활성 퀘스트 순회 -> 목표 매칭 -> 진행도 갱신 -> 완료 판정
        }

        public CumulativeStatsSaveData GetCumulativeStats()
            => _cumulativeStats;
        public void LoadCumulativeStats(CumulativeStatsSaveData data)
        {
            if (data != null) _cumulativeStats = data;
        }
    }
}