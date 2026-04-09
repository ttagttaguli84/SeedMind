// S-13: 업적 목록 개별 항목 UI
// -> see docs/systems/achievement-architecture.md 섹션 8.3
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Achievement;
using SeedMind.Achievement.Data;

namespace SeedMind.UI
{
    public class AchievementItemUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private GameObject _completedOverlay;
        [SerializeField] private GameObject _hiddenOverlay;

        public void Setup(AchievementData data, AchievementRecord record)
        {
            if (data.isHidden && !record.isUnlocked)
            {
                SetHiddenState();
                return;
            }

            _titleText.text = data.displayName;
            _descriptionText.text = data.description;
            if (data.icon != null) _iconImage.sprite = data.icon;

            if (record.isUnlocked)
                SetUnlockedState();
            else
                SetProgressState(data, record);
        }

        private void SetUnlockedState()
        {
            _completedOverlay.SetActive(true);
            _hiddenOverlay.SetActive(false);
            _progressBar.gameObject.SetActive(false);
        }

        private void SetProgressState(AchievementData data, AchievementRecord record)
        {
            _completedOverlay.SetActive(false);
            _hiddenOverlay.SetActive(false);
            _progressBar.gameObject.SetActive(true);

            int target = data.type == AchievementType.Single
                ? data.targetValue
                : (data.tiers.Length > 0 ? data.tiers[data.tiers.Length - 1].targetValue : 0);

            _progressBar.value = record.GetNormalizedProgress(target);
            _progressText.text = $"{record.currentProgress}/{target}";
        }

        private void SetHiddenState()
        {
            _titleText.text = "???";
            _descriptionText.text = "숨겨진 업적";
            _completedOverlay.SetActive(false);
            _hiddenOverlay.SetActive(true);
            _progressBar.gameObject.SetActive(false);
        }
    }
}
