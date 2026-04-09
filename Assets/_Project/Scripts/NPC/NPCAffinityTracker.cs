// S-04: NPC 친밀도 추적 매니저 (범용, 대장간 전용 아님)
// -> see docs/systems/blacksmith-architecture.md 섹션 1, 5
using System;
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Core;

namespace SeedMind.NPC
{
    /// <summary>
    /// 전체 NPC의 친밀도를 추적, 관리한다.
    /// 세이브/로드 시 AffinitySaveData를 사용한다.
    /// </summary>
    public class NPCAffinityTracker : MonoBehaviour
    {
        private Dictionary<string, int> _affinityMap
            = new Dictionary<string, int>();
        private Dictionary<string, int> _lastVisitDayMap
            = new Dictionary<string, int>();
        private Dictionary<string, HashSet<string>> _triggeredDialogueMap
            = new Dictionary<string, HashSet<string>>();

        // --- 이벤트 ---
        public event Action<string, int, int> OnAffinityChanged;
            // npcId, oldValue, newValue
        public event Action<string, int> OnAffinityLevelUp;
            // npcId, newLevel

        // --- 공개 메서드 ---

        public int GetAffinity(string npcId)
            => _affinityMap.TryGetValue(npcId, out var v) ? v : 0;

        public void AddAffinity(string npcId, int amount, int[] thresholds = null)
        {
            int oldVal = GetAffinity(npcId);
            int newVal = oldVal + amount;
            _affinityMap[npcId] = newVal;
            OnAffinityChanged?.Invoke(npcId, oldVal, newVal);

            if (thresholds != null)
            {
                int oldLevel = CalcAffinityLevel(thresholds, oldVal);
                int newLevel = CalcAffinityLevel(thresholds, newVal);
                if (newLevel > oldLevel)
                    OnAffinityLevelUp?.Invoke(npcId, newLevel);
            }
        }

        public int GetAffinityLevel(string npcId, int[] thresholds)
            => CalcAffinityLevel(thresholds, GetAffinity(npcId));

        private int CalcAffinityLevel(int[] thresholds, int value)
        {
            int level = 0;
            for (int i = 0; i < thresholds.Length; i++)
                if (value >= thresholds[i]) level = i;
            return level;
        }

        public bool HasTriggeredDialogue(string npcId, string dialogueId)
        {
            if (!_triggeredDialogueMap.TryGetValue(npcId, out var set)) return false;
            return set.Contains(dialogueId);
        }

        public void MarkDialogueTriggered(string npcId, string dialogueId)
        {
            if (!_triggeredDialogueMap.ContainsKey(npcId))
                _triggeredDialogueMap[npcId] = new HashSet<string>();
            _triggeredDialogueMap[npcId].Add(dialogueId);
        }

        public bool CanGiveDailyAffinity(string npcId)
        {
            if (!_lastVisitDayMap.TryGetValue(npcId, out var lastDay)) return true;
            var tm = FindObjectOfType<TimeManager>();
            if (tm == null) return true;
            return tm.CurrentDay > lastDay;
        }

        public void MarkDailyVisit(string npcId)
        {
            var tm = FindObjectOfType<TimeManager>();
            int currentDay = tm != null ? tm.CurrentDay : 0;
            _lastVisitDayMap[npcId] = currentDay;
        }

        public AffinitySaveData GetSaveData()
        {
            var entries = new AffinityEntry[_affinityMap.Count];
            int i = 0;
            foreach (var kvp in _affinityMap)
            {
                _triggeredDialogueMap.TryGetValue(kvp.Key, out var dialogues);
                _lastVisitDayMap.TryGetValue(kvp.Key, out var lastDay);
                entries[i++] = new AffinityEntry
                {
                    npcId = kvp.Key,
                    affinityValue = kvp.Value,
                    lastVisitDay = lastDay,
                    triggeredDialogueIds = dialogues != null
                        ? new List<string>(dialogues).ToArray()
                        : new string[0]
                };
            }
            return new AffinitySaveData { entries = entries };
        }

        public void LoadSaveData(AffinitySaveData data)
        {
            _affinityMap.Clear();
            _lastVisitDayMap.Clear();
            _triggeredDialogueMap.Clear();
            if (data?.entries == null) return;
            foreach (var entry in data.entries)
            {
                _affinityMap[entry.npcId] = entry.affinityValue;
                _lastVisitDayMap[entry.npcId] = entry.lastVisitDay;
                if (entry.triggeredDialogueIds != null)
                    _triggeredDialogueMap[entry.npcId] =
                        new HashSet<string>(entry.triggeredDialogueIds);
            }
        }
    }
}
