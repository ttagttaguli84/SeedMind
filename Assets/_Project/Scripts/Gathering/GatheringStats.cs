// 채집 런타임 통계 (Plain C# class)
// -> see docs/systems/gathering-architecture.md 섹션 1 (시스템 다이어그램)
using System.Collections.Generic;

namespace SeedMind.Gathering
{
    public class GatheringStats
    {
        public int totalGathered;                                   // 총 채집 횟수
        public Dictionary<string, int> gatheredByItemId = new Dictionary<string, int>(); // 아이템별
        public Dictionary<string, int> gatheredByZone   = new Dictionary<string, int>(); // Zone별
        public int rareItemsFound;                                  // 희귀 이상 아이템 발견 횟수
    }
}
