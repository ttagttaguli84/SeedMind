// S-04: 단계 완료 판정 유형
// -> see docs/systems/tutorial-architecture.md 섹션 4.2
namespace SeedMind.Tutorial.Data
{
    public enum StepCompletionType
    {
        EventBased      = 0,
        TimeBased       = 1,
        ClickToContinue = 2,
        Composite       = 3
    }
}
