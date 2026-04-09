// FishingManager — 낚시 시스템 통합 싱글턴 매니저
// -> see docs/systems/fishing-architecture.md 섹션 1, 11, 12
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Fishing.Data;
using SeedMind.Player;
using SeedMind.Save;
using SeedMind.Save.Data;

namespace SeedMind.Fishing
{
    public class FishingManager : Singleton<FishingManager>, ISaveable
    {
        // --- 직렬화 필드 ---
        [Header("데이터")]
        [SerializeField] private FishData[]     _fishDataRegistry;  // 전체 어종 목록 (15종)
        [SerializeField] private FishingConfig  _fishingConfig;

        [Header("씬 포인트")]
        [SerializeField] private FishingPoint[] _fishingPoints;     // Zone F 포인트 3개

        // --- 상태 ---
        private FishingState     _currentState  = FishingState.Idle;
        private FishData         _activeFish;
        private FishingPoint     _activePoint;
        private FishingMinigame  _minigame;
        private FishingStats     _stats;
        private FishingProficiency _proficiency;

        // 입질 타이머
        private float _biteTimer;
        private float _biteWindow;
        private bool  _biteTriggered;

        // 캐스팅 타이머
        private float _castTimer;
        private float _castDuration;

        // 읽기 전용 프로퍼티
        public FishingState  CurrentState    => _currentState;
        public FishData      ActiveFish      => _activeFish;
        public FishingStats  FishingStats    => _stats;
        public bool          IsPlayerFishing => _currentState != FishingState.Idle;

        // --- ISaveable ---
        public int SaveLoadOrder => 52;

        // ============================================================
        // Unity 생명주기
        // ============================================================

        protected override void Awake()
        {
            base.Awake();
            _stats      = new FishingStats();
            _minigame   = new FishingMinigame();
            _proficiency = new FishingProficiency();
        }

        private void Start()
        {
            // SaveManager 등록 (SaveLoadOrder = 52)
            // -> see docs/systems/fishing-architecture.md 섹션 12.2
            SeedMind.Save.SaveManager.Instance?.Register(this);

            Debug.Log($"[FishingManager] Initialized with {(_fishDataRegistry?.Length ?? 0)} fish types, config {(_fishingConfig != null ? "loaded" : "MISSING")}");
            Debug.Log($"[FishingManager] {(_fishingPoints?.Length ?? 0)} fishing points registered");
        }

