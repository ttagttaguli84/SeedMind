// S-10: 튜토리얼 전용 이벤트 허브
// -> see docs/systems/tutorial-architecture.md 섹션 9
// FarmEvents 패턴과 동일: event 키워드 없이 public static Action 사용
using System;

namespace SeedMind.Tutorial
{
    public static class TutorialEvents
    {
        // --- 시퀀스 ---
        public static Action<string> OnSequenceStarted;
        public static Action<string> OnSequenceCompleted;
        public static Action<string> OnSequenceSkipped;

        // --- 단계 ---
        public static Action<string, string> OnStepStarted;
        public static Action<string, string> OnStepCompleted;

        // --- 상황별 힌트 ---
        public static Action<string> OnContextHintShown;
        public static Action<string> OnContextHintDismissed;

        // --- 전체 ---
        public static Action OnAllTutorialsCompleted;
    }
}
