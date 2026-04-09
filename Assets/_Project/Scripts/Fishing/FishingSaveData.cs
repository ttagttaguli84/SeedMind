// 낚시 세이브 데이터 — GameSaveData.fishing 필드에 직렬화
// -> see docs/systems/fishing-architecture.md 섹션 10
using System.Collections.Generic;

namespace SeedMind.Fishing
{
    [System.Serializable]
    public class FishingSaveData
    {
        // 통계
        public int totalCasts;
        public int totalCaught;
        public int totalFailed;
        public Dictionary<string, int> caughtByFishId = new Dictionary<string, int>();
        public int rareFishCaught;
        public int maxStreak;
        public int currentStreak;

        // 낚시 숙련도 (ARC-029)
        // -> see docs/systems/fishing-architecture.md 섹션 4A
        public int fishingProficiencyXP;
        public int fishingProficiencyLevel;
    }
}
