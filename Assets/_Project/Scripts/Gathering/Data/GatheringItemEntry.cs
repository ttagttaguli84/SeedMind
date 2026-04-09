// 채집 포인트 아이템 풀 엔트리 (가중치 포함)
// -> see docs/systems/gathering-architecture.md 섹션 2.1
namespace SeedMind.Gathering
{
    [System.Serializable]
    public struct GatheringItemEntry
    {
        public GatheringItemData item;   // 아이템 SO 참조
        public float weight;             // 가중치 (절대값, 합산하여 정규화)
    }
}
