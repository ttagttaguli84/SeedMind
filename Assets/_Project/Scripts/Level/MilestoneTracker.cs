// MilestoneTracker — 마일스톤 진행 상황 추적
// -> see docs/systems/progression-architecture.md 섹션 4.3
using System.Collections.Generic;
using SeedMind.Level.Data;

namespace SeedMind.Level
{
    public class MilestoneTracker
    {
        private Dictionary<string, int> _milestoneProgress;
        private HashSet<string> _completedMilestones;
        private MilestoneData[] _allMilestones;

        public void Initialize(MilestoneData[] milestones)
        {
            _allMilestones = milestones ?? new MilestoneData[0];
            _milestoneProgress = new Dictionary<string, int>();
            _completedMilestones = new HashSet<string>();
        }

        public void UpdateProgress(MilestoneConditionType type, string param, int value)
        {
            if (_allMilestones == null) return;
            foreach (var ms in _allMilestones)
            {
                if (_completedMilestones.Contains(ms.milestoneId)) continue;
                if (ms.conditionType != type) continue;
                if (!string.IsNullOrEmpty(ms.conditionParam) && ms.conditionParam != param) continue;

                if (!_milestoneProgress.ContainsKey(ms.milestoneId))
                    _milestoneProgress[ms.milestoneId] = 0;
                _milestoneProgress[ms.milestoneId] += value;
            }
        }

        public void SetProgress(MilestoneConditionType type, string param, int currentValue)
        {
            if (_allMilestones == null) return;
            foreach (var ms in _allMilestones)
            {
                if (_completedMilestones.Contains(ms.milestoneId)) continue;
                if (ms.conditionType != type) continue;
                _milestoneProgress[ms.milestoneId] = currentValue;
            }
        }

        public MilestoneData[] CheckCompletions()
        {
            var completions = new List<MilestoneData>();
            if (_allMilestones == null) return completions.ToArray();

            foreach (var ms in _allMilestones)
            {
                if (_completedMilestones.Contains(ms.milestoneId)) continue;
                if (_milestoneProgress.TryGetValue(ms.milestoneId, out int progress)
                    && progress >= ms.conditionValue)
                {
                    _completedMilestones.Add(ms.milestoneId);
                    completions.Add(ms);
                }
            }
            return completions.ToArray();
        }

        public MilestoneSaveData GetSaveData()
        {
            var entries = new List<MilestoneProgressEntry>();
            foreach (var kv in _milestoneProgress)
                entries.Add(new MilestoneProgressEntry { milestoneId = kv.Key, currentValue = kv.Value });

            return new MilestoneSaveData
            {
                completedMilestoneIds = new List<string>(_completedMilestones).ToArray(),
                progressEntries = entries.ToArray()
            };
        }

        public void LoadSaveData(MilestoneSaveData data)
        {
            _completedMilestones = new HashSet<string>();
            _milestoneProgress = new Dictionary<string, int>();
            if (data == null) return;

            if (data.completedMilestoneIds != null)
                foreach (var id in data.completedMilestoneIds)
                    _completedMilestones.Add(id);

            if (data.progressEntries != null)
                foreach (var entry in data.progressEntries)
                    _milestoneProgress[entry.milestoneId] = entry.currentValue;
        }
    }

    [System.Serializable]
    public class MilestoneSaveData
    {
        public string[] completedMilestoneIds;
        public MilestoneProgressEntry[] progressEntries;
    }

    [System.Serializable]
    public class MilestoneProgressEntry
    {
        public string milestoneId;
        public int currentValue;
    }
}
