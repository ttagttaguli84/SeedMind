// AnimalManager — 목축/낙농 시스템 통합 매니저 (Singleton, ISaveable)
// -> see docs/systems/livestock-architecture.md 섹션 1, 4, 8
using System;
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Farm;
using SeedMind.Level;
using SeedMind.Player;
using SeedMind.Save;
using SeedMind.Building;
using SeedMind.Livestock.Data;

namespace SeedMind.Livestock
{
    public class AnimalManager : Singleton<AnimalManager>, ISaveable
    {
        // --- Inspector 필드 ---
        [SerializeField] private AnimalData[] _animalDataRegistry;
        [SerializeField] private LivestockConfig _livestockConfig;

        // --- 런타임 상태 ---
        private readonly List<AnimalInstance> _animals = new List<AnimalInstance>();
        private bool _isUnlocked;
        private int _barnLevel;
        private int _barnCapacity;
        private int _coopLevel;
        private int _coopCapacity;

        // --- ISaveable ---
        public int SaveLoadOrder => 48;

        // --- 읽기 전용 ---
        public bool IsUnlocked => _isUnlocked;
        public IReadOnlyList<AnimalInstance> Animals => _animals;
        public int BarnLevel => _barnLevel;
        public int CoopLevel => _coopLevel;

        // --- 이벤트 ---
        public event Action<AnimalInstance> OnAnimalAdded;
        public event Action<AnimalInstance> OnAnimalFed;
        public event Action<AnimalInstance> OnAnimalPetted;
        public event Action<AnimalInstance, AnimalProductInfo> OnProductCollected;
        public event Action<int> OnBarnUpgraded;
        public event Action<int> OnCoopUpgraded;

        // ============================================================
        // Unity 생명주기
        // ============================================================

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnEnable()
        {
            ZoneEvents.OnZoneUnlocked += HandleZoneUnlocked;
            BuildingEvents.OnBuildingCompleted += HandleBuildingCompleted;

            if (TimeManager.Instance != null)
                TimeManager.Instance.RegisterOnDayChanged(55, DailyUpdate);
        }

        private void OnDisable()
        {
            ZoneEvents.OnZoneUnlocked -= HandleZoneUnlocked;
            BuildingEvents.OnBuildingCompleted -= HandleBuildingCompleted;

            if (TimeManager.Instance != null)
                TimeManager.Instance.UnregisterOnDayChanged(DailyUpdate);
        }

        // ============================================================
        // 공개 API — 시설 제어
        // ============================================================

        public void UnlockBarn()
        {
            if (_barnLevel > 0) return;
            _barnLevel = 1;
            _barnCapacity = _livestockConfig != null ? _livestockConfig.initialBarnCapacity : 4;
            Debug.Log($"[AnimalManager] Barn unlocked, capacity={_barnCapacity}");
            LivestockEvents.RaiseBarnUpgraded(_barnLevel);
            OnBarnUpgraded?.Invoke(_barnLevel);
        }

        public void UnlockCoop()
        {
            if (_coopLevel > 0) return;
            _coopLevel = 1;
            _coopCapacity = _livestockConfig != null ? _livestockConfig.initialCoopCapacity : 4;
            Debug.Log($"[AnimalManager] Coop unlocked, capacity={_coopCapacity}");
            LivestockEvents.RaiseCoopUpgraded(_coopLevel);
            OnCoopUpgraded?.Invoke(_coopLevel);
        }

        public bool UpgradeBarn()
        {
            if (_livestockConfig == null || _barnLevel <= 0) return false;
            int tierIdx = _barnLevel - 1;
            if (_livestockConfig.barnUpgradeCost == null || tierIdx >= _livestockConfig.barnUpgradeCost.Length)
                return false;
            int cost = _livestockConfig.barnUpgradeCost[tierIdx];
            if (!EconomyManager.Instance.TrySpendGold(cost)) return false;
            _barnLevel++;
            if (_livestockConfig.barnUpgradeCapacity != null && tierIdx < _livestockConfig.barnUpgradeCapacity.Length)
                _barnCapacity = _livestockConfig.barnUpgradeCapacity[tierIdx];
            LivestockEvents.RaiseBarnUpgraded(_barnLevel);
            OnBarnUpgraded?.Invoke(_barnLevel);
            return true;
        }

