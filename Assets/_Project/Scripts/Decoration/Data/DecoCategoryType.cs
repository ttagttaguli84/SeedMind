// 장식 카테고리 타입 enum
// -> see docs/systems/decoration-architecture.md 섹션 2.2
namespace SeedMind.Decoration.Data
{
    /// <summary>장식 카테고리 (Fence/Path/Light/Ornament/WaterDecor)</summary>
    public enum DecoCategoryType
    {
        Fence,        // 울타리 — edge 배치
        Path,         // 경로 — 타일 오버레이
        Light,        // 조명 — 1×1 타일 점유
        Ornament,     // 장식물 — 1×1 또는 2×2
        WaterDecor    // 수경 장식 — 2×2~3×3, Zone F 전용
    }
}
