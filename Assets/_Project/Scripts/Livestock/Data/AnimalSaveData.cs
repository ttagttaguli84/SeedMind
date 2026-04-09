// AnimalSaveData — 목축 시스템 세이브 데이터
// -> see docs/systems/livestock-architecture.md 섹션 8.2
using System;
using SeedMind.Economy;

namespace SeedMind.Livestock
{
    [Serializable]
    public class AnimalSaveData
    {
        public bool isUnlocked;
        public int barnLevel;
        public int coopLevel;
        public AnimalInstanceSaveData[] animals;
    }

    [Serializable]
    public class AnimalInstanceSaveData
    {
        public string instanceId;
        public string animalDataId;
        public float happiness;
        public int daysSinceLastFed;
        public int daysSinceLastPetted;
        public bool isFedToday;
        public bool isPettedToday;
        public int daysSinceLastProduct;
        public bool isProductReady;
        public CropQuality productQuality;
        public int purchaseDay;
        public string displayName;
    }
}
