// S-07: 대화 흐름 ScriptableObject
// -> see docs/systems/npc-shop-architecture.md 섹션 2.2
using UnityEngine;

namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewDialogueData", menuName = "SeedMind/DialogueData")]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;                // "greeting_merchant_spring"
        public DialogueNode[] nodes;             // 대화 노드 배열 (순서대로 진행)
    }
}