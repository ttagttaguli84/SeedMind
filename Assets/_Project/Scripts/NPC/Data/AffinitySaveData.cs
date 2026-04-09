// S-03: NPC 친밀도 세이브 데이터
// -> see docs/systems/blacksmith-architecture.md 섹션 5.5
namespace SeedMind.NPC
{
    [System.Serializable]
    public class AffinitySaveData
    {
        public AffinityEntry[] entries;
    }

    [System.Serializable]
    public class AffinityEntry
    {
        public string npcId;                   // "npc_cheolsu"
        public int affinityValue;              // 현재 친밀도 수치
        public int lastVisitDay;               // 마지막 방문 일차
                                               // (CanGiveDailyAffinity/MarkDailyVisit 중복 방지,
                                               // -> see blacksmith-architecture.md 섹션 5.5)
        public string[] triggeredDialogueIds;  // 발동된 대화 ID 목록
                                               // (-> see blacksmith-architecture.md 섹션 5.5)
    }
}
