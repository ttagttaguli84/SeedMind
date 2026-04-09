// S-16: 대화창 UI 컨트롤러
// -> see docs/systems/npc-shop-architecture.md 섹션 3.4, 6.5
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.NPC;
using SeedMind.NPC.Data;

namespace SeedMind.UI
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject _dialoguePanel;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _speakerNameText;
        [SerializeField] private TextMeshProUGUI _dialogueText;
        [SerializeField] private Transform _choiceContainer;
        [SerializeField] private GameObject _choicePrefab;
        [SerializeField] private Button _advanceButton;

        [Header("시스템 참조")]
        [SerializeField] private DialogueSystem _dialogueSystem;

        private void OnEnable()
        {
            if (_dialogueSystem == null) return;
            _dialogueSystem.OnDialogueStarted += HandleDialogueStarted;
            _dialogueSystem.OnDialogueNodeChanged += HandleNodeChanged;
            _dialogueSystem.OnDialogueEnded += HandleDialogueEnded;
        }
        private void OnDisable()
        {
            if (_dialogueSystem == null) return;
            _dialogueSystem.OnDialogueStarted -= HandleDialogueStarted;
            _dialogueSystem.OnDialogueNodeChanged -= HandleNodeChanged;
            _dialogueSystem.OnDialogueEnded -= HandleDialogueEnded;
        }
        private void HandleDialogueStarted(DialogueData data) { _dialoguePanel.SetActive(true); }
        private void HandleNodeChanged(DialogueNode node) { /* 텍스트/선택지 갱신 */ }
        private void HandleDialogueEnded() { _dialoguePanel.SetActive(false); }
        // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.4
    }
}