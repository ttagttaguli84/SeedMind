namespace SeedMind.Farm.Data
{
    /// <summary>
    /// 작물 재배 가능 계절 비트마스크.
    /// SeedMind.Core.Season(순차 enum)과 별개 — 다중 계절 조합에 사용.
    /// -> see docs/systems/farming-architecture.md 섹션 4.1
    /// </summary>
    [System.Flags]
    public enum SeasonFlag
    {
        None   = 0,
        Spring = 1 << 0,  // 1
        Summer = 1 << 1,  // 2
        Autumn = 1 << 2,  // 4
        Winter = 1 << 3   // 8 — 온실 전용
    }
}
