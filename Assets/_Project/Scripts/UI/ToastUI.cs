// S-13: 토스트 UI 프리팹 컴포넌트
// -> see docs/systems/ui-architecture.md 섹션 3.4
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SeedMind.UI
{
    /// <summary>
    /// 개별 토스트 알림 UI. NotificationManager가 풀링하여 관리.
    /// </summary>
    public class ToastUI : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _iconImage;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Image _progressBar;
        [SerializeField] private CanvasGroup _canvasGroup;

        private float _totalDuration;
        private float _remainingTime;
        private bool _isActive;

        public void Setup(NotificationData data)
        {
            _totalDuration = data.Duration > 0 ? data.Duration : 3f;
            _remainingTime = _totalDuration;
            _isActive = true;

            if (_messageText != null) _messageText.text = data.Message;
            if (_iconImage != null) _iconImage.sprite = data.Icon;
            if (_backgroundImage != null && data.Color != default) _backgroundImage.color = data.Color;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
            if (_progressBar != null)
            {
                _progressBar.type = Image.Type.Filled;
                _progressBar.fillAmount = 1f;
            }
        }

        /// <summary>매 프레임 호출. false 반환 시 만료 → RetireToast 호출.</summary>
        public bool Tick(float deltaTime)
        {
            if (!_isActive) return false;
            _remainingTime -= deltaTime;
            if (_progressBar != null)
                _progressBar.fillAmount = Mathf.Clamp01(_remainingTime / _totalDuration);
            return _remainingTime > 0f;
        }

        /// <summary>즉시 숨김 (ClearAll 용)</summary>
        public void ForceHide()
        {
            _isActive = false;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        }

        public IEnumerator SlideIn()
        {
            if (_canvasGroup == null) yield break;
            _canvasGroup.alpha = 0f;
            float t = 0f;
            float dur = 0.25f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(t / dur);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
        }

        public IEnumerator SlideOut()
        {
            if (_canvasGroup == null) yield break;
            float t = 0f;
            float dur = 0.2f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(1f - t / dur);
                yield return null;
            }
            _canvasGroup.alpha = 0f;
        }
    }
}
