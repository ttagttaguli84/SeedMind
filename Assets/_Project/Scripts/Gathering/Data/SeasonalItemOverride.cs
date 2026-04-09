// 계절별 아이템 풀 오버라이드
// -> see docs/systems/gathering-architecture.md 섹션 2.1
using SeedMind.Core;

namespace SeedMind.Gathering
{
    [System.Serializable]
    public class SeasonalItemOverride
    {
        public Season season;                        // 적용 계절
        public GatheringItemEntry[] overrideItems;   // 해당 계절 전용 아이템 풀
    }
}
