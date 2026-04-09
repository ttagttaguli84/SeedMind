// S-09: 여행 상인 후보 아이템 풀 ScriptableObject
// -> see docs/systems/npc-shop-architecture.md 섹션 2.3
using UnityEngine;

namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewTravelingShopPool", menuName = "SeedMind/TravelingShopPool")]
    public class TravelingShopPoolData : ScriptableObject
    {
        public string poolId;

        [Header("후보 아이템")]
        public TravelingShopCandidate[] candidates;

        [Header("출현 규칙")]
        public int minItemCount;                 // -> see docs/content/npcs.md
        public int maxItemCount;                 // -> see docs/content/npcs.md
    }

    [System.Serializable]
    public class TravelingShopCandidate
    {
        public ScriptableObject itemReference;   // CropData, FertilizerData 등
        public float selectionWeight;
        public int minPlayerLevel;
        public int stockMin;
        public int stockMax;
    }
}