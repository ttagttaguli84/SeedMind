// S-03: 퀘스트 목표 타입 열거형
// -> see docs/systems/quest-architecture.md 섹션 2.3
namespace SeedMind.Quest
{
    public enum ObjectiveType
    {
        Harvest         = 0,   // 작물 수확
        Sell            = 1,   // 아이템 판매
        Deliver         = 2,   // NPC에게 납품
        Process         = 3,   // 가공품 제작
        Build           = 4,   // 시설 건설
        EarnGold        = 5,   // 골드 획득
        Till            = 6,   // 경작지 생성
        Water           = 7,   // 물주기
        QualityHarvest  = 8,   // 특정 품질 이상 수확
        UpgradeTool     = 9,   // 도구 업그레이드
        ReachLevel      = 10,  // 레벨 도달
        Composite       = 11,  // 복합 (AND/OR)
        Fish            = 12,  // 낚시 (어종 포획)
        Gather          = 13   // 채집 (채집물 수집)
    }
}