// S-02: 업적 달성 조건 타입 열거형
// -> see docs/systems/achievement-architecture.md 섹션 2.3
namespace SeedMind.Achievement.Data
{
    public enum AchievementConditionType
    {
        HarvestCount            = 0,   // 총 수확 횟수 (작물 무관)
        GoldEarned              = 1,   // 누적 골드 획득량
        BuildingCount           = 2,   // 건설한 시설 총 수
        ToolUpgradeCount        = 3,   // 도구 업그레이드 총 횟수
        NPCMet                  = 4,   // 만난 NPC 수
        QuestCompleted          = 5,   // 완료한 퀘스트 총 수
        SpecificCropHarvested   = 6,   // 특정 작물 수확 횟수
        GoldSpent               = 7,   // 누적 골드 지출량
        DaysPlayed              = 8,   // 게임 내 경과 일수
        SeasonCompleted         = 9,   // 완료한 계절 수
        SpecificBuildingBuilt   = 10,  // 특정 시설 건설 여부
        TotalItemsSold          = 11,  // 판매한 아이템 총 수
        QualityHarvestCount     = 12,  // 특정 품질 이상 수확 횟수
        ProcessingCount         = 13,  // 가공 완료 총 횟수
        PurchaseCount           = 14,  // 상점 구매 횟수
        GatherCount             = 15,  // 채집 총 횟수 (아이템 무관)
        GatherSpeciesCollected  = 16,  // 채집으로 수집한 종류 수 (고유 itemId 수)
        GatherSickleUpgraded    = 17,  // 채집 낫 업그레이드 단계 달성
        Custom                  = 99   // 숨겨진 업적 전용 복합 조건
        // Custom 조건 적용 업적 -> see docs/systems/achievement-system.md 섹션 3.7
        // GatherCount, GatherSpeciesCollected, GatherSickleUpgraded -> see docs/systems/achievement-architecture.md 섹션 2.3
    }
}