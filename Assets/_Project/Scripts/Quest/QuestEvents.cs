// S-13: 퀘스트 정적 이벤트 허브
// -> see docs/systems/quest-architecture.md 섹션 6.2
using System;

namespace SeedMind.Quest
{
    public static class QuestEvents
    {
        // --- 상태 변경 ---
        public static event Action<QuestInstance> OnQuestUnlocked;
        public static event Action<QuestInstance> OnQuestActivated;
        public static event Action<QuestInstance> OnQuestCompleted;
        public static event Action<QuestInstance> OnQuestRewarded;
        public static event Action<QuestInstance> OnQuestFailed;

        // --- 진행도 ---
        public static event Action<QuestInstance, int> OnObjectiveProgress;

        // --- 일일 목표 ---
        public static event Action<QuestInstance[]> OnDailyQuestsSelected;

        // --- NPC 의뢰 ---
        public static event Action<QuestInstance> OnNPCRequestAvailable;

        // --- Raise 메서드 ---
        public static void RaiseQuestUnlocked(QuestInstance q)
            => OnQuestUnlocked?.Invoke(q);
        public static void RaiseQuestActivated(QuestInstance q)
            => OnQuestActivated?.Invoke(q);
        public static void RaiseQuestCompleted(QuestInstance q)
            => OnQuestCompleted?.Invoke(q);
        public static void RaiseQuestRewarded(QuestInstance q)
            => OnQuestRewarded?.Invoke(q);
        public static void RaiseQuestFailed(QuestInstance q)
            => OnQuestFailed?.Invoke(q);
        public static void RaiseObjectiveProgress(QuestInstance q, int idx)
            => OnObjectiveProgress?.Invoke(q, idx);
        public static void RaiseDailyQuestsSelected(QuestInstance[] quests)
            => OnDailyQuestsSelected?.Invoke(quests);
        public static void RaiseNPCRequestAvailable(QuestInstance q)
            => OnNPCRequestAvailable?.Invoke(q);
    }
}