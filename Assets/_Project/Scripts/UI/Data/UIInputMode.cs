// S-03: UI 입력 모드 열거형
// -> see docs/systems/ui-architecture.md 섹션 1.6
namespace SeedMind.UI
{
    public enum UIInputMode
    {
        Gameplay    = 0,   // 이동/도구 사용/상호작용 활성
        UIScreen    = 1,   // 이동 차단, UI 커서 활성, Esc로 닫기
        Dialogue    = 2,   // 모든 입력 차단, 대화 진행 키만 허용
        Popup       = 3    // UI Screen 위 팝업, Screen 조작 차단
    }
}
