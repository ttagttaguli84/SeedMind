// S-06: 대화 노드 데이터
// -> see docs/systems/npc-shop-architecture.md 섹션 2.2
using UnityEngine;

namespace SeedMind.NPC.Data
{
    [System.Serializable]
    public class DialogueNode
    {
        public string speakerName;               // 화자 이름
        [TextArea(2, 5)]
        public string text;                      // 대사 텍스트 (-> see docs/content/npcs.md)
        public DialogueChoice[] choices;         // 선택지 배열 (비어있으면 자동 다음 노드)
    }
}