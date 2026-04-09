// S-04: 업적 유형 열거형
// -> see docs/systems/achievement-architecture.md 섹션 2.1
namespace SeedMind.Achievement
{
    public enum AchievementType
    {
        Single  = 0,   // 단일 달성 (1회 조건 충족 시 영구 해금)
        Tiered  = 1    // 단계형 달성 (Bronze -> Silver -> Gold 순차 해금)
    }
}