        public bool UpgradeCoop()
        {
            if (_livestockConfig == null || _coopLevel <= 0) return false;
            int tierIdx = _coopLevel - 1;
            if (_livestockConfig.coopUpgradeCost == null || tierIdx >= _livestockConfig.coopUpgradeCost.Length)
                return false;
            int cost = _livestockConfig.coopUpgradeCost[tierIdx];
            if (!EconomyManager.Instance.TrySpendGold(cost)) return false;
            _coopLevel++;
            if (_livestockConfig.coopUpgradeCapacity != null && tierIdx < _livestockConfig.coopUpgradeCapacity.Length)
                _coopCapacity = _livestockConfig.coopUpgradeCapacity[tierIdx];
            LivestockEvents.RaiseCoopUpgraded(_coopLevel);
            OnCoopUpgraded?.Invoke(_coopLevel);
            return true;
        }

        // ============================================================
        // 공개 API — 동물 관리
        // ============================================================

        public bool TryBuyAnimal(string animalDataId)
        {
            var data = FindAnimalData(animalDataId);
            if (data == null)
            {
                Debug.LogWarning($"[AnimalManager] TryBuyAnimal: 알 수 없는 animalDataId={animalDataId}");
                return false;
            }
            if (data.animalType == AnimalType.Poultry && _coopLevel <= 0)
            {
                Debug.Log("[AnimalManager] 닭장이 없습니다.");
                return false;
            }
            if ((data.animalType == AnimalType.Cattle || data.animalType == AnimalType.SmallAnimal) && _barnLevel <= 0)
            {
                Debug.Log("[AnimalManager] 외양간이 없습니다.");
                return false;
            }
            int count = CountAnimals(data.animalType);
            int cap = data.animalType == AnimalType.Poultry ? _coopCapacity : _barnCapacity;
            if (count >= cap)
            {
                Debug.Log($"[AnimalManager] 수용 한도 초과: {data.animalType} {count}/{cap}");
                return false;
            }
            if (data.purchasePrice > 0 && !EconomyManager.Instance.TrySpendGold(data.purchasePrice))
                return false;

            float initHappiness = _livestockConfig != null ? _livestockConfig.initialHappiness : 50f;
            int today = TimeManager.Instance != null ? TimeManager.Instance.TotalElapsedDays : 0;
            var instance = new AnimalInstance
            {
                instanceId = Guid.NewGuid().ToString(),
                animalDataId = animalDataId,
                data = data,
                happiness = initHappiness,
                displayName = data.animalName,
                purchaseDay = today
            };

            _animals.Add(instance);
            Debug.Log($"[AnimalManager] Animal purchased - {animalDataId}, instanceId={instance.instanceId}, happiness={instance.happiness}");
            LivestockEvents.RaiseAnimalPurchased(instance);
            OnAnimalAdded?.Invoke(instance);
            return true;
        }

        public bool FeedAnimal(AnimalInstance animal)
        {
            if (animal == null || animal.data == null) return false;
            if (animal.isFedToday)
            {
                Debug.Log($"[AnimalManager] {animal.displayName} 오늘 이미 먹이를 받았습니다.");
                return false;
            }
            var inv = InventoryManager.Instance;
            if (inv != null && !string.IsNullOrEmpty(animal.data.requiredFeedId))
            {
                int needed = animal.data.dailyFeedAmount > 0 ? animal.data.dailyFeedAmount : 1;
                if (!inv.HasItem(animal.data.requiredFeedId, needed))
                {
                    Debug.Log($"[AnimalManager] 사료 부족: {animal.data.requiredFeedId} x{needed}");
                    return false;
                }
                inv.RemoveItem(animal.data.requiredFeedId, needed);
            }
            animal.isFedToday = true;
            animal.daysSinceLastFed = 0;
            float delta = animal.data.feedHappinessGain;
            animal.happiness = HappinessCalculator.Clamp(animal.happiness + delta);
            Debug.Log($"[AnimalManager] AnimalFed: {animal.displayName}, happiness delta=+{delta:F1}");
            LivestockEvents.RaiseAnimalFed(animal);
            OnAnimalFed?.Invoke(animal);
            var pm = ProgressionManager.Instance;
            if (pm != null)
                pm.AddExp(pm.GetExpForSource(XPSource.AnimalCare), XPSource.AnimalCare);
            return true;
        }

