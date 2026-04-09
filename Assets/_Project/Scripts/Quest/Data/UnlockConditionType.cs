// S-05: 퀘스트 해금 조건 타입 열거형
// -> see docs/systems/quest-architecture.md 섹션 2.5
namespace SeedMind.Quest
{
    public enum UnlockConditionType
    {
        Level              = 0,
        Season             = 1,
        QuestComplete      = 2,
        FacilityBuilt      = 3,
        DayOfSeason        = 4,
        TutorialComplete   = 5
    }
}