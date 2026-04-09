// S-09: 대장간 도구 업그레이드 화면 (ScreenBase 파생)
// -> see docs/systems/blacksmith-architecture.md 섹션 3.3
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Player;
using SeedMind.Player.Data;

namespace SeedMind.UI
{
    /// <summary>업그레이드 UI 콜백 결과</summary>
    public enum UpgradeUIResult
    {
        Cancelled  = 0,
        Confirmed  = 1,
        InProgress = 2,
    }

    /// <summary>
    /// 대장간 도구 업그레이드 화면.
    /// ScreenBase를 상속하여 UIManager Screen FSM에 통합된다.
    /// ScreenType.ToolUpgrade = 11 (-> see ui-architecture.md 섹션 1.2)
    /// </summary>
    public class ToolUpgradeUI : ScreenBase
    {
        [Header("도구 목록")]
        [SerializeField] private Transform _toolSlotContainer;
        [SerializeField] private ToolUpgradeSlotUI _toolSlotPrefab;

        [Header("비교 뷰")]
        [SerializeField] private ToolComparisonPanel _currentToolPanel;
        [SerializeField] private ToolComparisonPanel _upgradedToolPanel;

        [Header("비용 패널")]
        [SerializeField] private TMP_Text _goldCostText;
        [SerializeField] private Image _goldIcon;
        [SerializeField] private Transform _materialSlotContainer;
        [SerializeField] private MaterialSlotUI _materialSlotPrefab;
        [SerializeField] private TMP_Text _timeCostText;
        [SerializeField] private TMP_Text _levelRequirementText;

        [Header("버튼")]
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private Button _closeButton;

        [Header("확인 팝업")]
        [SerializeField] private GameObject _confirmPopup;
        [SerializeField] private TMP_Text _confirmText;
        [SerializeField] private Button _confirmYesButton;
        [SerializeField] private Button _confirmNoButton;

        // --- 콜백 ---
        public event Action<UpgradeUIResult> OnResultCallback;

        private ToolData _selectedTool;
        private ToolUpgradeSystem _upgradeSystem;

        private void Awake()
        {
            _upgradeSystem = FindObjectOfType<ToolUpgradeSystem>();
        }

        // --- ScreenBase 오버라이드 ---

        public override void OnBeforeOpen()
        {
            RefreshToolList();
            _selectedTool = null;
        }

        public override void OnAfterOpen()
        {
            // 첫 번째 슬롯에 포커스
        }

        public override void OnBeforeClose()
        {
            _selectedTool = null;
            if (_confirmPopup != null)
                _confirmPopup.SetActive(false);
        }

        // --- 공개 메서드 ---

        public void RefreshToolList()
        {
            // TODO: ToolUpgradeSystem에서 업그레이드 가능 도구 목록 로드
            // 비용 수치: (-> see docs/systems/tool-upgrade.md 섹션 2.1)
        }

        public void SelectTool(ToolData tool)
        {
            _selectedTool = tool;
            RefreshComparisonView();
            RefreshCostPanel();
        }

        public void RefreshComparisonView()
        {
            if (_selectedTool == null) return;
            if (_currentToolPanel != null)
                _currentToolPanel.Setup(_selectedTool);
            if (_upgradedToolPanel != null && _selectedTool.nextTier != null)
                _upgradedToolPanel.Setup(_selectedTool.nextTier);
        }

        public void RefreshCostPanel()
        {
            if (_selectedTool == null) return;
            // 비용 수치: (-> see docs/systems/tool-upgrade.md 섹션 2.1)
            if (_goldCostText != null)
                _goldCostText.text = $"{_selectedTool.upgradeGoldCost}G";
            if (_timeCostText != null)
                _timeCostText.text = $"{_selectedTool.upgradeTimeDays}일";
            if (_levelRequirementText != null)
                _levelRequirementText.text = $"Lv.{_selectedTool.requiredLevel} 이상";
        }

        public void OnUpgradeButtonClicked()
        {
            if (_selectedTool == null || _upgradeSystem == null) return;

            // 조건 검증 순서: 등급 → 진행 중 → 레벨 → 골드/재료
            // -> see docs/mcp/tool-upgrade-design-analysis.md 섹션 5.3
            var check = _upgradeSystem.CanUpgrade(_selectedTool);
            if (!check.canUpgrade)
            {
                Debug.Log($"[ToolUpgradeUI] 업그레이드 불가: {check.failReason}");
                return;
            }

            ShowConfirmPopup();
        }

        public void OnConfirmUpgrade()
        {
            if (_selectedTool == null || _upgradeSystem == null) return;
            _upgradeSystem.StartUpgrade(_selectedTool);
            HideConfirmPopup();
            OnResultCallback?.Invoke(UpgradeUIResult.Confirmed);
            Hide();
        }

        public void OnCancelUpgrade()
        {
            HideConfirmPopup();
            OnResultCallback?.Invoke(UpgradeUIResult.Cancelled);
        }

        // --- 내부 헬퍼 ---

        private void ShowConfirmPopup()
        {
            if (_confirmPopup == null) return;
            if (_confirmText != null && _selectedTool != null)
                _confirmText.text = $"{_selectedTool.displayName}을(를) 업그레이드하시겠습니까?";
            _confirmPopup.SetActive(true);
        }

        private void HideConfirmPopup()
        {
            if (_confirmPopup != null)
                _confirmPopup.SetActive(false);
        }
    }
}
