// S-08: 퀘스트 보상 데이터 (직렬화 클래스)
// -> see docs/systems/quest-architecture.md 섹션 4.3
namespace SeedMind.Quest.Data
{
    [System.Serializable]
    public class QuestRewardData
    {
        public RewardType type;
        public int amount;                        // 수량 (-> see docs/systems/quest-system.md 섹션 7)
        public string targetId;                   // 대상 ID (Gold/XP는 "")
        public bool scaledByLevel;                // 레벨 스케일 적용 여부
                                                  // -> see docs/systems/quest-system.md 섹션 5.2
    }
}