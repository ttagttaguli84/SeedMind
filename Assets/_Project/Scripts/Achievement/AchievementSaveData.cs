// S-08: 업적 세이브 데이터 구조
// -> see docs/systems/achievement-architecture.md 섹션 7.2
using System.Collections.Generic;

namespace SeedMind.Achievement
{
    [System.Serializable]
    public class AchievementSaveData
    {
        public List<AchievementRecord> records;
        public int totalUnlocked;
    }
}