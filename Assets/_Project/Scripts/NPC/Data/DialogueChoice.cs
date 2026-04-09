// S-05: 대화 선택지 데이터
// -> see docs/systems/npc-shop-architecture.md 섹션 2.2
namespace SeedMind.NPC.Data
{
    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;                // 선택지 UI 텍스트
        public DialogueChoiceAction action;      // 선택 시 동작
        public int jumpToNode;                   // 점프할 노드 인덱스 (-1 = 대화 종료)
    }
}