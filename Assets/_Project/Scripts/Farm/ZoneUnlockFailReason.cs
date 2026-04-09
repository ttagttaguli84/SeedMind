// 구역 해금 실패 사유를 나타내는 enum
// -> see docs/systems/farm-expansion-architecture.md 섹션 4.3
namespace SeedMind.Farm
{
    public enum ZoneUnlockFailReason
    {
        LevelInsufficient,   // 레벨 요건 미충족
        InsufficientGold,    // 골드 부족
        AlreadyUnlocked,     // 이미 해금됨
        PrerequisiteZone,    // 선행 구역 미해금
    }
}
