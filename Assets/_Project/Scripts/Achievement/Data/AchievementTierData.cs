// S-05: 단계형 업적의 각 단계 데이터 (Bronze/Silver/Gold)
// -> see docs/systems/achievement-architecture.md 섹션 4.1
namespace SeedMind.Achievement.Data
{
    [System.Serializable]
    public class AchievementTierData
    {
        public string tierName;                     // "Bronze" / "Silver" / "Gold"
        public AchievementConditionType conditionType;
        public string targetId;
        public int targetValue;                     // -> see docs/systems/achievement-system.md 섹션 3
        public AchievementRewardType rewardType;
        public int rewardAmount;                    // -> see docs/balance/progression-curve.md
        public string rewardItemId;
        public string rewardTitleId;
    }
}