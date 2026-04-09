// LevelBarUI — HUD 레벨/경험치 바 표시 컴포넌트
// -> see docs/systems/progression-architecture.md 섹션 6
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Level;

namespace SeedMind.UI
{
    public class LevelBarUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Slider _expSlider;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _expText;

        private void OnEnable()
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnExpGained += HandleExpGained;
                ProgressionManager.Instance.OnLevelUp   += HandleLevelUp;
            }
        }

        private void OnDisable()
        {
            if (ProgressionManager.Instance != null)
            {
                ProgressionManager.Instance.OnExpGained -= HandleExpGained;
                ProgressionManager.Instance.OnLevelUp   -= HandleLevelUp;
            }
        }

        private void Start()
        {
            RefreshUI();
        }

        private void HandleExpGained(ExpGainInfo info)
        {
            RefreshUI();
        }

        private void HandleLevelUp(LevelUpInfo info)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            var pm = ProgressionManager.Instance;
            if (pm == null) return;

            if (_levelText != null)
                _levelText.text = $"Lv.{pm.CurrentLevel}";

            if (_expSlider != null)
                _expSlider.value = pm.ExpProgress;

            if (_expText != null)
                _expText.text = pm.IsMaxLevel
                    ? "MAX"
                    : $"{pm.CurrentExp}/{pm.CurrentExp + pm.ExpToNextLevel}";
        }
    }
}
