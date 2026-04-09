// S-02: 팝업 우선순위 열거형
// -> see docs/systems/ui-architecture.md 섹션 1.5
namespace SeedMind.UI
{
    public enum PopupPriority
    {
        Low      = 0,   // 일반 안내 (예: 힌트)
        Normal   = 1,   // 일반 팝업 (예: 확인 대화상자)
        High     = 2,   // 중요 팝업 (예: 레벨업, 퀘스트 완료)
        Critical = 3    // 최우선 (예: 자동저장 실패, 튜토리얼 강제)
    }
}
