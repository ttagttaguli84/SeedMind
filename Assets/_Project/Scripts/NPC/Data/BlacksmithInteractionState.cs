// S-01: 대장간 NPC 상호작용 상태 열거형
// -> see docs/systems/blacksmith-architecture.md 섹션 2.1
namespace SeedMind.NPC
{
    public enum BlacksmithInteractionState
    {
        Idle              = 0,  // 상호작용 없음
        Greeting          = 1,  // 인사 대화 재생 중
        ServiceMenu       = 2,  // 서비스 선택지 표시 중
        Chatting          = 3,  // 일상 대화 재생
        UpgradeSelect     = 4,  // ToolUpgradeUI에서 도구 선택 중
        UpgradeConfirm    = 5,  // 업그레이드 확인 팝업
        UpgradeResult     = 6,  // 업그레이드 시작/실패 결과 표시
        PickupResult      = 7,  // 완성 도구 수령 결과 표시
        MaterialShop      = 8,  // 재료 구매 상점 (ShopUI 위임)
    }
}
