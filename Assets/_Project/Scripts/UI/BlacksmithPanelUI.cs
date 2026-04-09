// T-08: 대장간 UI 패널 (MonoBehaviour)
// -> see docs/systems/tool-upgrade.md 섹션 6.2 (상호작용 흐름)
// -> see docs/systems/tool-upgrade-architecture.md 섹션 5.1
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Player;
using SeedMind.Player.Data;

namespace SeedMind.UI
{
    /// <summary>
    /// 대장간 NPC 상호작용 시 표시되는 업그레이드 UI 패널.
    /// 도구 업그레이드 / 재료 구매 / 도구 수령 3개 탭을 제공한다.
    /// -> see docs/systems/tool-upgrade.md 섹션 6.2
    /// </summary>
    public class BlacksmithPanelUI : MonoBehaviour
    {
        [Header("시스템 참조")]
        [SerializeField] private ToolUpgradeSystem _upgradeSystem;
        [SerializeField] private InventoryManager _inventoryManager;

        [Header("탭 패널")]
        [SerializeField] private GameObject _upgradePanel;
        [SerializeField] private GameObject _materialShopPanel;
        [SerializeField] private GameObject _collectPanel;

        [Header("확인 팝업")]
        [SerializeField] private GameObject _confirmPopup;
        [SerializeField] private TextMeshProUGUI _confirmDescText;
        [SerializeField] private TextMeshProUGUI _confirmCostText;

        private ToolData _selectedTool;

        // ── 공개 API ────────────────────────────────────────────────

        public void Show()
        {
            gameObject.SetActive(true);
            ShowUpgradeMenu();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ShowUpgradeMenu()
        {
            SetActivePanel(_upgradePanel);
            RefreshUpgradePanel();
        }

        public void ShowMaterialShop()
        {
            SetActivePanel(_materialShopPanel);
        }

        public void ShowCollectMenu()
        {
            SetActivePanel(_collectPanel);
            RefreshCollectPanel();
        }

        // ── 업그레이드 패널 ─────────────────────────────────────────

        public void OnToolSelected(ToolData tool)
        {
            _selectedTool = tool;
        }

        public void OnUpgradeConfirmed()
        {
            if (_selectedTool == null || _upgradeSystem == null) return;
            _upgradeSystem.StartUpgrade(_selectedTool);
            HideConfirmPopup();
            RefreshUpgradePanel();
        }

        public void OnUpgradeCancelled()
        {
            HideConfirmPopup();
        }

        public void OnUpgradeButtonClicked(ToolData tool)
        {
            if (tool == null || _upgradeSystem == null) return;
            var check = _upgradeSystem.CanUpgrade(tool);
            if (!check.canUpgrade)
            {
                Debug.Log($"[BlacksmithPanelUI] 업그레이드 불가: {check.failReason}");
                return;
            }
            _selectedTool = tool;
            ShowConfirmPopup(tool, check.cost);
        }

        // ── 이벤트 구독 ─────────────────────────────────────────────

        private void OnEnable()
        {
            ToolUpgradeEvents.OnUpgradeStarted += OnUpgradeEvent;
            ToolUpgradeEvents.OnUpgradeCompleted += OnUpgradeEvent;
        }

        private void OnDisable()
        {
            ToolUpgradeEvents.OnUpgradeStarted -= OnUpgradeEvent;
            ToolUpgradeEvents.OnUpgradeCompleted -= OnUpgradeEvent;
        }

        // ── 내부 헬퍼 ───────────────────────────────────────────────

        private void SetActivePanel(GameObject target)
        {
            if (_upgradePanel) _upgradePanel.SetActive(_upgradePanel == target);
            if (_materialShopPanel) _materialShopPanel.SetActive(_materialShopPanel == target);
            if (_collectPanel) _collectPanel.SetActive(_collectPanel == target);
        }

        private void RefreshUpgradePanel()
        {
            // TODO: 각 UpgradeSlot UI 갱신 — ToolUpgradeSystem.CanUpgrade 결과 반영
        }

        private void RefreshCollectPanel()
        {
            // TODO: 완료된 업그레이드 목록 갱신
        }

        private void ShowConfirmPopup(ToolData tool, UpgradeCostInfo cost)
        {
            if (_confirmPopup == null) return;
            if (_confirmDescText != null)
                _confirmDescText.text = $"{tool.displayName}을(를) 업그레이드합니다.";
            if (_confirmCostText != null)
                _confirmCostText.text = $"비용: {cost.goldCost}G | 소요: {cost.timeDays}일";
            _confirmPopup.SetActive(true);
        }

        private void HideConfirmPopup()
        {
            if (_confirmPopup != null)
                _confirmPopup.SetActive(false);
        }

        private void OnUpgradeEvent(ToolUpgradeInfo info)
        {
            RefreshUpgradePanel();
        }
    }
}
