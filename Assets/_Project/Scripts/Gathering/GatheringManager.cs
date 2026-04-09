// GatheringManager — 채집 시스템 통합 싱글턴 매니저
// -> see docs/systems/gathering-architecture.md 섹션 1, 5, 6, 7
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Level;
using SeedMind.Player;
using SeedMind.Save;
using SeedMind.Save.Data;

namespace SeedMind.Gathering
{
    public class GatheringManager : Singleton<GatheringManager>, ISaveable
    {
        // ── 직렬화 필드 ─────────────────────────────────────────────
        [Header("설정")]
        [SerializeField] private GatheringConfig _gatheringConfig;

        [Header("씬 포인트")]
        [SerializeField] private GatheringPoint[] _gatheringPoints;

        // ── 상태 ────────────────────────────────────────────────────
        private Dictionary<string, GatheringPointState> _pointStates =
            new Dictionary<string, GatheringPointState>();
        private GatheringStats     _stats;
        private GatheringProficiency _proficiency;

        // ── 읽기 전용 프로퍼티 ───────────────────────────────────────
        public GatheringStats GatheringStats    => _stats;
        public GatheringProficiency Proficiency => _proficiency;
        public int ProficiencyLevel             => _proficiency?.CurrentLevel ?? 1;

        // ── ISaveable ───────────────────────────────────────────────
        public int SaveLoadOrder => 54;

        // ============================================================
        // Unity 생명주기
        // ============================================================

        protected override void Awake()
        {
            base.Awake();
            _stats       = new GatheringStats();
            _proficiency = new GatheringProficiency(_gatheringConfig);
            _proficiency.OnLevelUp += level =>
            {
                GatheringEvents.OnProficiencyLevelUp?.Invoke(level);
                Debug.Log($"[GatheringManager] 채집 숙련도 레벨업! Lv.{level}");
            };
        }

        private void Start()
        {
            SaveManager.Instance?.Register(this);
            Initialize();
            Debug.Log($"[GatheringManager] Initialized. Points: {_gatheringPoints?.Length ?? 0}, Config: {(_gatheringConfig != null ? "OK" : "MISSING")}");
        }

