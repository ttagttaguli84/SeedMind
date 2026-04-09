// 농장 구역의 현재 상태를 나타내는 enum
// -> see docs/systems/farm-expansion-architecture.md 섹션 2.1
namespace SeedMind.Farm
{
    public enum ZoneState
    {
        Locked,        // 해금 조건 미충족 (선행 구역 미해금)
        Unlockable,    // 해금 가능 (레벨/골드 조건 충족, 선행 구역 해금됨)
        Unlocked,      // 해금됨 (장애물 잔존)
        FullyCleared,  // 완전 개간 (모든 장애물 제거됨)
    }
}
