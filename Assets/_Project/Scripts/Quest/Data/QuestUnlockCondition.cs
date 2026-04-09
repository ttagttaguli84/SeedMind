// S-09: 퀘스트 해금 조건 (직렬화 클래스)
// -> see docs/systems/quest-architecture.md 섹션 4.4
using SeedMind.Core;

namespace SeedMind.Quest.Data
{
    [System.Serializable]
    public class QuestUnlockCondition
    {
        public UnlockConditionType type;
        public string stringParam;                // 문자열 파라미터 (퀘스트ID, 시설ID)
        public int intParam;                      // 정수 파라미터 (레벨, 일차)
                                                  // -> see docs/systems/quest-system.md 섹션 2.4
        public Season seasonParam;                // 계절 파라미터 (Season 조건용)
    }
}