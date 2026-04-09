// AnimalInstance — 개별 동물 런타임 인스턴스 (Serializable, SO 아님)
// -> see docs/systems/livestock-architecture.md 섹션 3
using System;
using SeedMind.Economy;
using SeedMind.Livestock.Data;

namespace SeedMind.Livestock
{
    [Serializable]
    public class AnimalInstance
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
        public CropQuality productQuality;  // -> see docs/systems/economy-architecture.md 섹션 4.4
        public int purchaseDay;
        public string displayName;

        [NonSerialized] public AnimalData data; // 런타임 참조 (저장 안 됨)
    }
}
