// S-05: 시퀀스 시작 트리거 유형
// -> see docs/systems/tutorial-architecture.md 섹션 4.1
namespace SeedMind.Tutorial.Data
{
    public enum TutorialTriggerType
    {
        NewGame        = 0,
        UnlockAchieved = 1,
        FirstVisit     = 2,
        EventFired     = 3,
        LevelReached   = 4
    }
}
