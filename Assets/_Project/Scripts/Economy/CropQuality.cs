namespace SeedMind.Economy
{
    /// <summary>
    /// 작물/아이템 품질 등급.
    /// 판매 시 가격 배수에 영향을 준다.
    /// -> see docs/systems/economy-architecture.md 섹션 4.4 for canonical 정의
    /// -> see docs/systems/crop-growth.md for 품질 결정 로직
    /// </summary>
    public enum CropQuality
    {
        Normal  = 0,   // 일반  — 기본 판매가 x1.0
        Silver  = 1,   // 은별  — x1.25
        Gold    = 2,   // 금별  — x1.5
        Iridium = 3    // 이리듐별 — x2.0
    }
}