        private void OnEnable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.RegisterOnDayChanged(55, OnDayChanged);
                TimeManager.Instance.RegisterOnSeasonChanged(55, OnSeasonChanged);
            }
        }

        private void OnDisable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.UnregisterOnDayChanged(OnDayChanged);
                TimeManager.Instance.UnregisterOnSeasonChanged(OnSeasonChanged);
            }
        }

        private void Update()
        {
            switch (_currentState)
            {
                case FishingState.Casting:
                    UpdateCasting();
                    break;
                case FishingState.Waiting:
                    UpdateWaiting();
                    break;
                case FishingState.Biting:
                    UpdateBiting();
                    break;
                case FishingState.Reeling:
                    UpdateReeling();
                    break;
            }
        }

        // ============================================================
        // 공개 API
        // ============================================================

        /// <summary>낚시 시작 시도. 성공 시 Casting 상태로 전환.</summary>
        public bool TryStartFishing(FishingPoint point)
        {
            if (_currentState != FishingState.Idle) return false;
            if (point == null || !point.CanFish()) return false;

            _activePoint = point;
            _stats.RecordCast();

            // 캐스팅 시간 결정
            _castDuration = _fishingConfig != null
                ? Random.Range(_fishingConfig.castDurationRange.x, _fishingConfig.castDurationRange.y)
                : 2f;
            _castTimer = 0f;

            SetState(FishingState.Casting);
            FishingEvents.OnFishCast?.Invoke(point);
            point.SetOccupied(true);
            return true;
        }

        /// <summary>현재 낚시 취소. 어느 상태에서든 호출 가능.</summary>
        public void CancelFishing()
        {
            if (_currentState == FishingState.Idle) return;
            _minigame.Reset();
            if (_activePoint != null) _activePoint.SetOccupied(false);
            _activePoint = null;
            _activeFish  = null;
            SetState(FishingState.Idle);
        }

        /// <summary>미니게임 입력 처리 — 플레이어가 Space/클릭 시 호출.</summary>
        public void OnMinigameInput()
        {
            if (_currentState == FishingState.Biting)
            {
                // 입질 시 반응
                if (_biteTriggered)
                    BeginReeling();
            }
            else if (_currentState == FishingState.Reeling)
            {
                _minigame.ProcessInput();
            }
        }

        // ============================================================
        // Update 내부 로직
        // ============================================================

        private void UpdateCasting()
        {
            _castTimer += Time.deltaTime;
            if (_castTimer >= _castDuration)
                SetState(FishingState.Waiting);
        }

        private void UpdateWaiting()
        {
            _biteTimer -= Time.deltaTime;
            if (_biteTimer <= 0f)
                TriggerBite();
        }

        private void UpdateBiting()
        {
            _biteWindow -= Time.deltaTime;
            if (_biteWindow <= 0f)
            {
                // 반응 창 종료 — 실패
                RecordFail();
            }
        }

        private void UpdateReeling()
        {
            var result = _minigame.Update(Time.deltaTime);
            if (result == MinigameResult.Success)
                CompleteFishing();
            else if (result == MinigameResult.Fail)
                RecordFail();
        }

        // ============================================================
        // 내부 낚시 플로우
        // ============================================================

        private void StartWaiting()
        {
            // 어종 선택
            _activeFish = SelectRandomFish();

            // 입질 대기 시간 결정
            float minDelay = _fishingConfig?.biteDelayRange.x ?? 3f;
            float maxDelay = _fishingConfig?.biteDelayRange.y ?? 15f;
            float reduction = _proficiency?.BiteDelayReduction ?? 0f;
            _biteTimer = Random.Range(minDelay, maxDelay) * (1f - reduction);
        }

        private void TriggerBite()
        {
            if (_activeFish == null) _activeFish = SelectRandomFish();
            _biteTriggered = true;
            _biteWindow = _fishingConfig?.biteWindowDuration ?? 1.0f;
            FishingEvents.OnFishBite?.Invoke(_activeFish);
            SetState(FishingState.Biting);
        }

        private void BeginReeling()
        {
            _biteTriggered = false;
            _minigame.Start(_fishingConfig, _activeFish);
            SetState(FishingState.Reeling);
        }

        private void CompleteFishing()
        {
            if (_activeFish == null) return;

            var quality = _minigame.CalculateQuality();

            // 인벤토리 추가
            var result = InventoryManager.Instance?.AddItem(
                _activeFish.dataId, 1, quality, HarvestOrigin.Fishing);

            if (result.HasValue && !result.Value.success && result.Value.addedQuantity == 0)
            {
                // 인벤토리 만석
                FishingEvents.OnInventoryFull?.Invoke(_activeFish);
            }
            else
            {
                // 이벤트 발행
                FishingEvents.OnFishCaught?.Invoke(_activeFish, quality);

                // 통계 기록
                bool isRare = _activeFish.rarity >= FishRarity.Rare;
                _stats.RecordCaught(_activeFish.fishId, isRare);

                // 숙련도 XP
                if (_proficiency != null)
                {
                    bool leveledUp = _proficiency.AddXP(_activeFish.expReward);
                    if (leveledUp)
                        FishingEvents.OnProficiencyLevelUp?.Invoke(_proficiency.Level);
                }

                // 판매 가격 (EconomyManager 연동 — economy-tasks.md 완전 구현 이후)
                int sellPrice = Mathf.RoundToInt(_activeFish.basePrice * GetQualityMultiplier(quality));
                Debug.Log($"[FishingManager] 포획: {_activeFish.displayName} ({quality}) 예상가 {sellPrice}G");
            }

            if (_activePoint != null)
            {
                _activePoint.RecordUse();
                _activePoint.SetOccupied(false);
            }
            _activePoint = null;
            SetState(FishingState.Success);
            SetState(FishingState.Idle);
        }

        private void RecordFail()
        {
            _stats.RecordFailed();
            _minigame.Reset();
            FishingEvents.OnFishingFailed?.Invoke();
            if (_activePoint != null) _activePoint.SetOccupied(false);
            _activePoint = null;
            _activeFish  = null;
            SetState(FishingState.Fail);
            SetState(FishingState.Idle);
        }

        // ============================================================
        // 어종 선택 알고리즘
        // ============================================================

        private FishData SelectRandomFish()
        {
            if (_fishDataRegistry == null || _fishDataRegistry.Length == 0) return null;

            Season  season  = TimeManager.Instance?.CurrentSeason  ?? Season.Spring;
            DayPhase phase  = TimeManager.Instance?.CurrentDayPhase ?? DayPhase.Morning;
            WeatherType weather = WeatherType.Clear; // TODO: WeatherSystem 연동

            // 현재 계절/시간에 등장 가능한 어종 필터링
            var candidates = new List<(FishData fish, float weight)>();
            var seasonFlag = SeasonToFlag(season);

            foreach (var fish in _fishDataRegistry)
            {
                if (fish == null) continue;
                if ((fish.seasonAvailability & seasonFlag) == 0) continue;

                float weight = GetTimeWeight(fish, phase);
                if (weight <= 0f) continue;

                // 날씨 보정
                var weatherFlag = WeatherTypeToFlag(weather);
                if ((fish.weatherBonus & weatherFlag) != 0)
                    weight *= 1.5f;

                // 희귀도 보정 (숙련도 보정 포함)
                weight *= GetRarityWeight(fish.rarity);

                candidates.Add((fish, weight));
            }

            if (candidates.Count == 0)
            {
                // fallback: 전체 중 랜덤
                return _fishDataRegistry[Random.Range(0, _fishDataRegistry.Length)];
            }

            // 가중치 랜덤 선택
            float total = 0f;
            foreach (var (_, w) in candidates) total += w;
            float roll = Random.Range(0f, total);
            float cum  = 0f;
            foreach (var (fish, w) in candidates)
            {
                cum += w;
                if (roll <= cum) return fish;
            }
            return candidates[candidates.Count - 1].fish;
        }

        private float GetTimeWeight(FishData fish, DayPhase phase)
        {
            if (fish.timeWeights == null || fish.timeWeights.Length < 5) return 0.2f;
            return fish.timeWeights[(int)phase];
        }

        private float GetRarityWeight(FishRarity rarity)
        {
            // 기본 희귀도 가중치 — 숙련도 보정 포함
            float boost = _proficiency?.RarityBoost ?? 0f;
            return rarity switch
            {
                FishRarity.Common    => 0.60f,
                FishRarity.Uncommon  => 0.28f + boost * 0.5f,
                FishRarity.Rare      => 0.10f + boost * 0.35f,
                FishRarity.Legendary => 0.02f + boost * 0.15f,
                _                    => 0.60f
            };
        }

        private static SeedMind.Farm.Data.SeasonFlag SeasonToFlag(Season s) => s switch
        {
            Season.Spring => SeedMind.Farm.Data.SeasonFlag.Spring,
            Season.Summer => SeedMind.Farm.Data.SeasonFlag.Summer,
            Season.Autumn => SeedMind.Farm.Data.SeasonFlag.Autumn,
            Season.Winter => SeedMind.Farm.Data.SeasonFlag.Winter,
            _             => SeedMind.Farm.Data.SeasonFlag.Spring
        };

        private static WeatherFlag WeatherTypeToFlag(WeatherType w) => w switch
        {
            WeatherType.Clear     => WeatherFlag.Clear,
            WeatherType.Cloudy    => WeatherFlag.Cloudy,
            WeatherType.Rain      => WeatherFlag.Rain,
            WeatherType.HeavyRain => WeatherFlag.HeavyRain,
            WeatherType.Storm     => WeatherFlag.Storm,
            WeatherType.Snow      => WeatherFlag.Snow,
            WeatherType.Blizzard  => WeatherFlag.Blizzard,
            _                    => WeatherFlag.Clear
        };

        private static float GetQualityMultiplier(CropQuality q) => q switch
        {
            CropQuality.Normal  => 1.0f,
            CropQuality.Silver  => 1.25f,
            CropQuality.Gold    => 1.5f,
            CropQuality.Iridium => 2.0f,
            _                   => 1.0f
        };

        // ============================================================
        // 시간 이벤트 핸들러
        // ============================================================

        private void OnDayChanged(int newDay)
        {
            if (_fishingPoints != null)
                foreach (var p in _fishingPoints)
                    p?.ResetDailyCount();
        }

        private void OnSeasonChanged(Season newSeason) { }

        private void SetState(FishingState newState)
        {
            var prev = _currentState;
            _currentState = newState;

            // Waiting 진입 시 어종 선택 및 타이머 설정
            if (newState == FishingState.Waiting && prev == FishingState.Casting)
                StartWaiting();

            Debug.Log($"[FishingManager] State: {prev} -> {newState}");
        }

        // ============================================================
        // ISaveable
        // ============================================================

        public object GetSaveData()
        {
            return new FishingSaveData
            {
                totalCasts              = _stats.totalCasts,
                totalCaught             = _stats.totalCaught,
                totalFailed             = _stats.totalFailed,
                caughtByFishId          = new System.Collections.Generic.Dictionary<string, int>(_stats.caughtByFishId),
                rareFishCaught          = _stats.rareFishCaught,
                maxStreak               = _stats.maxStreak,
                currentStreak           = _stats.currentStreak,
                fishingProficiencyXP    = _proficiency?.TotalXP    ?? 0,
                fishingProficiencyLevel = _proficiency?.Level       ?? 1
            };
        }

        public void LoadSaveData(object data)
        {
            if (data is not GameSaveData saveData) return;
            var d = saveData.fishing;
            if (d == null) return;

            _stats.totalCasts    = d.totalCasts;
            _stats.totalCaught   = d.totalCaught;
            _stats.totalFailed   = d.totalFailed;
            _stats.rareFishCaught = d.rareFishCaught;
            _stats.maxStreak     = d.maxStreak;
            _stats.currentStreak = d.currentStreak;
            if (d.caughtByFishId != null)
                foreach (var kv in d.caughtByFishId)
                    _stats.caughtByFishId[kv.Key] = kv.Value;

            _proficiency?.LoadFromSave(d.fishingProficiencyXP, d.fishingProficiencyLevel);

            Debug.Log($"[FishingManager] LoadSaveData 완료: caught={d.totalCaught}, profLv={d.fishingProficiencyLevel}");
        }
    }
}
