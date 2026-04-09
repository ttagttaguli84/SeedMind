// S-12: 업적 달성 토스트 알림 UI
// -> see docs/systems/achievement-architecture.md 섹션 8.2
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Achievement;
using SeedMind.Achievement.Data;

namespace SeedMind.UI
{
    public class AchievementToastUI : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Animator _animator;

        [SerializeField] private float _displayDuration = 4f;
            // -> see docs/systems/achievement-system.md 섹션 5.4
        [SerializeField] private float _hiddenDisplayDuration = 6f;
            // -> see docs/systems/achievement-system.md 섹션 5.4
        [SerializeField] private float _slideInDuration = 0.3f;
        [SerializeField] private float _slideOutDuration = 0.3f;

        private Queue<AchievementData> _toastQueue = new Queue<AchievementData>();
        private bool _isShowing;

        private void OnEnable()
        {
            AchievementEvents.OnAchievementUnlocked += ShowToast;
        }

        private void OnDisable()
        {
            AchievementEvents.OnAchievementUnlocked -= ShowToast;
        }

        public void ShowToast(AchievementData data)
        {
            _toastQueue.Enqueue(data);
            if (!_isShowing) StartCoroutine(ProcessQueue());
        }

        private IEnumerator ProcessQueue()
        {
            _isShowing = true;
            while (_toastQueue.Count > 0)
            {
                var data = _toastQueue.Dequeue();
                yield return StartCoroutine(DisplaySingle(data));
            }
            _isShowing = false;
        }

        private IEnumerator DisplaySingle(AchievementData data)
        {
            _titleText.text = data.displayName;
            _descriptionText.text = data.description;
            if (data.icon != null) _iconImage.sprite = data.icon;

            gameObject.SetActive(true);
            yield return new WaitForSeconds(_slideInDuration);

            float duration = data.isHidden ? _hiddenDisplayDuration : _displayDuration;
            yield return new WaitForSeconds(duration);

            yield return new WaitForSeconds(_slideOutDuration);
            gameObject.SetActive(false);
        }
    }
}
