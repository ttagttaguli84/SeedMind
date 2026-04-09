// AnimalData — 동물 종별 기본 데이터 SO
// -> see docs/content/livestock-system.md 섹션 1.1, 2.2 for canonical 수치
using UnityEngine;

namespace SeedMind.Livestock.Data
{
    [CreateAssetMenu(fileName = "SO_Animal_", menuName = "SeedMind/Livestock/AnimalData")]
    public class AnimalData : ScriptableObject
    {
        [Header("식별")]
        public string animalId;
        public string animalName;
        public AnimalType animalType;

        [Header("구매")]
        public int purchasePrice;       // -> see docs/content/livestock-system.md 섹션 1.1
        public int unlockLevel;         // -> see docs/content/livestock-system.md 섹션 1.2

        [Header("사료")]
        public string requiredFeedId;   // -> see docs/content/livestock-system.md 섹션 2.2
        public int dailyFeedAmount;     // 1마리 기준 하루 사료 개수

        [Header("생산물")]
        public string productItemId;            // -> see docs/content/livestock-system.md 섹션 4.1
        public int productionIntervalDays;       // -> see docs/content/livestock-system.md 섹션 1.1
        public int baseProductAmount;            // -> see docs/content/livestock-system.md 섹션 4.1

        [Header("행복도")]
        public float baseHappinessDecay;    // 하루 기본 감소량 -> see docs/content/livestock-system.md 섹션 5.2
        public float feedHappinessGain;     // 먹이 줄 때 증가량
        public float petHappinessGain;      // 쓰다듬을 때 증가량

        [Header("비주얼")]
        public GameObject animalPrefab;
    }
}
