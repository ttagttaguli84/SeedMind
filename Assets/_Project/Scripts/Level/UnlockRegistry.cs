// UnlockRegistry — 런타임 해금 상태 관리
// -> see docs/systems/progression-architecture.md 섹션 3.3
using System;
using System.Collections.Generic;
using SeedMind.Level.Data;

namespace SeedMind.Level
{
    public class UnlockRegistry
    {
        private Dictionary<UnlockType, HashSet<string>> _unlockedItems;

        public void Initialize(LevelUnlockEntry[] unlockTable, int currentLevel)
        {
            _unlockedItems = new Dictionary<UnlockType, HashSet<string>>();
            foreach (UnlockType type in Enum.GetValues(typeof(UnlockType)))
                _unlockedItems[type] = new HashSet<string>();

            for (int lvl = 1; lvl <= currentLevel; lvl++)
            {
                var entry = FindEntry(unlockTable, lvl);
                if (entry == null) continue;
                foreach (var item in entry.items)
                    _unlockedItems[item.type].Add(item.itemId);
            }
        }

        public bool IsUnlocked(UnlockType type, string itemId)
            => _unlockedItems != null
               && _unlockedItems.ContainsKey(type)
               && _unlockedItems[type].Contains(itemId);

        public string[] GetUnlockedItems(UnlockType type)
            => _unlockedItems != null && _unlockedItems.ContainsKey(type)
               ? new List<string>(_unlockedItems[type]).ToArray()
               : Array.Empty<string>();

        public UnlockInfo[] ApplyLevelUnlocks(LevelUnlockEntry[] unlockTable, int newLevel)
        {
            var entry = FindEntry(unlockTable, newLevel);
            if (entry == null) return Array.Empty<UnlockInfo>();

            var newUnlocks = new List<UnlockInfo>();
            foreach (var item in entry.items)
            {
                if (_unlockedItems[item.type].Add(item.itemId))
                {
                    newUnlocks.Add(new UnlockInfo
                    {
                        type = item.type,
                        itemId = item.itemId,
                        displayName = item.displayName
                    });
                }
            }
            return newUnlocks.ToArray();
        }

        public UnlockSaveData GetSaveData()
        {
            return new UnlockSaveData
            {
                unlockedCrops      = GetUnlockedItems(UnlockType.Crop),
                unlockedBuildings  = GetUnlockedItems(UnlockType.Facility),
                unlockedRecipes    = GetUnlockedItems(UnlockType.Recipe),
                unlockedFertilizers = GetUnlockedItems(UnlockType.Fertilizer),
                unlockedTools      = GetUnlockedItems(UnlockType.Tool),
                unlockedFarmExpansions = GetUnlockedItems(UnlockType.FarmExpansion),
            };
        }

        public void LoadSaveData(UnlockSaveData data)
        {
            _unlockedItems = new Dictionary<UnlockType, HashSet<string>>();
            foreach (UnlockType type in Enum.GetValues(typeof(UnlockType)))
                _unlockedItems[type] = new HashSet<string>();

            if (data == null) return;
            AddRange(UnlockType.Crop, data.unlockedCrops);
            AddRange(UnlockType.Facility, data.unlockedBuildings);
            AddRange(UnlockType.Recipe, data.unlockedRecipes);
            AddRange(UnlockType.Fertilizer, data.unlockedFertilizers);
            AddRange(UnlockType.Tool, data.unlockedTools);
            AddRange(UnlockType.FarmExpansion, data.unlockedFarmExpansions);
        }

        private void AddRange(UnlockType type, string[] ids)
        {
            if (ids == null) return;
            foreach (var id in ids)
                _unlockedItems[type].Add(id);
        }

        private LevelUnlockEntry FindEntry(LevelUnlockEntry[] table, int level)
        {
            if (table == null) return null;
            foreach (var entry in table)
                if (entry.level == level) return entry;
            return null;
        }
    }

    // 세이브 데이터 구조 (-> see docs/pipeline/data-pipeline.md 섹션 2.6)
    [System.Serializable]
    public class UnlockSaveData
    {
        public string[] unlockedCrops;
        public string[] unlockedBuildings;
        public string[] unlockedRecipes;
        public string[] unlockedFertilizers;
        public string[] unlockedTools;
        public string[] unlockedFarmExpansions;
    }

    // 이벤트 정보 구조체
    public struct UnlockInfo
    {
        public UnlockType type;
        public string itemId;
        public string displayName;
    }
}
