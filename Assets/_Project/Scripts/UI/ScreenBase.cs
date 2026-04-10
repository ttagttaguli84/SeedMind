// S-07: Screen 추상 기반 클래스 (Template Method 패턴)
// -> see docs/systems/ui-architecture.md 섹션 1.4
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SeedMind.UI
{
    /// <summary>
    /// 모든 Screen의 Open/Close 생명주기를 통일하는 추상 기반 클래스.
    /// 파생 클래스: InventoryUI, ShopUI, QuestUI, AchievementPanel,
    ///              MenuUI, SaveLoadUI, DialogueUI, ProcessingUI
    /// </summary>
    public abstract class ScreenBase : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected Selectable _firstSelected;
        [SerializeField] protected ScreenType _screenType;
        [SerializeField] protected bool _pauseGameTime;
        [SerializeField] protected float _fadeInDuration = 0.15f;
            // -> see docs/systems/ui-architecture.md 섹션 1.4
        [SerializeField] protected float _fadeOutDuration = 0.1f;
            // -> see docs/systems/ui-architecture.md 섹션 1.4

        public ScreenType ScreenType => _screenType;
        public bool IsOpen { get; protected set; }
        public bool PausesGameTime => _pauseGameTime;

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
                _canvasGroup = GetComponent<CanvasGroup>();
            // 초기 상태: 숨김 (Open() 호출 전까지 화면에 표시되지 않음)
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            if (_screenType != ScreenType.None && UIManager.Instance != null)
                UIManager.Instance.RegisterScreen(_screenType, this);
        }

        /// <summary>화면 열기 (페이드 인)</summary>
        public virtual IEnumerator Open()
        {
            if (IsOpen) yield break;
            OnBeforeOpen();
            yield return StartCoroutine(FadeCanvas(0f, 1f, _fadeInDuration));
            IsOpen = true;
            OnAfterOpen();
            UIEvents.RaiseScreenOpened(_screenType);
        }

        /// <summary>화면 닫기 (페이드 아웃)</summary>
        public virtual IEnumerator Close()
        {
            if (!IsOpen) yield break;
            OnBeforeClose();
            yield return StartCoroutine(FadeCanvas(1f, 0f, _fadeOutDuration));
            IsOpen = false;
            OnAfterClose();
            UIEvents.RaiseScreenClosed(_screenType);
        }

        public virtual void OnBeforeOpen() { }
        public virtual void OnAfterOpen() { }
        public virtual void OnBeforeClose() { }
        public virtual void OnAfterClose() { }

        // 하위 호환 stub (blacksmith-tasks.md 등 기존 사용처)
        public virtual void Show()
        {
            if (gameObject.activeSelf) return;
            gameObject.SetActive(true);
            OnBeforeOpen();
            IsOpen = true;
            OnAfterOpen();
        }

        public virtual void Hide()
        {
            OnBeforeClose();
            IsOpen = false;
            gameObject.SetActive(false);
            OnAfterClose();
        }

        protected void SetInteractable(bool value)
        {
            if (_canvasGroup == null) return;
            _canvasGroup.interactable = value;
            _canvasGroup.blocksRaycasts = value;
        }

        protected void SetBlocksRaycasts(bool value)
        {
            if (_canvasGroup != null)
                _canvasGroup.blocksRaycasts = value;
        }

        private IEnumerator FadeCanvas(float from, float to, float duration)
        {
            if (_canvasGroup == null) yield break;
            float elapsed = 0f;
            _canvasGroup.alpha = from;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = to;
            bool visible = to > 0.5f;
            _canvasGroup.interactable = visible;
            _canvasGroup.blocksRaycasts = visible;
        }
    }
}
