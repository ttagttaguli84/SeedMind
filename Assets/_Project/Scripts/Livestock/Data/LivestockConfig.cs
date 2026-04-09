// LivestockConfig — 목축 시스템 전역 설정 SO
// -> see docs/content/livestock-system.md 섹션 3.1, 3.2, 5 for canonical 수치
using UnityEngine;

namespace SeedMind.Livestock.Data
{
    [CreateAssetMenu(fileName = "SO_LivestockConfig", menuName = "SeedMind/Livestock/LivestockConfig")]
    public class LivestockConfig : ScriptableObject
    {
        [Header("닭장 (Chicken Coop)")]
        public int initialCoopCapacity;         // -> see docs/content/livestock-system.md 섹션 3.1
        public int[] coopUpgradeCapacity;
        public int[] coopUpgradeCost;

        [Header("외양간 (Barn)")]
        public int initialBarnCapacity;         // -> see docs/content/livestock-system.md 섹션 3.2
        public int[] barnUpgradeCapacity;
        public int[] barnUpgradeCost;

        [Header("행복도 품질 임계값")]
        public float goldQualityThreshold;      // -> see docs/content/livestock-system.md 섹션 5.3
        public float silverQualityThreshold;

        [Header("방치 패널티")]
        public int neglectThresholdDays;        // -> see docs/content/livestock-system.md 섹션 5.4
        public float neglectPenaltyPerDay;

        [Header("초기 행복도")]
        public float initialHappiness;          // -> see docs/content/livestock-system.md 섹션 5.1

        [Header("생산량 배수 커브 (행복도 0~100 → 배수)")]
        public AnimationCurve productionMultiplierCurve;

        private void Reset()
        {
            // AnimationCurve 기본값: 행복도 0 → 0.5배, 100 → 1.5배
            productionMultiplierCurve = AnimationCurve.Linear(0f, 0.5f, 100f, 1.5f);
        }
    }
}
