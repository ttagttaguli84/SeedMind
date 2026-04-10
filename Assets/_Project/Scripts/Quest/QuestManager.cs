// S-18: 퀘스트 생명주기 관리 (MonoBehaviour Singleton, ISaveable)
// -> see docs/systems/quest-architecture.md 섹션 3.2
using UnityEngine;
using System.Collections.Generic;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Level;
using SeedMind.Level.Data;
using SeedMind.Quest.Data;

namespace SeedMind.Quest
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestData[] _allQuests;
        [SerializeField] private QuestData[] _dailyQuestPool;

        private readonly Dictionary<string, QuestInstance> _activeQuests
            = new Dictionary<string, QuestInstance>();
        private readonly HashSet<string> _completedQuestIds = new HashSet<string>();
        private string _trackedQuestId;

        public int SaveLoadOrder => 85;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_allQuests == null) return;
            foreach (var data in _allQuests)
            {
                if (data == null || string.IsNullOrEmpty(data.questId)) continue;
                if (!_activeQuests.ContainsKey(data.questId))
                {
                    var inst = new QuestInstance(data);
                    inst.Status = QuestStatus.Available;
                    _activeQuests[data.questId] = inst;
                }
            }
        }

        public bool AcceptQuest(string questId)
        {
            if (!_activeQuests.TryGetValue(questId, out var inst)) return false;
            if (inst.Status != QuestStatus.Available) return false;

            inst.Status = QuestStatus.Active;
            inst.AcceptedDay = TimeManager.Instance != null ? TimeManager.Instance.CurrentDay : 0;
            Debug.Log($"[QuestManager] 퀘스트 수락: {questId}");
            return true;
        }

        public bool AbandonQuest(string questId)
        {
            if (!_activeQuests.TryGetValue(questId, out var inst)) return false;
            if (inst.Status != QuestStatus.Active) return false;

            inst.Status = QuestStatus.Available;
            Debug.Log($"[QuestManager] 퀘스트 포기: {questId}");
            return true;
        }

        public bool ClaimReward(string questId)
        {
            if (!_activeQuests.TryGetValue(questId, out var inst)) return false;
            if (inst.Status != QuestStatus.Active) return false;
            if (!inst.AreAllObjectivesComplete()) return false;

            GiveRewards(inst);

            inst.Status = QuestStatus.Rewarded;
            inst.CompletedDay = TimeManager.Instance != null ? TimeManager.Instance.CurrentDay : 0;
            _completedQuestIds.Add(questId);
            _activeQuests.Remove(questId);

            Debug.Log($"[QuestManager] 퀘스트 완료 및 보상 지급: {questId}");
            return true;
        }

        private void GiveRewards(QuestInstance inst)
        {
            if (inst.Data.rewards == null) return;
            foreach (var reward in inst.Data.rewards)
            {
                switch (reward.type)
                {
                    case RewardType.Gold:
                        EconomyManager.Instance?.AddGold(reward.amount);
                        break;
                    case RewardType.XP:
                        ProgressionManager.Instance?.AddExp(reward.amount, XPSource.QuestComplete);
                        break;
                    case RewardType.Item:
                        Player.InventoryManager.Instance?.AddItem(reward.targetId, reward.amount);
                        break;
                }
            }
        }

        public IReadOnlyList<QuestInstance> GetActiveQuests()
        {
            var list = new List<QuestInstance>();
            foreach (var inst in _activeQuests.Values)
                if (inst.Status == QuestStatus.Active)
                    list.Add(inst);
            return list;
        }

        public IReadOnlyList<QuestInstance> GetQuestsByCategory(QuestCategory cat)
        {
            var list = new List<QuestInstance>();
            foreach (var inst in _activeQuests.Values)
                if (inst.Data.category == cat)
                    list.Add(inst);
            return list;
        }

        public bool IsQuestCompleted(string questId)
            => _completedQuestIds.Contains(questId);

        public QuestInstance GetTrackedQuest()
        {
            if (_trackedQuestId == null) return null;
            _activeQuests.TryGetValue(_trackedQuestId, out var inst);
            return inst;
        }

        public void SetTrackedQuest(string questId)
        {
            _trackedQuestId = questId;
        }

        public object GetSaveData() => new QuestSaveData();
        public void LoadSaveData(object rawData) { }
    }
}