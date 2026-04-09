// S-20: HUD 퀘스트 추적 위젯
// -> see docs/systems/quest-architecture.md 섹션 10
// -> see docs/systems/quest-system.md 섹션 8.3 for 추적 위젯 구조
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Quest;

namespace SeedMind.UI
{
    public class QuestTrackingUI : MonoBehaviour
    {
        [Header("위젯")]
        [SerializeField] private GameObject _trackingWidget;
        [SerializeField] private TextMeshProUGUI _questTitleText;
        [SerializeField] private TextMeshProUGUI _objectiveText;
        [SerializeField] private Slider _progressBar;

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

        private void OnQuestChanged(QuestInstance q) { Refresh(); }

        private void OnProgressChanged(QuestInstance q, int idx) { Refresh(); }

        private void Refresh()
        {
            // QuestManager.GetTrackedQuest() -> 표시 갱신
        }
    }
}
