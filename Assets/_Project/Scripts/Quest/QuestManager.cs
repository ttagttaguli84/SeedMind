// S-18: 퀘스트 생명주기 관리 (MonoBehaviour Singleton, ISaveable)
// -> see docs/systems/quest-architecture.md 섹션 3.2
using UnityEngine;
using System.Collections.Generic;
using SeedMind.Quest.Data;

namespace SeedMind.Quest
{
    public class QuestManager : MonoBehaviour //, ISaveable
    {
        [SerializeField] private QuestData[] _allQuests;
        [SerializeField] private QuestData[] _dailyQuestPool;

        private QuestTracker _tracker;
        private QuestRewarder _rewarder;
        private DailyQuestSelector _dailySelector;
        private NPCRequestScheduler _npcScheduler;

        private Dictionary<string, QuestInstance> _activeQuests
            = new Dictionary<string, QuestInstance>();
        private HashSet<string> _completedQuestIds = new HashSet<string>();

        // ISaveable
        public int SaveLoadOrder => 85; // -> see docs/systems/quest-architecture.md 섹션 8.1

        public void Initialize()
        {
            // -> see docs/systems/quest-architecture.md 섹션 3.2
            // 1) QuestInstance 생성 2) 해금 판정 3) 자동 Active 전환
            // 4) _tracker 초기화 5) _dailySelector 첫 날 선택
        }

        public bool AcceptQuest(string questId) { return false; }
        public bool AbandonQuest(string questId) { return false; }
        public bool ClaimReward(string questId) { return false; }
        public IReadOnlyList<QuestInstance> GetActiveQuests()
            => new List<QuestInstance>();
        public IReadOnlyList<QuestInstance> GetQuestsByCategory(
            QuestCategory cat) => new List<QuestInstance>();
        public bool IsQuestCompleted(string questId)
            => _completedQuestIds.Contains(questId);
        public QuestInstance GetTrackedQuest() => null;
        public void SetTrackedQuest(string questId) { }

        public object GetSaveData()
        {
            // -> see docs/systems/quest-architecture.md 섹션 8.3
            return new QuestSaveData();
        }
        public void LoadSaveData(object rawData)
        {
            // -> see docs/systems/quest-architecture.md 섹션 8.3
        }

        // [구독] TimeManager.OnDayChanged -> CheckDailyReset, CheckTimeLimits
        // [구독] TimeManager.OnSeasonChanged -> UnlockSeasonalQuests
        // [구독] ProgressionEvents.OnLevelUp -> CheckLevelUnlocks
        // [구독] TutorialEvents.OnTutorialCompleted -> ActivateQuestSystem
        // [구독] NPCEvents.OnRequestAccepted -> AcceptNPCQuest
    }
}