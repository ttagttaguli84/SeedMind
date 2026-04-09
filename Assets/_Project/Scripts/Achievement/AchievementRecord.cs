// S-07: 개별 업적의 런타임 진행 상태
// -> see docs/systems/achievement-architecture.md 섹션 6.1
using System.Collections.Generic;
using UnityEngine;

namespace SeedMind.Achievement
{
    [System.Serializable]
    public class AchievementRecord
    {
        public string achievementId;
        public int currentProgress;
        public bool isUnlocked;
        public int unlockedDay;        // -1 = 미달성
        public int unlockedSeason;     // -1 = 미달성
        public int unlockedYear;       // -1 = 미달성
        public string currentTier;     // "None"/"Bronze"/"Silver"/"Gold" (Single이면 "")
        public List<TierUnlockRecord> tierHistory;

        public AchievementRecord(string id)
        {
            achievementId = id;
            currentProgress = 0;
            isUnlocked = false;
            unlockedDay = -1;
            unlockedSeason = -1;
            unlockedYear = -1;
            currentTier = "";
            tierHistory = new List<TierUnlockRecord>();
        }

        public float GetNormalizedProgress(int targetValue)
        {
            if (targetValue <= 0) return isUnlocked ? 1f : 0f;
            return Mathf.Clamp01((float)currentProgress / targetValue);
        }
    }

    [System.Serializable]
    public class TierUnlockRecord
    {
        public string tier;            // "Bronze" / "Silver" / "Gold"
        public int unlockedDay;
        public int unlockedSeason;
        public int unlockedYear;
    }
}