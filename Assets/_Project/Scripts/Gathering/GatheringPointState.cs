// 채집 포인트 런타임 상태 (Plain C# class)
// -> see docs/systems/gathering-architecture.md 섹션 1 (시스템 다이어그램)
using SeedMind.Core;

namespace SeedMind.Gathering
{
    [System.Serializable]
    public class GatheringPointState
    {
        public string pointId;
        public bool isActive = true;
        public int respawnDaysRemaining;
        public int lastGatheredDay;
        public Season lastGatheredSeason;
    }
}
