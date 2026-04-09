// MilestoneData — 마일스톤 정의 데이터 클래스
// -> see docs/systems/progression-architecture.md 섹션 4.1
using UnityEngine;

namespace SeedMind.Level.Data
{
    [System.Serializable]
    public class MilestoneData
    {
        public string milestoneId;
        public string displayName;
        public string description;
        public MilestoneConditionType conditionType;
        public string conditionParam;
        public int conditionValue;
        public MilestoneReward reward;
        public Sprite icon;
        public bool isHidden;
    }

    [System.Serializable]
    public class MilestoneReward
    {
        public int goldReward;          // -> see docs/balance/progression-curve.md
        public int expReward;           // -> see docs/balance/progression-curve.md
        public UnlockItemEntry[] unlockRewards;
    }
}
