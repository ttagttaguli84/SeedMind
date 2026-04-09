// 울타리 배치용 방향 enum
// -> see docs/systems/decoration-architecture.md 섹션 2.2
namespace SeedMind.Decoration.Data
{
    /// <summary>울타리 edge 배치 방향</summary>
    public enum EdgeDirection
    {
        None,   // 방향 없음 (비-edge 배치 아이템)
        North,  // 타일 북쪽 경계
        South,  // 타일 남쪽 경계
        East,   // 타일 동쪽 경계
        West    // 타일 서쪽 경계
    }
}
