using System;
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Farm.Data;
using SeedMind.Gathering;
using SeedMind.Level;
using SeedMind.Save;

namespace SeedMind.Collection
{
    /// <summary>
    /// 채집 도감 싱글턴. 채집 아이템 발견 기록·보상·세이브/로드 담당.
    /// SaveLoadOrder = 56 (GatheringManager=54, InventoryManager=55 다음)
    /// -> see docs/systems/collection-architecture.md 섹션 5
    /// </summary>
    public class GatheringCatalogManager : MonoBehaviour, ISaveable
    {
        public static GatheringCatalogManager Instance { get; private set; }

        [Header("도감 데이터")]
        [SerializeField] private GatheringCatalogData[] _catalogDataRegistry;

        // 런타임
        private Dictionary<string, GatheringCatalogData> _catalogDataMap = new();
        private Dictionary<string, GatheringCatalogEntry> _entries = new();
        private int _discoveredCount;

        // 마일스톤 (-> see docs/systems/collection-system.md 섹션 5.3.2)
        private static readonly int[] Milestones = { 10, 20, 27 };

        // 이벤트
        public static Action<string> OnItemDiscovered;
        public static Action<string, GatheringCatalogEntry> OnCatalogUpdated;
        public static Action<int> OnMilestoneReached;
        public static Action OnCatalogCompleted;

        // ISaveable
        public int SaveLoadOrder => 56;

        // 프로퍼티
        public int DiscoveredCount => _discoveredCount;
        public int TotalItemCount => _catalogDataRegistry != null ? _catalogDataRegistry.Length : 0;
        public float CompletionRate => TotalItemCount > 0 ? (float)_discoveredCount / TotalItemCount : 0f;
        public bool IsComplete => _discoveredCount == TotalItemCount && TotalItemCount > 0;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            Initialize();
        }

        private void OnEnable()
        {
            GatheringEvents.OnItemGathered += HandleItemGathered;
        }

        private void OnDisable()
        {
            GatheringEvents.OnItemGathered -= HandleItemGathered;
        }

        private void Initialize()
        {
            _catalogDataMap.Clear();
            _entries.Clear();

            if (_catalogDataRegistry == null) return;

            foreach (var data in _catalogDataRegistry)
            {
                if (data == null || string.IsNullOrEmpty(data.itemId)) continue;
                _catalogDataMap[data.itemId] = data;
            }

            Debug.Log($"[GatheringCatalogManager] Initialized: {_catalogDataMap.Count} items");
        }

        private void HandleItemGathered(GatheringItemData item, CropQuality quality, int quantity)
        {
            if (item == null) return;
            RegisterGather(item.dataId, quality, quantity);
        }

        public GatheringCatalogEntry RegisterGather(string itemId, CropQuality quality, int quantity)
        {
            if (string.IsNullOrEmpty(itemId)) return null;

            bool isFirstDiscovery = !_entries.ContainsKey(itemId) || !_entries[itemId].isDiscovered;

            if (!_entries.TryGetValue(itemId, out var entry))
            {
                entry = new GatheringCatalogEntry(itemId);
                _entries[itemId] = entry;
            }

            if (isFirstDiscovery && !entry.isDiscovered)
            {
                entry.isDiscovered = true;
                entry.firstGatheredDay = TimeManager.Instance != null ? TimeManager.Instance.CurrentDay : -1;
                entry.firstGatheredSeason = TimeManager.Instance != null ? (int)TimeManager.Instance.CurrentSeason : -1;
                entry.firstGatheredYear = TimeManager.Instance != null ? TimeManager.Instance.CurrentYear : -1;

                // 첫 발견 보상
                if (_catalogDataMap.TryGetValue(itemId, out var catalogData))
                {
                    if (EconomyManager.Instance != null)
                        EconomyManager.Instance.AddGold(catalogData.firstDiscoverGold);

                    if (ProgressionManager.Instance != null)
                        ProgressionManager.Instance.AddExp(catalogData.firstDiscoverXP, XPSource.GatheringCatalog);
                }

                _discoveredCount++;
                Debug.Log($"[GatheringCatalogManager] Discovered: {itemId} ({_discoveredCount}/{TotalItemCount})");
                OnItemDiscovered?.Invoke(itemId);
                CheckMilestone(_discoveredCount);
            }

            // 수량/품질 갱신
            entry.totalGathered += quantity;
            int qualityInt = (int)quality;
            if (qualityInt > entry.bestQuality)
            {
                entry.bestQuality = qualityInt;
                entry.isNewBestQuality = true;
            }
            else
            {
                entry.isNewBestQuality = false;
            }

            OnCatalogUpdated?.Invoke(itemId, entry);
            return entry;
        }

        private void CheckMilestone(int count)
        {
            foreach (int milestone in Milestones)
            {
                if (count == milestone)
                {
                    OnMilestoneReached?.Invoke(count);
                    Debug.Log($"[GatheringCatalogManager] Milestone reached: {count}");
                    if (count == TotalItemCount)
                        OnCatalogCompleted?.Invoke();
                    break;
                }
            }
        }

        public GatheringCatalogEntry GetEntry(string itemId)
        {
            return _entries.TryGetValue(itemId, out var entry) ? entry : null;
        }

        public GatheringCatalogData GetCatalogData(string itemId)
        {
            return _catalogDataMap.TryGetValue(itemId, out var data) ? data : null;
        }

        public IReadOnlyDictionary<string, GatheringCatalogEntry> GetAllEntries()
        {
            return _entries;
        }

        public bool IsDiscovered(string itemId)
        {
            return _entries.TryGetValue(itemId, out var e) && e.isDiscovered;
        }

        // ISaveable
        public object GetSaveData()
        {
            var saveData = new GatheringCatalogSaveData
            {
                entries = new System.Collections.Generic.List<GatheringCatalogEntry>(_entries.Values),
                discoveredCount = _discoveredCount
            };
            return saveData;
        }

        public void LoadSaveData(object data)
        {
            if (data is not GatheringCatalogSaveData save || save == null)
            {
                MigrateFromGatheringStats();
                return;
            }

            _entries.Clear();
            _discoveredCount = 0;

            if (save.entries != null)
            {
                foreach (var entry in save.entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.itemId)) continue;
                    _entries[entry.itemId] = entry;
                    if (entry.isDiscovered) _discoveredCount++;
                }
            }

            Debug.Log($"[GatheringCatalogManager] Loaded: {_discoveredCount} discovered");
        }

        private void MigrateFromGatheringStats()
        {
            // 구버전 세이브 호환: 도감 데이터 없음 → 빈 상태로 시작
            Debug.Log("[GatheringCatalogManager] No catalog save data found. Starting fresh.");
        }
    }
}
