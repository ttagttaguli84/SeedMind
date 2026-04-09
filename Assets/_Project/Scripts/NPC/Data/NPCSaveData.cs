// S-10: NPC 세이브 데이터
// -> see docs/systems/npc-shop-architecture.md 섹션 7.1~7.2
namespace SeedMind.NPC.Data
{
    [System.Serializable]
    public class NPCSaveData
    {
        public TravelingMerchantSaveData travelingMerchant;
    }

    [System.Serializable]
    public class TravelingMerchantSaveData
    {
        public bool isPresent;
        public int randomSeed;
        public string[] currentStockItemIds;
        public int[] currentStockQuantities;
        // DayFlag 기반 고정 스케줄이므로 nextVisitDay, departureDayOffset 불필요
        // -> see docs/content/npcs.md 섹션 6.2
    }
}