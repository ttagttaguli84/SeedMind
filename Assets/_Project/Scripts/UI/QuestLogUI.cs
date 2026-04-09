// S-19: 퀘스트 로그 UI 컨트롤러
// -> see docs/systems/quest-architecture.md 섹션 10
// -> see docs/systems/quest-system.md 섹션 8.1 for UI 구조
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Quest;

namespace SeedMind.UI
{
    public class QuestLogUI : MonoBehaviour
    {
        [Header("패널")]
        [SerializeField] private GameObject _logPanel;

        [Header("탭 버튼")]
        [SerializeField] private Button _tabMainQuest;
        [SerializeField] private Button _tabNPCRequest;
        [SerializeField] private Button _tabDailyChallenge;
        [SerializeField] private Button _tabFarmChallenge;

        [Header("콘텐츠")]
        [SerializeField] private Transform _questListContainer;
        [SerializeField] private GameObject _questEntryPrefab;
        [SerializeField] private TextMeshProUGUI _questDetailTitle;
        [SerializeField] private TextMeshProUGUI _questDetailDesc;
        [SerializeField] private Transform _objectiveListContainer;

        private QuestCategory _currentTab = QuestCategory.MainQuest;

        public void Toggle()
        {
            _logPanel.SetActive(!_logPanel.activeSelf);
            if (_logPanel.activeSelf) RefreshList();
        }

        private void OnEnable()
        {
            QuestEvents.OnQuestActivated += OnQuestChanged;
            QuestEvents.OnQuestCompleted += OnQuestChanged;
            QuestEvents.OnObjectiveProgress += OnProgressChanged;
        }

        private void OnDisable()
        {
            QuestEvents.OnQuestActivated -= OnQuestChanged;
            QuestEvents.OnQuestCompleted -= OnQuestChanged;
            QuestEvents.OnObjectiveProgress -= OnProgressChanged;
        }

        private void OnQuestChanged(QuestInstance q) { RefreshList(); }

        private void OnProgressChanged(QuestInstance q, int idx) { RefreshList(); }

        private void RefreshList() { /* 현재 탭의 퀘스트 목록 갱신 */ }

        private void ShowQuestDetail(QuestInstance quest) { /* 상세 표시 */ }
    }
}
