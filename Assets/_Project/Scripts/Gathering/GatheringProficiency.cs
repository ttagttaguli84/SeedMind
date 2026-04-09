// 채집 숙련도 — 레벨 1~10, XP 기반 보너스 (ARC-031)
// FishingProficiency와 동일한 패턴
// -> see docs/systems/gathering-architecture.md 섹션 4
using UnityEngine;
using SeedMind.Economy;

namespace SeedMind.Gathering
{
    /// <summary>
    /// 채집 전용 숙련도 시스템.
    /// GatheringManager가 보유하며, 세이브 데이터로 직렬화된다.
    /// </summary>
    public class GatheringProficiency
    {
        private int _currentXP;
        private int _currentLevel = 1;
        private readonly GatheringConfig _config;

        public int CurrentXP    => _currentXP;
        public int CurrentLevel => _currentLevel;
        public bool IsMaxLevel  => _currentLevel >= (_config?.proficiencyMaxLevel ?? 10);

        public int XPToNextLevel
        {
            get
            {
                if (_config == null || IsMaxLevel) return 0;
                int idx = _currentLevel - 1;
                if (idx < 0 || idx >= _config.proficiencyXPThresholds.Length) return 0;
                return Mathf.Max(0, _config.proficiencyXPThresholds[idx] - _currentXP);
            }
        }

        public System.Action<int> OnLevelUp;

        public GatheringProficiency(GatheringConfig config)
        {
            _config = config;
        }

        /// <summary>XP 추가. 레벨업 시 OnLevelUp 발행.</summary>
        public void AddXP(int amount)
        {
            if (IsMaxLevel || amount <= 0) return;

            _currentXP += amount;

            int maxLevel = _config?.proficiencyMaxLevel ?? 10;
            while (_currentLevel < maxLevel)
            {
                int idx = _currentLevel - 1;
                if (idx >= 0 && idx < _config.proficiencyXPThresholds.Length
                    && _currentXP >= _config.proficiencyXPThresholds[idx])
                {
                    _currentLevel++;
                    OnLevelUp?.Invoke(_currentLevel);
                    Debug.Log($"[GatheringProficiency] 채집 숙련도 레벨업! Lv.{_currentLevel}");
                }
                else break;
            }
        }

        /// <summary>희귀도별 채집 숙련도 XP 반환.</summary>
        public int GetXPForGather(GatheringRarity rarity)
        {
            if (_config?.gatherXPByRarity == null) return 5;
            int idx = (int)rarity;
            return idx < _config.gatherXPByRarity.Length ? _config.gatherXPByRarity[idx] : 5;
        }

        /// <summary>레벨별 보너스 수량 획득 확률.</summary>
        public float GetBonusQuantityChance()
        {
            if (_config?.bonusQuantityByLevel == null || _currentLevel <= 0) return 0f;
            int idx = _currentLevel - 1;
            return idx < _config.bonusQuantityByLevel.Length ? _config.bonusQuantityByLevel[idx] : 0f;
        }

        /// <summary>레벨별 희귀 아이템 출현 확률 보정.</summary>
        public float GetRarityBonus()
        {
            if (_config?.rarityBonusByLevel == null || _currentLevel <= 0) return 0f;
            int idx = _currentLevel - 1;
            return idx < _config.rarityBonusByLevel.Length ? _config.rarityBonusByLevel[idx] : 0f;
        }

        /// <summary>레벨별 최대 품질.</summary>
        public CropQuality GetMaxGatherQuality()
        {
            if (_config?.maxQualityByLevel == null || _currentLevel <= 0) return CropQuality.Normal;
            int idx = _currentLevel - 1;
            return idx < _config.maxQualityByLevel.Length ? _config.maxQualityByLevel[idx] : CropQuality.Normal;
        }

        /// <summary>레벨별 에너지 소모 감소량.</summary>
        public int GetEnergyCostReduction()
        {
            if (_config?.energyCostReductionByLevel == null || _currentLevel <= 0) return 0;
            int idx = _currentLevel - 1;
            return idx < _config.energyCostReductionByLevel.Length ? _config.energyCostReductionByLevel[idx] : 0;
        }

        /// <summary>레벨별 채집 속도 배율.</summary>
        public float GetGatherSpeedMultiplier()
        {
            if (_config?.gatherSpeedMultiplierByLevel == null || _currentLevel <= 0) return 1f;
            int idx = _currentLevel - 1;
            return idx < _config.gatherSpeedMultiplierByLevel.Length ? _config.gatherSpeedMultiplierByLevel[idx] : 1f;
        }

        // ── 직렬화 ────────────────────────────────────────────────────

        public (int xp, int level) GetSaveData() => (_currentXP, _currentLevel);

        public void LoadSaveData(int xp, int level)
        {
            _currentXP    = xp;
            _currentLevel = Mathf.Clamp(level, 1, _config?.proficiencyMaxLevel ?? 10);
        }
    }
}
