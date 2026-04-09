// S-07: 도구 현재/업그레이드 후 스탯 비교 패널
// -> see docs/systems/blacksmith-architecture.md 섹션 3.1
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Player.Data;

namespace SeedMind.UI
{
    /// <summary>
    /// 도구의 현재 또는 업그레이드 후 스탯을 표시하는 패널.
    /// ToolUpgradeUI의 ComparisonView에서 좌(현재)/우(업그레이드 후) 한 쌍으로 사용.
    /// </summary>
    public class ToolComparisonPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _label;         // "현재" / "업그레이드 후"
        [SerializeField] private Image _toolIcon;
        [SerializeField] private TMP_Text _toolName;
        [SerializeField] private TMP_Text _tierText;
        [SerializeField] private TMP_Text _rangeStat;
        [SerializeField] private TMP_Text _speedStat;
        [SerializeField] private TMP_Text _specialStat;

        /// <summary>스탯 패널을 ToolData로 초기화</summary>
        public void Setup(ToolData tool)
        {
            if (tool == null) return;
            if (_toolName != null)  _toolName.text  = tool.displayName;
            if (_tierText != null)  _tierText.text  = $"Tier {tool.tier}";
            // 스탯 수치: ToolData SO 직접 필드
            // -> see docs/systems/tool-upgrade.md 섹션 3.1~3.3
            if (_rangeStat != null) _rangeStat.text  = $"범위: {tool.range}";
            if (_speedStat != null) _speedStat.text  = $"속도: {tool.useSpeed:F1}";
        }

        /// <summary>개선된 수치를 초록색, 악화된 수치를 빨간색으로 표시</summary>
        public void HighlightChanges(ToolComparisonPanel other)
        {
            // TODO: 수치 비교 후 색상 적용
        }
    }
}
