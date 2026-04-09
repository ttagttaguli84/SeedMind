// 마일스톤 조건 유형 열거형
// -> see docs/systems/progression-architecture.md 섹션 4.2
namespace SeedMind.Level
{
    public enum MilestoneConditionType
    {
        LevelReached,        // 특정 레벨 도달
        FirstHarvest,        // 특정 작물 첫 수확 (conditionParam = cropId)
        TotalHarvest,        // 총 수확 횟수 (conditionValue = 목표 횟수)
        CropHarvestCount,    // 특정 작물 수확 횟수 (conditionParam = cropId)
        GoldOwned,           // 골드 보유량 도달 (conditionValue = 목표 골드)
        GoldEarned,          // 누적 골드 수입 (conditionValue = 목표 골드)
        FacilityBuilt,       // 특정 시설 건설 (conditionParam = buildingId)
        TotalFacilities,     // 총 시설 수 (conditionValue = 목표 수)
        QualityHarvest,      // 특정 품질 이상 수확 (conditionParam = 품질, conditionValue = 횟수)
        DaysPlayed,          // 경과 일수 (conditionValue = 목표 일수)
    }
}
