// S-12: 런타임 퀘스트 상태 래퍼
// -> see docs/systems/quest-architecture.md 섹션 5.1
using UnityEngine;
using SeedMind.Quest.Data;

namespace SeedMind.Quest
{
    public class QuestInstance
    {
        public QuestData Data { get; private set; }
        public QuestStatus Status { get; set; }
        public int[] ObjectiveProgress { get; private set; }
        public int AcceptedDay { get; set; }
        public int CompletedDay { get; set; }
        public bool IsTracked { get; set; }

        public QuestInstance(QuestData data)
        {
            Data = data;
            Status = QuestStatus.Locked;
            ObjectiveProgress = new int[data.objectives.Length];
            AcceptedDay = -1;
            CompletedDay = -1;
            IsTracked = false;
        }

        public void UpdateProgress(int objectiveIndex, int delta)
        {
            if (Status != QuestStatus.Active) return;
            ObjectiveProgress[objectiveIndex] += delta;
            int required = Data.objectives[objectiveIndex].requiredAmount;
            ObjectiveProgress[objectiveIndex] = Mathf.Min(
                ObjectiveProgress[objectiveIndex], required);
        }

        public bool IsObjectiveComplete(int objectiveIndex)
            => ObjectiveProgress[objectiveIndex]
               >= Data.objectives[objectiveIndex].requiredAmount;

        public bool AreAllObjectivesComplete()
        {
            for (int i = 0; i < ObjectiveProgress.Length; i++)
                if (!IsObjectiveComplete(i)) return false;
            return true;
        }

        public float GetOverallProgress()
        {
            if (Data.objectives.Length == 0) return 1f;
            float total = 0f;
            for (int i = 0; i < Data.objectives.Length; i++)
                total += (float)ObjectiveProgress[i]
                         / Data.objectives[i].requiredAmount;
            return total / Data.objectives.Length;
        }

        public int GetRemainingDays(int currentDay)
        {
            if (Data.timeLimitDays <= 0) return -1;
            return Data.timeLimitDays - (currentDay - AcceptedDay);
        }

        public bool IsExpired(int currentDay)
        {
            if (Data.timeLimitDays <= 0) return false;
            return GetRemainingDays(currentDay) <= 0;
        }
    }
}