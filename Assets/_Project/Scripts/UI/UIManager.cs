// S-11: UI 시스템 중앙 관리자 (Singleton)
// -> see docs/systems/ui-architecture.md 섹션 1.3
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeedMind.UI
{
    /// <summary>
    /// Screen FSM 관리, 화면 전환 중앙 제어, PopupQueue 조율.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // --- 상태 ---
        private ScreenType _currentScreen = ScreenType.None;
        private ScreenType _previousScreen = ScreenType.None;
        private bool _isTransitioning;

        // --- Screen 레지스트리 ---
        private readonly Dictionary<ScreenType, ScreenBase> _screens
            = new Dictionary<ScreenType, ScreenBase>();

        // --- Popup 큐 ---
        private PopupQueue _popupQueue = new PopupQueue();
        private PopupBase _activePopup;

        // --- 참조 ---
        [SerializeField] private HUDController _hudController;
        [SerializeField] private NotificationManager _notificationManager;

        // --- 읽기 전용 프로퍼티 ---
        public ScreenType CurrentScreen => _currentScreen;
        public bool IsScreenOpen => _currentScreen != ScreenType.None
                                 && _currentScreen != ScreenType.Farming;
        public bool IsTransitioning => _isTransitioning;
        public bool IsPopupActive => _activePopup != null;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // --- 화면 전환 API ---
        public void OpenScreen(ScreenType type)
        {
            if (_isTransitioning || type == _currentScreen) return;
            StartCoroutine(TransitionScreen(_currentScreen, type));
        }

        public void CloseCurrentScreen()
        {
            if (_isTransitioning || !IsScreenOpen) return;
            StartCoroutine(TransitionScreen(_currentScreen, ScreenType.None));
        }

        public void ToggleScreen(ScreenType type)
        {
            if (_currentScreen == type) CloseCurrentScreen();
            else OpenScreen(type);
        }

        public void ReturnToPreviousScreen()
        {
            OpenScreen(_previousScreen);
        }

        // --- 팝업 API ---
        public void ShowPopup(PopupBase popup, PopupPriority priority = PopupPriority.Normal)
        {
            if (_activePopup == null)
                StartCoroutine(ShowPopupCoroutine(popup));
            else
                _popupQueue.Enqueue(popup, priority);
        }

        public void ClosePopup()
        {
            if (_activePopup == null) return;
            StartCoroutine(ClosePopupCoroutine());
        }

        public void CloseAllPopups()
        {
            _popupQueue.Clear();
            ClosePopup();
        }

        // --- Screen 등록 ---
        public void RegisterScreen(ScreenType type, ScreenBase screen)
        {
            if (!_screens.ContainsKey(type))
                _screens[type] = screen;
        }

        public void UnregisterScreen(ScreenType type)
        {
            _screens.Remove(type);
        }

        // --- 유틸리티 ---
        public void SetInputMode(UIInputMode mode)
        {
            UIEvents.RaiseInputModeChanged(mode);
        }

        // --- 내부 메서드 ---
        private IEnumerator TransitionScreen(ScreenType from, ScreenType to)
        {
            _isTransitioning = true;

            // 현재 화면 닫기
            if (from != ScreenType.None && from != ScreenType.Farming
                && _screens.TryGetValue(from, out var fromScreen))
            {
                yield return StartCoroutine(fromScreen.Close());
            }

            _previousScreen = from;
            _currentScreen = to;

            // 새 화면 열기
            if (to != ScreenType.None && to != ScreenType.Farming
                && _screens.TryGetValue(to, out var toScreen))
            {
                yield return StartCoroutine(toScreen.Open());
                UIInputMode mode = toScreen.PausesGameTime ? UIInputMode.Popup : UIInputMode.UIScreen;
                SetInputMode(mode);
            }
            else
            {
                SetInputMode(UIInputMode.Gameplay);
            }

            _isTransitioning = false;
        }

        private IEnumerator ShowPopupCoroutine(PopupBase popup)
        {
            _activePopup = popup;
            SetInputMode(UIInputMode.Popup);
            yield return StartCoroutine(popup.Show());
        }

        private IEnumerator ClosePopupCoroutine()
        {
            yield return StartCoroutine(_activePopup.Hide());
            _activePopup = null;

            if (!_popupQueue.IsEmpty)
            {
                var next = _popupQueue.Dequeue();
                if (next.HasValue)
                    StartCoroutine(ShowPopupCoroutine(next.Value.Popup));
            }
            else
            {
                UpdateInputMode();
            }
        }

        private void UpdateInputMode()
        {
            if (IsScreenOpen && _screens.TryGetValue(_currentScreen, out var screen))
            {
                SetInputMode(screen.PausesGameTime ? UIInputMode.Popup : UIInputMode.UIScreen);
            }
            else
            {
                SetInputMode(UIInputMode.Gameplay);
            }
        }
    }
}
