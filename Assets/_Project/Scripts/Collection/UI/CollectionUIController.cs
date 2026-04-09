using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.UI;

namespace SeedMind.Collection.UI
{
    /// <summary>
    /// 수집 도감 통합 패널 컨트롤러. 어종/채집물 탭 전환 및 전체 완성률 표시.
    /// -> see docs/systems/collection-architecture.md 섹션 6.1, 6.2
    /// </summary>
    public class CollectionUIController : ScreenBase
    {
        [Header("매니저 참조")]
        [SerializeField] private GatheringCatalogManager _gatheringCatalogManager;

        [Header("UI 참조")]
        [SerializeField] private Button[] _tabButtons;          // [0]=Fish, [1]=Gathering
        [SerializeField] private TMP_Text _completionHeaderText;
        [SerializeField] private GatheringCatalogUI _gatheringPanel;

        private CollectionTab _currentTab = CollectionTab.Gathering;

        // 전체 아이템 수 (낚시 도감 15 + 채집 도감 27)
        private const int FishTotalCount = 15;

        public int TotalDiscoveredCount => _gatheringCatalogManager != null
            ? _gatheringCatalogManager.DiscoveredCount
            : 0;

        public int TotalItemCount => (_gatheringCatalogManager != null ? _gatheringCatalogManager.TotalItemCount : 0) + FishTotalCount;

        public float OverallCompletionRate => TotalItemCount > 0 ? (float)TotalDiscoveredCount / TotalItemCount : 0f;

        private void Awake()
        {
            _screenType = ScreenType.Collection;
        }

        private void OnEnable()
        {
            GatheringCatalogManager.OnCatalogUpdated += OnCatalogUpdated;
            GatheringCatalogManager.OnMilestoneReached += OnMilestoneReached;
        }

        private void OnDisable()
        {
            GatheringCatalogManager.OnCatalogUpdated -= OnCatalogUpdated;
            GatheringCatalogManager.OnMilestoneReached -= OnMilestoneReached;
        }

        private void Start()
        {
            if (_gatheringCatalogManager == null)
                _gatheringCatalogManager = GatheringCatalogManager.Instance;

            SetupTabButtons();
            SwitchTab(CollectionTab.Gathering);
        }

        private void SetupTabButtons()
        {
            if (_tabButtons == null) return;
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int idx = i;
                _tabButtons[i]?.onClick.AddListener(() => SwitchTab((CollectionTab)idx));
            }
        }

        public void Open()
        {
            gameObject.SetActive(true);
            UpdateCompletionHeader();
            RefreshCurrentTab();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void SwitchTab(CollectionTab tab)
        {
            _currentTab = tab;

            if (_gatheringPanel != null)
                _gatheringPanel.gameObject.SetActive(tab == CollectionTab.Gathering);

            UpdateCompletionHeader();
            RefreshCurrentTab();
        }

        private void RefreshCurrentTab()
        {
            if (_currentTab == CollectionTab.Gathering)
                _gatheringPanel?.Refresh();
        }

        private void UpdateCompletionHeader()
        {
            if (_completionHeaderText == null) return;
            int discovered = TotalDiscoveredCount;
            int total = TotalItemCount;
            float rate = total > 0 ? (float)discovered / total * 100f : 0f;
            _completionHeaderText.text = $"전체 수집 도감 {discovered}/{total} ({rate:F1}%)";
        }

        private void OnCatalogUpdated(string itemId, GatheringCatalogEntry entry)
        {
            UpdateCompletionHeader();
            if (_currentTab == CollectionTab.Gathering)
                _gatheringPanel?.Refresh();
        }

        private void OnMilestoneReached(int count)
        {
            UpdateCompletionHeader();
        }
    }
}
