// S-06: 대장간 NPC 상호작용 진입점
// -> see docs/systems/blacksmith-architecture.md 섹션 1, 2
using UnityEngine;
using SeedMind.NPC.Data;
using SeedMind.Player;

namespace SeedMind.NPC
{
    /// <summary>
    /// 대장간 NPC(철수) 상호작용 진입점.
    /// 기존 NPCController에 부착하여 대장간 고유의 FSM 기반 대화/서비스 흐름을 관리한다.
    /// </summary>
    public class BlacksmithNPC : MonoBehaviour
    {
        [Header("설정 참조")]
        [SerializeField] private NPCData _npcData;
            // -> see npc-shop-architecture.md 섹션 2.1
        [SerializeField] private BlacksmithNPCData _blacksmithData;

        // --- 외부 참조 (Awake에서 캐싱) ---
        private DialogueSystem _dialogueSystem;
        private ToolUpgradeSystem _upgradeSystem;
        private NPCAffinityTracker _affinityTracker;
        private NPCController _npcController;

        // --- 상태 ---
        private BlacksmithInteractionState _interactionState
            = BlacksmithInteractionState.Idle;
        private bool _isInteracting;

        private void Awake()
        {
            _dialogueSystem   = FindObjectOfType<DialogueSystem>();
            _upgradeSystem    = FindObjectOfType<ToolUpgradeSystem>();
            _affinityTracker  = FindObjectOfType<NPCAffinityTracker>();
            _npcController    = GetComponent<NPCController>();
        }

        private void OnEnable()
        {
            ToolUpgradeEvents.OnUpgradeCompleted += OnUpgradeCompleted;
        }

        private void OnDisable()
        {
            ToolUpgradeEvents.OnUpgradeCompleted -= OnUpgradeCompleted;
        }

        // --- 공개 메서드 ---

        /// <summary>플레이어가 NPC와 상호작용 시작</summary>
        public void Interact()
        {
            if (_isInteracting) return;
            _isInteracting = true;

            var greeting = SelectGreetingDialogue();
            if (greeting != null && _dialogueSystem != null)
            {
                _interactionState = BlacksmithInteractionState.Greeting;
                _dialogueSystem.StartDialogue(greeting, _npcController);

                // 일일 친밀도 부여
                if (_affinityTracker != null && _blacksmithData != null)
                {
                    if (_affinityTracker.CanGiveDailyAffinity(_blacksmithData.npcId))
                    {
                        _affinityTracker.MarkDailyVisit(_blacksmithData.npcId);
                    }
                }
            }
        }

        /// <summary>대화 선택지 액션 처리</summary>
        public void HandleDialogueChoice(DialogueChoiceAction action)
        {
            switch (action)
            {
                case DialogueChoiceAction.OpenUpgrade:
                    _interactionState = BlacksmithInteractionState.UpgradeSelect;
                    break;
                case DialogueChoiceAction.Continue:
                    _interactionState = BlacksmithInteractionState.ServiceMenu;
                    break;
                case DialogueChoiceAction.CloseDialogue:
                    EndInteraction();
                    break;
                default:
                    break;
            }
        }

        /// <summary>ToolUpgradeUI 결과 수신</summary>
        public void OnUpgradeUIResult(bool confirmed)
        {
            _interactionState = confirmed
                ? BlacksmithInteractionState.UpgradeResult
                : BlacksmithInteractionState.ServiceMenu;
        }

        // --- 내부 메서드 ---

        private DialogueData SelectGreetingDialogue()
        {
            if (_blacksmithData == null) return null;

            // 도구 수령 대기 중인지 확인
            if (CheckPendingPickup() && _blacksmithData.pendingPickupDialogue != null)
                return _blacksmithData.pendingPickupDialogue;

            // 최초 만남 판별 (친밀도 0이고 방문 이력 없음)
            if (_affinityTracker != null)
            {
                int level = _affinityTracker.GetAffinityLevel(
                    _blacksmithData.npcId, _blacksmithData.affinityThresholds);

                if (_blacksmithData.greetingDialogues != null
                    && level < _blacksmithData.greetingDialogues.Length)
                    return _blacksmithData.greetingDialogues[level];
            }

            return _blacksmithData.greetingDialogues != null
                && _blacksmithData.greetingDialogues.Length > 0
                    ? _blacksmithData.greetingDialogues[0]
                    : null;
        }

        private bool CheckPendingPickup()
        {
            if (_upgradeSystem == null) return false;
            // PendingUpgrade 완료 여부 확인
            // (ToolUpgradeSystem 내부 구현에서 완료된 항목 존재 시 true)
            return false; // TODO: _upgradeSystem.HasCompletedUpgrade() 구현 후 연결
        }

        private void OnUpgradeCompleted(Player.Data.ToolUpgradeInfo info)
        {
            // 업그레이드 완료 시 친밀도 부여
            if (_affinityTracker != null && _blacksmithData != null)
            {
                _affinityTracker.AddAffinity(
                    _blacksmithData.npcId,
                    _blacksmithData.upgradeCompleteAffinity,
                    _blacksmithData.affinityThresholds);
            }
        }

        private void EndInteraction()
        {
            _isInteracting = false;
            _interactionState = BlacksmithInteractionState.Idle;
        }
    }
}
