// S-01: 업적 카테고리 열거형
// -> see docs/systems/achievement-architecture.md 섹션 2.2
// -> see docs/systems/achievement-system.md 섹션 1 for 카테고리 정의
namespace SeedMind.Achievement.Data
{
    public enum AchievementCategory
    {
        Farming     = 0,   // 경작/수확 관련
        Economy     = 1,   // 골드/거래 관련
        Facility    = 2,   // 시설 건설/업그레이드 관련
        Tool        = 3,   // 도구 업그레이드 관련
        Explorer    = 4,   // 탐험/발견 관련
        Quest       = 5,   // 퀘스트 완료 관련
        Hidden      = 6,   // 숨겨진 업적 (달성 전 조건 비공개)
        Angler      = 7,   // 낚시 관련 (-> see docs/content/achievements.md 섹션 9)
        Gatherer    = 8    // 채집 관련 (-> see docs/content/achievements.md 섹션 9.5)
    }
}