        private void OnEnable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.RegisterOnDayChanged(56, ProcessDayChange);
                TimeManager.Instance.RegisterOnSeasonChanged(56, ProcessSeasonChange);
            }
        }

        private void OnDisable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.UnregisterOnDayChanged(ProcessDayChange);
                TimeManager.Instance.UnregisterOnSeasonChanged(ProcessSeasonChange);
            }
        }

        // ============================================================
        // 초기화
        // ============================================================

        public void Initialize()
        {
            _pointStates.Clear();
            if (_gatheringPoints == null) return;

            foreach (var point in _gatheringPoints)
            {
                if (point == null || string.IsNullOrEmpty(point.PointId)) continue;
                _pointStates[point.PointId] = new GatheringPointState
                {
                    pointId  = point.PointId,
                    isActive = true,
                    respawnDaysRemaining = 0
                };
                point.SetActive(true);
            }
        }

        // ============================================================
        // 채집 인터랙션
        // ============================================================

        /// <summary>채집 시도. 성공 시 아이템 획득 + 포인트 비활성화.</summary>
        public GatherResult TryGather(GatheringPoint point)
        {
            if (point == null) return GatherResult.Fail;

            // 1) 포인트 활성 확인
            if (!IsPointActive(point.PointId))
            {
                Debug.Log($"[GatheringManager] 포인트 {point.PointId} 비활성 — 채집 불가");
                return GatherResult.Fail;
            }

            // 2) 아이템 선택
            var item = SelectGatheringItem(point);
            if (item == null) return GatherResult.Fail;

            // 3) 에너지 확인 (TODO: PlayerController Singleton/Energy API 구현 후 연동)
            // int energyCost = Mathf.Max(0,
            //     (_gatheringConfig?.baseGatherEnergy ?? 5) - _proficiency.GetEnergyCostReduction());
            // PlayerController.Instance?.ConsumeEnergy(energyCost);

            // 5) 수량 결정
            int quantity = CalculateQuantity(item);

            // 6) 품질 결정
            var quality = DetermineQuality(item);

            // 7) 인벤토리 추가
            var inv = InventoryManager.Instance;
            bool bonusTriggered = false;
            if (inv != null)
            {
                var result = inv.AddItem(item.dataId, quantity, quality, HarvestOrigin.Gathering);
                if (!result.success && result.addedQuantity == 0)
                {
                    GatheringEvents.OnInventoryFull?.Invoke(item);
                    Debug.Log("[GatheringManager] 배낭 가득 — 채집 실패");
                    return GatherResult.Fail;
                }
                // 보너스 수량 체크
                if (Random.value < _proficiency.GetBonusQuantityChance())
                {
                    inv.AddItem(item.dataId, 1, quality, HarvestOrigin.Gathering);
                    bonusTriggered = true;
                }
            }

            // 8) 진행 XP 부여
            ProgressionManager.Instance?.AddExp(item.expReward, XPSource.GatheringComplete);

            // 9) 채집 숙련도 XP
            _proficiency.AddXP(_proficiency.GetXPForGather(item.rarity));

            // 10) 통계 갱신
            _stats.totalGathered++;
            if (!_stats.gatheredByItemId.ContainsKey(item.dataId)) _stats.gatheredByItemId[item.dataId] = 0;
            _stats.gatheredByItemId[item.dataId] += quantity;
            string zoneId = point.PointData?.zoneId ?? "unknown";
            if (!_stats.gatheredByZone.ContainsKey(zoneId)) _stats.gatheredByZone[zoneId] = 0;
            _stats.gatheredByZone[zoneId]++;
            if (item.rarity >= GatheringRarity.Rare) _stats.rareItemsFound++;

            // 11) 포인트 비활성화 + 재생성 타이머
            if (_pointStates.TryGetValue(point.PointId, out var state))
            {
                state.isActive = false;
                int variance = point.PointData?.respawnVariance ?? 1;
                state.respawnDaysRemaining = (point.PointData?.respawnDays ?? 3)
                    + Random.Range(-variance, variance + 1);
                state.lastGatheredDay    = TimeManager.Instance?.CurrentDay ?? 0;
                state.lastGatheredSeason = TimeManager.Instance?.CurrentSeason ?? Season.Spring;
            }
            point.SetActive(false);
            GatheringEvents.OnPointDepleted?.Invoke(point);

            // 12) 이벤트 발행
            GatheringEvents.OnItemGathered?.Invoke(item, quality, quantity);
            point.PlayGatherEffect();

            return new GatherResult
            {
                success        = true,
                item           = item,
                quality        = quality,
                quantity       = quantity,
                bonusTriggered = bonusTriggered
            };
        }

        public bool IsPointActive(string pointId)
        {
            return _pointStates.TryGetValue(pointId, out var s) && s.isActive;
        }

        public int GetActivePointCount()
        {
            int count = 0;
            foreach (var s in _pointStates.Values) if (s.isActive) count++;
            return count;
        }

        // ============================================================
        // 내부 헬퍼
        // ============================================================

        private GatheringItemData SelectGatheringItem(GatheringPoint point)
        {
            if (point.PointData == null) return null;

            // 계절별 오버라이드 확인
            GatheringItemEntry[] pool = point.PointData.availableItems;
            Season curSeason = TimeManager.Instance?.CurrentSeason ?? Season.Spring;

            if (point.PointData.seasonOverrides != null)
            {
                foreach (var so in point.PointData.seasonOverrides)
                {
                    if (so.season == curSeason && so.overrideItems != null && so.overrideItems.Length > 0)
                    {
                        pool = so.overrideItems;
                        break;
                    }
                }
            }

            if (pool == null || pool.Length == 0) return null;

            // 가중치 랜덤 선택
            float totalWeight = 0f;
            foreach (var e in pool)
            {
                float w = e.weight;
                if (e.item != null && e.item.rarity >= GatheringRarity.Rare)
                    w *= (1f + _proficiency.GetRarityBonus());
                totalWeight += w;
            }

            float rand = Random.Range(0f, totalWeight);
            float accumulated = 0f;
            foreach (var e in pool)
            {
                if (e.item == null) continue;
                float w = e.weight;
                if (e.item.rarity >= GatheringRarity.Rare)
                    w *= (1f + _proficiency.GetRarityBonus());
                accumulated += w;
                if (rand <= accumulated) return e.item;
            }
            return pool.Length > 0 ? pool[pool.Length - 1].item : null;
        }

        private int CalculateQuantity(GatheringItemData item)
        {
            int min = item.baseQuantityRange.x;
            int max = item.baseQuantityRange.y;
            return Random.Range(min, max + 1);
        }

        private CropQuality DetermineQuality(GatheringItemData item)
        {
            if (!item.qualityEnabled) return CropQuality.Normal;

            float roll = Random.value;
            var thresholds = _gatheringConfig?.qualityThresholds;
            CropQuality maxQ = _proficiency.GetMaxGatherQuality();

            if (thresholds != null && thresholds.Length >= 4)
            {
                if (roll >= thresholds[3] && maxQ >= CropQuality.Iridium) return CropQuality.Iridium;
                if (roll >= thresholds[2] && maxQ >= CropQuality.Gold)    return CropQuality.Gold;
                if (roll >= thresholds[1] && maxQ >= CropQuality.Silver)  return CropQuality.Silver;
            }
            return CropQuality.Normal;
        }

        private GatheringPoint FindPointById(string pointId)
        {
            if (_gatheringPoints == null) return null;
            foreach (var p in _gatheringPoints)
                if (p != null && p.PointId == pointId) return p;
            return null;
        }

        // ============================================================
        // 시간 이벤트 핸들러
        // ============================================================

        private void ProcessDayChange(int newDay)
        {
            foreach (var state in _pointStates.Values)
            {
                if (state.isActive) continue;
                state.respawnDaysRemaining--;
                if (state.respawnDaysRemaining <= 0)
                {
                    state.isActive = true;
                    state.respawnDaysRemaining = 0;
                    var p = FindPointById(state.pointId);
                    if (p != null)
                    {
                        p.SetActive(true);
                        GatheringEvents.OnPointRespawned?.Invoke(p);
                    }
                }
            }
        }

        private void ProcessSeasonChange(Season newSeason)
        {
            if (_gatheringConfig == null || !_gatheringConfig.seasonalRefreshOnChange) return;

            foreach (var state in _pointStates.Values)
            {
                state.isActive = true;
                state.respawnDaysRemaining = 0;
                var p = FindPointById(state.pointId);
                if (p != null) p.SetActive(true);
            }
            Debug.Log($"[GatheringManager] 계절 전환({newSeason}) — 모든 채집 포인트 리프레시");
        }

        // ============================================================
        // ISaveable 구현
        // ============================================================

        public object GetSaveData()
        {
            var save = new GatheringSaveData
            {
                totalGathered              = _stats.totalGathered,
                gatheredByItemId           = new Dictionary<string, int>(_stats.gatheredByItemId),
                gatheredByZone             = new Dictionary<string, int>(_stats.gatheredByZone),
                rareItemsFound             = _stats.rareItemsFound,
                gatheringProficiencyXP     = _proficiency.CurrentXP,
                gatheringProficiencyLevel  = _proficiency.CurrentLevel
            };

            save.pointStates = new List<GatheringPointStateSaveData>();
            foreach (var kv in _pointStates)
            {
                save.pointStates.Add(new GatheringPointStateSaveData
                {
                    pointId               = kv.Key,
                    isActive              = kv.Value.isActive,
                    respawnDaysRemaining  = kv.Value.respawnDaysRemaining,
                    lastGatheredDay       = kv.Value.lastGatheredDay,
                    lastGatheredSeason    = (int)kv.Value.lastGatheredSeason
                });
            }
            return save;
        }

        public void LoadSaveData(object data)
        {
            if (data is not GatheringSaveData save) return;

            _stats.totalGathered    = save.totalGathered;
            _stats.rareItemsFound   = save.rareItemsFound;
            _stats.gatheredByItemId = save.gatheredByItemId ?? new Dictionary<string, int>();
            _stats.gatheredByZone   = save.gatheredByZone   ?? new Dictionary<string, int>();

            _proficiency.LoadSaveData(save.gatheringProficiencyXP, save.gatheringProficiencyLevel);

            if (save.pointStates != null)
            {
                foreach (var ps in save.pointStates)
                {
                    _pointStates[ps.pointId] = new GatheringPointState
                    {
                        pointId              = ps.pointId,
                        isActive             = ps.isActive,
                        respawnDaysRemaining = ps.respawnDaysRemaining,
                        lastGatheredDay      = ps.lastGatheredDay,
                        lastGatheredSeason   = (Season)ps.lastGatheredSeason
                    };
                    var p = FindPointById(ps.pointId);
                    if (p != null) p.SetActive(ps.isActive);
                }
            }
            Debug.Log("[GatheringManager] 세이브 데이터 복원 완료");
        }
    }
}
