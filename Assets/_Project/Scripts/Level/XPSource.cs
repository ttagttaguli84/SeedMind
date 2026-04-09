// XP 획득 출처 열거형
// -> see docs/systems/progression-architecture.md 섹션 2.2
namespace SeedMind.Level
{
    public enum XPSource
    {
        CropHarvest,        // 작물 수확
        ToolUse,            // 도구 사용 (경작, 물주기 등)
        FacilityBuild,      // 시설 건설
        FacilityProcess,    // 가공 완료
        MilestoneReward,    // 마일스톤 달성 보너스
        QuestComplete,      // 퀘스트 완료 보상
        AchievementReward,  // 업적 달성 보상
        ToolUpgrade,        // 도구 업그레이드 완료
        AnimalCare,         // 동물 돌봄 (먹이, 쓰다듬기)
        AnimalHarvest,      // 동물 생산물 수집
        FishingCatch,       // 낚시 포획
        GatheringComplete,  // 채집 완료
        GatheringCatalog,   // 채집 도감 첫 발견 보상
    }
}
