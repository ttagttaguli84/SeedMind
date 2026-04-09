// S-13: 개별 NPC 행동 제어 (MonoBehaviour)
// -> see docs/systems/npc-shop-architecture.md 섹션 3.3
using UnityEngine;
using SeedMind.NPC.Data;

namespace SeedMind.NPC
{
    public class NPCController : MonoBehaviour
    {
        [SerializeField] private NPCData _npcData;

        private NPCActivityState _currentState;
        private bool _isInteracting;

        public NPCData Data => _npcData;
        public NPCActivityState CurrentState => _currentState;

        public void Interact(/* PlayerController player */) { /* 대화 트리거 */ }
        public void SetState(NPCActivityState state)
        {
            _currentState = state;
            NPCEvents.RaiseNPCStateChanged(_npcData.npcId, state);
        }
        public bool IsAvailable() => _currentState == NPCActivityState.Active;
        private void StartDialogue() { /* DialogueSystem 호출 */ }
        private void HandleDialogueChoice(DialogueChoiceAction action) { /* 서비스 위임 */ }
        private void OpenNPCService() { /* NPC 유형별 서비스 분기 */ }
        // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.3, 5.1~5.2
    }
}