namespace SeedMind.Building.Data
{
    /// <summary>
    /// 시설 배치 규칙.
    /// -> see docs/pipeline/data-pipeline.md 섹션 2.4 for canonical enum 정의
    /// </summary>
    public enum PlacementRule
    {
        FarmOnly,   // 농장 그리드 내부에만 배치 가능
        FarmEdge,   // 농장 그리드 가장자리에만 배치 가능
        Anywhere    // 농장 영역 어디든 배치 가능
    }
}