        public bool PetAnimal(AnimalInstance animal)
        {
            if (animal == null || animal.data == null) return false;
            if (animal.isPettedToday)
            {
                Debug.Log($"[AnimalManager] {animal.displayName} 오늘 이미 쓰다듬었습니다.");
                return false;
            }
            animal.isPettedToday = true;
            animal.daysSinceLastPetted = 0;
            float delta = animal.data.petHappinessGain;
            animal.happiness = HappinessCalculator.Clamp(animal.happiness + delta);
            Debug.Log($"[AnimalManager] AnimalPetted: {animal.displayName}, happiness delta=+{delta:F1}");
            LivestockEvents.RaiseAnimalPetted(animal);
            OnAnimalPetted?.Invoke(animal);
            var pm = ProgressionManager.Instance;
            if (pm != null)
                pm.AddExp(pm.GetExpForSource(XPSource.AnimalCare), XPSource.AnimalCare);
            return true;
        }

        public CollectResult CollectProduct(AnimalInstance animal)
        {
            if (animal == null || animal.data == null) return CollectResult.NotReady;
            if (!animal.isProductReady) return CollectResult.NotReady;
            var inv = InventoryManager.Instance;
            if (inv == null) return CollectResult.InventoryFull;
            float multiplier = HappinessCalculator.GetProductionMultiplier(animal.happiness, _livestockConfig);
            int amount = Mathf.Max(1, Mathf.RoundToInt(animal.data.baseProductAmount * multiplier));
            var quality = HappinessCalculator.GetProductQuality(animal.happiness, _livestockConfig);
            var result = inv.AddItem(animal.data.productItemId, amount, quality, HarvestOrigin.Barn);
            if (!result.success && result.addedQuantity == 0)
                return CollectResult.InventoryFull;
            var info = new AnimalProductInfo
            {
                productItemId = animal.data.productItemId,
                quality = quality,
                estimatedAmount = result.addedQuantity
            };
            animal.isProductReady = false;
            animal.daysSinceLastProduct = 0;
            Debug.Log($"[AnimalManager] Product collected: {animal.displayName} → {info.productItemId} x{info.estimatedAmount} ({info.quality})");
            LivestockEvents.RaiseProductCollected(animal, info);
            OnProductCollected?.Invoke(animal, info);
            var pm = ProgressionManager.Instance;
            if (pm != null)
                pm.AddExp(pm.GetExpForSource(XPSource.AnimalHarvest), XPSource.AnimalHarvest);
            return CollectResult.Success;
        }

        public AnimalInstance GetAnimalById(string instanceId)
        {
            foreach (var a in _animals)
                if (a.instanceId == instanceId) return a;
            return null;
        }

        // ============================================================
        // 일일 업데이트 (TimeManager 콜백, priority=55)
        // ============================================================

        private void DailyUpdate(int day)
        {
            foreach (var animal in _animals)
            {
                if (animal == null || animal.data == null) continue;
                float delta = HappinessCalculator.CalculateDailyDelta(animal, _livestockConfig);
                animal.happiness = HappinessCalculator.Clamp(animal.happiness + delta);
                if (!animal.isFedToday) animal.daysSinceLastFed++;
                if (!animal.isPettedToday) animal.daysSinceLastPetted++;
                animal.isFedToday = false;
                animal.isPettedToday = false;
                float sadThreshold = _livestockConfig != null ? _livestockConfig.silverQualityThreshold * 0.5f : 20f;
                if (animal.happiness < sadThreshold)
                    LivestockEvents.RaiseAnimalSad(animal);
                if (!animal.isProductReady)
                {
                    animal.daysSinceLastProduct++;
                    if (animal.daysSinceLastFed == 0 &&
                        animal.daysSinceLastProduct >= animal.data.productionIntervalDays)
                    {
                        animal.isProductReady = true;
                        animal.productQuality = HappinessCalculator.GetProductQuality(animal.happiness, _livestockConfig);
                        LivestockEvents.RaiseProductReady(animal);
                    }
                }
            }
            Debug.Log($"[AnimalManager] DailyUpdate day={day}, animals={_animals.Count}");
        }

