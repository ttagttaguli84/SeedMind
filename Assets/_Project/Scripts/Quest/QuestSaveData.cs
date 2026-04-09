// S-11: 퀘스트 세이브 데이터 (전체 Serializable 클래스들)
// -> see docs/systems/quest-architecture.md 섹션 8.2
using System.Collections.Generic;

namespace SeedMind.Quest
{
    [System.Serializable]
    public class QuestSaveData
    {
        public QuestProgressEntry[] questProgress;
        public string[] completedQuestIds;
        public DailyQuestSaveState dailyState;
        public NPCRequestSaveState npcRequestState;
        public CumulativeStatsSaveData cumulativeStats;
    }

    [System.Serializable]
    public class QuestProgressEntry
    {
        public string questId;
        public int status;                        // QuestStatus enum (int)
        public int[] objectiveProgress;
        public int acceptedDay;                   // -1 = 미수락
        public int completedDay;                  // -1 = 미완료
        public bool isTracked;
    }

    [System.Serializable]
    public class DailyQuestSaveState
    {
        public int lastSelectedDay;
        public string[] previousDailyIds;
        public string[] todayDailyIds;
    }

    [System.Serializable]
    public class NPCRequestSaveState
    {
        public Dictionary<string, int> cooldowns; // NPC ID -> 남은 쿨다운 일수
        public int activeRequestCount;
    }

    [System.Serializable]
    public class CumulativeStatsSaveData
    {
        public int totalHarvested;
        public int totalSold;
        public int totalProcessed;
        public int totalBuilt;
    }
}