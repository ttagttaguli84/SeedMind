// S-02: 세이브/로드 시스템의 정적 이벤트 허브
// -> see docs/systems/save-load-architecture.md 섹션 5.1
namespace SeedMind.Save
{
    using System;

    /// <summary>
    /// 세이브/로드 시스템의 정적 이벤트 허브.
    /// UI, 자동저장 트리거 등 외부 시스템이 구독한다.
    /// </summary>
    public static class SaveEvents
    {
        // --- 저장 이벤트 ---
        public static event Action<int> OnSaveStarted;           // slotIndex
        public static event Action<int> OnSaveCompleted;         // slotIndex
        public static event Action<int, string> OnSaveFailed;    // slotIndex, errorMessage

        // --- 로드 이벤트 ---
        public static event Action<int> OnLoadStarted;           // slotIndex
        public static event Action<int> OnLoadCompleted;         // slotIndex
        public static event Action<int, string> OnLoadFailed;    // slotIndex, errorMessage

        // --- 자동저장 이벤트 ---
        public static event Action<string> OnAutoSaveTriggered;  // reason

        // --- 발행 메서드 ---
        internal static void RaiseSaveStarted(int slot) => OnSaveStarted?.Invoke(slot);
        internal static void RaiseSaveCompleted(int slot) => OnSaveCompleted?.Invoke(slot);
        internal static void RaiseSaveFailed(int slot, string msg) => OnSaveFailed?.Invoke(slot, msg);
        internal static void RaiseLoadStarted(int slot) => OnLoadStarted?.Invoke(slot);
        internal static void RaiseLoadCompleted(int slot) => OnLoadCompleted?.Invoke(slot);
        internal static void RaiseLoadFailed(int slot, string msg) => OnLoadFailed?.Invoke(slot, msg);
        internal static void RaiseAutoSaveTriggered(string reason) => OnAutoSaveTriggered?.Invoke(reason);
    }
}
