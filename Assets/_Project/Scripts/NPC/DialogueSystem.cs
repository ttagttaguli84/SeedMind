// S-14: 대화 흐름 관리 (MonoBehaviour Singleton)
// -> see docs/systems/npc-shop-architecture.md 섹션 3.4
using System;
using UnityEngine;
using SeedMind.NPC.Data;

namespace SeedMind.NPC
{
    public class DialogueSystem : MonoBehaviour
    {
        private DialogueData _currentDialogue;
        private int _currentNodeIndex;
        private bool _isActive;
        private NPCController _currentNPC;

        public bool IsActive => _isActive;
        public DialogueNode CurrentNode =>
            (_currentDialogue != null && _currentNodeIndex < _currentDialogue.nodes.Length)
            ? _currentDialogue.nodes[_currentNodeIndex] : null;

        // 이벤트
        public event Action<DialogueData> OnDialogueStarted;
        public event Action<DialogueNode> OnDialogueNodeChanged;
        public event Action<DialogueChoiceAction> OnDialogueChoiceMade;
        public event Action OnDialogueEnded;

        public void StartDialogue(DialogueData data, NPCController npc)
        {
            _currentDialogue = data;
            _currentNPC = npc;
            _currentNodeIndex = 0;
            _isActive = true;
            OnDialogueStarted?.Invoke(data);
            NPCEvents.RaiseDialogueStarted(npc.Data.npcId, data);
            OnDialogueNodeChanged?.Invoke(CurrentNode);
        }
        public void AdvanceNode() { /* _currentNodeIndex++, 범위 체크 */ }
        public void SelectChoice(int choiceIndex) { /* 선택지 처리, 점프/액션 */ }
        public void EndDialogue()
        {
            _isActive = false;
            OnDialogueEnded?.Invoke();
            NPCEvents.RaiseDialogueEnded(_currentNPC.Data.npcId);
            _currentDialogue = null;
            _currentNPC = null;
        }
        private void ProcessChoiceAction(DialogueChoiceAction action)
        {
            OnDialogueChoiceMade?.Invoke(action);
        }
        // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.4
    }
}