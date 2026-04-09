// 낚시 런타임 통계 — 세션 중 추적, 저장 시 FishingSaveData로 복사
// -> see docs/systems/fishing-architecture.md 섹션 1
using System.Collections.Generic;

namespace SeedMind.Fishing
{
    [System.Serializable]
    public class FishingStats
    {
        public int totalCasts;
        public int totalCaught;
        public int totalFailed;
        public Dictionary<string, int> caughtByFishId = new Dictionary<string, int>();
        public int rareFishCaught;
        public int maxStreak;
        public int currentStreak;

        public void RecordCast() => totalCasts++;

        public void RecordCaught(string fishId, bool isRareOrHigher)
        {
            totalCaught++;
            currentStreak++;
            if (currentStreak > maxStreak) maxStreak = currentStreak;
            if (!caughtByFishId.ContainsKey(fishId)) caughtByFishId[fishId] = 0;
            caughtByFishId[fishId]++;
            if (isRareOrHigher) rareFishCaught++;
        }

        public void RecordFailed()
        {
            totalFailed++;
            currentStreak = 0;
        }
    }
}
