namespace SeedMind.Building.Data
{
    /// <summary>
    /// 시설 효과 유형.
    /// -> see docs/pipeline/data-pipeline.md 섹션 2.4 for canonical enum 정의
    /// </summary>
    public enum BuildingEffectType
    {
        None,           // 효과 없음
        AutoWater,      // 인접 타일 자동 물주기 (물탱크)
        SeasonBypass,   // 계절 무관 재배 (온실)
        Storage,        // 작물 저장 (창고) -- effectValue = 최대 슬롯 수
        Processing      // 작물 가공 (가공소) -- effectValue = 초기 슬롯 수
    }
}
