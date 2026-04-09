// S-10: UI 정적 이벤트 허브
// -> see docs/systems/ui-architecture.md 섹션 4.1
using System;

namespace SeedMind.UI
{
    /// <summary>
    /// UI 시스템의 외부 발행 이벤트.
    /// 다른 시스템이 UI 상태 변화를 감지하기 위해 구독한다.
    /// </summary>
    public static class UIEvents
    {
        // --- Screen 상태 ---
        public static Action<ScreenType> OnScreenOpened;
        public static Action<ScreenType> OnScreenClosed;

        // --- Popup 상태 ---
        public static Action<PopupBase> OnPopupShown;
        public static Action<PopupBase> OnPopupHidden;

        // --- 입력 모드 ---
        public static Action<UIInputMode> OnInputModeChanged;

        // --- 알림 ---
        public static Action<NotificationData> OnNotificationShown;
        public static Action OnAllNotificationsCleared;

        // --- HUD 갱신 ---
        public static Action OnHUDRefreshRequested;

        // --- Raise 메서드 ---
        public static void RaiseScreenOpened(ScreenType t)
            => OnScreenOpened?.Invoke(t);
        public static void RaiseScreenClosed(ScreenType t)
            => OnScreenClosed?.Invoke(t);
        public static void RaisePopupShown(PopupBase p)
            => OnPopupShown?.Invoke(p);
        public static void RaisePopupHidden(PopupBase p)
            => OnPopupHidden?.Invoke(p);
        public static void RaiseInputModeChanged(UIInputMode m)
            => OnInputModeChanged?.Invoke(m);
        public static void RaiseNotificationShown(NotificationData d)
            => OnNotificationShown?.Invoke(d);
        public static void RaiseAllNotificationsCleared()
            => OnAllNotificationsCleared?.Invoke();
        public static void RaiseHUDRefreshRequested()
            => OnHUDRefreshRequested?.Invoke();
    }
}
