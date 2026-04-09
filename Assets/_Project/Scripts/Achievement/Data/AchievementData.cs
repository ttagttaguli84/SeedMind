// S-06: 업적 정적 정의 ScriptableObject
// -> see docs/systems/achievement-architecture.md 섹션 4.1
using UnityEngine;

namespace SeedMind.Achievement.Data
{
    [CreateAssetMenu(fileName = "NewAchievementData", menuName = "SeedMind/AchievementData")]
    public class AchievementData : ScriptableObject
    {
        [Header("기본 정보")]
        public string achievementId;                // 고유 식별자
        public string displayName;                  // -> see docs/systems/achievement-system.md 섹션 3
        [TextArea(2, 4)]
        public string description;                  // -> see docs/systems/achievement-system.md 섹션 3
        public AchievementCategory category;        // -> see S-01
        public AchievementType type;                // -> see S-04

        [Header("달성 조건 -- Single 전용")]
        public AchievementConditionType conditionType;  // -> see S-02
        public string targetId;                     // 대상 ID (""이면 any)
        public int targetValue;                     // -> see docs/systems/achievement-system.md

        [Header("단계형 조건 -- Tiered 전용")]
        public AchievementTierData[] tiers;         // Bronze[0], Silver[1], Gold[2]

        [Header("보상 -- Single 전용")]
        public AchievementRewardType rewardType;    // -> see S-03
        public int rewardAmount;                    // -> see docs/balance/progression-curve.md
        public string rewardItemId;                 // 아이템 ID ("" 이면 미사용)
        public string rewardTitleId;                // 칭호 ID ("" 이면 미사용)

        [Header("표시")]
        public bool isHidden;                       // true이면 달성 전까지 조건 비공개
        public Sprite icon;                         // 업적 아이콘 (null이면 카테고리 기본)
        public int sortOrder;                       // 카테고리 내 표시 순서
    }
}