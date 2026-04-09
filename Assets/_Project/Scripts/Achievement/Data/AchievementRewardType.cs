// S-03: 업적 보상 타입 열거형
// -> see docs/systems/achievement-architecture.md 섹션 4.2
namespace SeedMind.Achievement
{
    public enum AchievementRewardType
    {
        None    = 0,   // 보상 없음 (달성 자체가 목적)
        Gold    = 1,   // 골드
        XP      = 2,   // 경험치
        Item    = 3,   // 아이템
        Title   = 4    // 칭호 (-> see docs/systems/achievement-system.md 섹션 4.2)
    }
}