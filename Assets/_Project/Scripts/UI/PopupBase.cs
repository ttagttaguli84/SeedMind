// S-08: Popup 추상 기반 클래스
// -> see docs/systems/ui-architecture.md 섹션 1.5
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SeedMind.UI
{
    /// <summary>
    /// 모든 Popup의 Show/Hide 생명주기를 통일하는 추상 기반 클래스.
    /// Screen 위에 중첩 표시되며, PopupQueue에 의해 우선순위 관리.
    /// </summary>
    public abstract class PopupBase : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected Image _backgroundDimmer;
        [SerializeField] protected bool _closeOnBackgroundClick = true;
        [SerializeField] protected float _fadeInDuration = 0.2f;
            // -> see docs/systems/ui-architecture.md 섹션 1.5
        [SerializeField] protected float _fadeOutDuration = 0.15f;
            // -> see docs/systems/ui-architecture.md 섹션 1.5

        public Action OnConfirm;
        public Action OnCancel;

        public bool IsVisible { get; protected set; }

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual IEnumerator Show()
        {
            if (IsVisible) yield break;
            gameObject.SetActive(true);
            OnBeforeShow();
            yield return StartCoroutine(FadeCanvas(0f, 1f, _fadeInDuration));
            IsVisible = true;
            OnAfterShow();
            UIEvents.RaisePopupShown(this);
        }

        public virtual IEnumerator Hide()
        {
            if (!IsVisible) yield break;
            OnBeforeHide();
            yield return StartCoroutine(FadeCanvas(1f, 0f, _fadeOutDuration));
            IsVisible = false;
            OnAfterHide();
            UIEvents.RaisePopupHidden(this);
            gameObject.SetActive(false);
        }

        protected virtual void OnBeforeShow() { }
        protected virtual void OnAfterShow() { }
        protected virtual void OnBeforeHide() { }
        protected virtual void OnAfterHide() { }

        private IEnumerator FadeCanvas(float from, float to, float duration)
        {
            if (_canvasGroup == null) yield break;
            float elapsed = 0f;
            _canvasGroup.alpha = from;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = to;
        }
    }
}