        // ============================================================
        // 이벤트 핸들러
        // ============================================================

        private void HandleZoneUnlocked(string zoneId, ZoneData zone)
        {
            if (zoneId != "zone_south_meadow") return;
            _isUnlocked = true;
            Debug.Log("[AnimalManager] Zone E unlocked, livestock system activated");
        }

        private void HandleBuildingCompleted(SeedMind.Building.BuildingInstance inst)
        {
            if (inst?.Data == null) return;
            string id = inst.Data.dataId;
            if (id == "building_barn" || id == "building_barn_2")
            {
                if (_barnLevel <= 0) UnlockBarn();
                else { _barnLevel++; Debug.Log($"[AnimalManager] Barn built, barnLevel={_barnLevel}"); }
            }
            else if (id == "building_chicken_coop" || id == "building_chicken_coop_2")
            {
                if (_coopLevel <= 0) UnlockCoop();
                else { _coopLevel++; Debug.Log($"[AnimalManager] Coop built, coopLevel={_coopLevel}"); }
            }
        }

        // ============================================================
        // ISaveable
        // ============================================================

        public object GetSaveData()
        {
            var saveAnimals = new AnimalInstanceSaveData[_animals.Count];
            for (int i = 0; i < _animals.Count; i++)
            {
                var a = _animals[i];
                saveAnimals[i] = new AnimalInstanceSaveData
                {
                    instanceId = a.instanceId,
                    animalDataId = a.animalDataId,
                    happiness = a.happiness,
                    daysSinceLastFed = a.daysSinceLastFed,
                    daysSinceLastPetted = a.daysSinceLastPetted,
                    isFedToday = a.isFedToday,
                    isPettedToday = a.isPettedToday,
                    daysSinceLastProduct = a.daysSinceLastProduct,
                    isProductReady = a.isProductReady,
                    productQuality = a.productQuality,
                    purchaseDay = a.purchaseDay,
                    displayName = a.displayName
                };
            }
            return new AnimalSaveData
            {
                isUnlocked = _isUnlocked,
                barnLevel = _barnLevel,
                coopLevel = _coopLevel,
                animals = saveAnimals
            };
        }

        public void LoadSaveData(object data)
        {
            _animals.Clear();
            var save = data as AnimalSaveData;
            if (save == null) { Debug.Log("[AnimalManager] LoadSaveData: null → 초기 상태"); return; }
            _isUnlocked = save.isUnlocked;
            _barnLevel = save.barnLevel;
            _coopLevel = save.coopLevel;
            if (_livestockConfig != null)
            {
                _barnCapacity = _barnLevel > 0 ? _livestockConfig.initialBarnCapacity : 0;
                _coopCapacity = _coopLevel > 0 ? _livestockConfig.initialCoopCapacity : 0;
            }
            if (save.animals != null)
            {
                foreach (var s in save.animals)
                {
                    _animals.Add(new AnimalInstance
                    {
                        instanceId = s.instanceId,
                        animalDataId = s.animalDataId,
                        data = FindAnimalData(s.animalDataId),
                        happiness = s.happiness,
                        daysSinceLastFed = s.daysSinceLastFed,
                        daysSinceLastPetted = s.daysSinceLastPetted,
                        isFedToday = s.isFedToday,
                        isPettedToday = s.isPettedToday,
                        daysSinceLastProduct = s.daysSinceLastProduct,
                        isProductReady = s.isProductReady,
                        productQuality = s.productQuality,
                        purchaseDay = s.purchaseDay,
                        displayName = s.displayName
                    });
                }
            }
            Debug.Log($"[AnimalManager] Loaded: barnLevel={_barnLevel}, coopLevel={_coopLevel}, animals={_animals.Count}");
        }

        // ============================================================
        // 내부 헬퍼
        // ============================================================

        private AnimalData FindAnimalData(string id)
        {
            if (_animalDataRegistry == null) return null;
            foreach (var d in _animalDataRegistry)
                if (d != null && d.animalId == id) return d;
            return null;
        }

        private int CountAnimals(AnimalType type)
        {
            int count = 0;
            foreach (var a in _animals)
                if (a.data != null && a.data.animalType == type) count++;
            return count;
        }
    }
}
