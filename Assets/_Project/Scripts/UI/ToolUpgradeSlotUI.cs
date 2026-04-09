// S-10: 업그레이드 UI 개별 도구 슬롯
// -> see docs/systems/blacksmith-architecture.md 섹션 3.4
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Player.Data;

namespace SeedMind.UI
{
    /// <summary>
    /// 도구 업그레이드 UI에서 개별 도구의 상태를 표시하는 슬롯.
    /// </summary>
    public class ToolUpgradeSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _toolIcon;
        [SerializeField] private TMP_Text _toolName;
        [SerializeField] private Image _tierBadge;
        [SerializeField] private GameObject _upgradeArrow;
        [SerializeField] private GameObject _pendingOverlay;   // "업그레이드 중"
        [SerializeField] private TMP_Text _pendingDaysText;    // "잔여 N일"
        [SerializeField] private Button _selectButton;

        private ToolData _tool;
        private System.Action<ToolData> _onSelected;

        /// <summary>슬롯 초기화</summary>
        public void Setup(ToolData tool, UpgradeCheckResult checkResult,
                          PendingUpgrade pending, System.Action<ToolData> onSelected)
        {
            _tool = tool;
            _onSelected = onSelected;

            if (_toolName != null)
                _toolName.text = tool != null ? tool.displayName : "";

            bool isInProgress = pending != null;
            if (_pendingOverlay != null)
                _pendingOverlay.SetActive(isInProgress);

            if (_pendingDaysText != null && isInProgress)
                _pendingDaysText.text = $"잔여 {pending.remainingDays}일";

            if (_upgradeArrow != null)
                _upgradeArrow.SetActive(!isInProgress && checkResult.canUpgrade);

            // 등급 뱃지 색상: (-> see docs/systems/tool-upgrade.md 섹션 7.3)
            if (_tierBadge != null && tool != null)
            {
                _tierBadge.color = tool.tier switch
                {
                    1 => Color.gray,
                    2 => Color.cyan,
                    3 => Color.yellow,
                    _ => Color.white,
                };
            }

            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(() => _onSelected?.Invoke(_tool));
                _selectButton.interactable = !isInProgress;
            }
        }

        public void SetSelected(bool selected)
        {
            if (_tierBadge != null)
                _tierBadge.color = selected ? Color.green : Color.gray;
        }
    }
}
