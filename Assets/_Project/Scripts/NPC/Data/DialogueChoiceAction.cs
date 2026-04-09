// S-04: 대화 선택지 액션 열거형
// -> see docs/systems/npc-shop-architecture.md 섹션 2.2
namespace SeedMind.NPC.Data
{
    public enum DialogueChoiceAction
    {
        Continue      = 0,  // 다음 노드로 진행 (jumpToNode 사용)
        OpenShop      = 1,  // 상점 UI 열기
        OpenUpgrade   = 2,  // 도구 업그레이드 UI 열기
        OpenBuild     = 3,  // 시설 건설 UI 열기
        CloseDialogue = 4   // 대화 종료
    }
}