namespace SeedMind.Building.Data
{
    /// <summary>
    /// 가공 유형.
    /// -> see docs/pipeline/data-pipeline.md 섹션 2.5 for canonical 정의
    /// </summary>
    public enum ProcessingType
    {
        Jam,            // 잼 (가공소)
        Juice,          // 주스 (가공소)
        Pickle,         // 절임 (가공소)
        Mill,           // 제분 (제분소)
        Fermentation,   // 발효 (발효실)
        Bake,           // 베이킹 (베이커리)
        Cheese          // 유제품 -- 치즈 공방 전용
    }
}
