// S-16: 일일 목표 2개 랜덤 선택, 중복 방지
// -> see docs/systems/quest-architecture.md 섹션 3.1
// -> see docs/systems/quest-system.md 섹션 5.1 for 선택 규칙
using SeedMind.Quest.Data;

namespace SeedMind.Quest
{
    public class DailyQuestSelector
    {
        private QuestData[] _dailyPool;
        private string[] _previousDailyIds;
        private string[] _todayDailyIds;
        private int _lastSelectedDay;

        public DailyQuestSelector(QuestData[] dailyPool)
        {
            _dailyPool = dailyPool;
            _previousDailyIds = System.Array.Empty<string>();
            _todayDailyIds = System.Array.Empty<string>();
        }

        public QuestData[] SelectDailyQuests(int currentDay, int playerLevel,
            bool hasProcessor)
        {
            // 선택 규칙: -> see docs/systems/quest-system.md 섹션 5.1
            // 1) 풀에서 조건 필터 (레벨, 가공소 보유 여부)
            // 2) 전날과 동일 목표 제외
            // 3) 2개 랜덤 선택
            return null; // 구현 시 교체
        }

        public DailyQuestSaveState GetSaveState()
        {
            return new DailyQuestSaveState
            {
                lastSelectedDay = _lastSelectedDay,
                previousDailyIds = _previousDailyIds,
                todayDailyIds = _todayDailyIds
            };
        }

        public void LoadSaveState(DailyQuestSaveState state)
        {
            if (state == null) return;
            _lastSelectedDay = state.lastSelectedDay;
            _previousDailyIds = state.previousDailyIds
                ?? System.Array.Empty<string>();
            _todayDailyIds = state.todayDailyIds
                ?? System.Array.Empty<string>();
        }
    }
}