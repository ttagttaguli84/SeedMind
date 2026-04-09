// HappinessCalculator — 동물 행복도/품질 계산 유틸리티 (static)
// -> see docs/systems/livestock-architecture.md 섹션 5.1
// -> see docs/content/livestock-system.md 섹션 5 for canonical 수치
using SeedMind.Economy;
using SeedMind.Livestock.Data;

namespace SeedMind.Livestock
{
    public static class HappinessCalculator
    {
        /// <summary>하루 행복도 변화량 계산 (먹이/쓰다듬 여부 반영)</summary>
        public static float CalculateDailyDelta(AnimalInstance animal, LivestockConfig config)
        {
            if (animal == null || config == null || animal.data == null) return 0f;

            float delta = -animal.data.baseHappinessDecay;

            if (animal.isFedToday)
                delta += animal.data.feedHappinessGain;

            if (animal.isPettedToday)
                delta += animal.data.petHappinessGain;

            // 방치 패널티: config.neglectThresholdDays 초과 시
            if (animal.daysSinceLastFed >= config.neglectThresholdDays)
                delta -= config.neglectPenaltyPerDay;

            return delta;
        }

        /// <summary>행복도에 따른 생산량 배수 반환</summary>
        public static float GetProductionMultiplier(float happiness, LivestockConfig config)
        {
            if (config == null || config.productionMultiplierCurve == null) return 1f;
            return config.productionMultiplierCurve.Evaluate(happiness);
        }

        /// <summary>행복도에 따른 생산물 품질 결정</summary>
        public static CropQuality GetProductQuality(float happiness, LivestockConfig config)
        {
            if (config == null) return CropQuality.Normal;

            if (happiness >= config.goldQualityThreshold)
                return CropQuality.Gold;
            if (happiness >= config.silverQualityThreshold)
                return CropQuality.Silver;
            return CropQuality.Normal;
        }

        public static float Clamp(float happiness)
            => UnityEngine.Mathf.Clamp(happiness, 0f, 200f); // -> see docs/content/livestock-system.md 섹션 5.1 (최대 200)
    }
}
