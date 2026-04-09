// S-09: 업적 시스템 정적 이벤트 허브
// -> see docs/systems/achievement-architecture.md 섹션 3.3
using SeedMind.Achievement.Data;

namespace SeedMind.Achievement
{
    public static class AchievementEvents
    {
        /// <summary>업적 달성 시 발행. UI 토스트 트리거용.</summary>
        public static event System.Action<AchievementData> OnAchievementUnlocked;

        /// <summary>업적 진행도 갱신 시 발행. UI 프로그레스 바 갱신용.</summary>
        public static event System.Action<string, float> OnProgressUpdated;
        // string = achievementId, float = normalizedProgress (0.0~1.0)

        internal static void RaiseAchievementUnlocked(AchievementData data)
            => OnAchievementUnlocked?.Invoke(data);

        internal static void RaiseProgressUpdated(string id, float progress)
            => OnProgressUpdated?.Invoke(id, progress);
    }
}