// 채집 결과 struct
// -> see docs/systems/gathering-architecture.md 섹션 1 (시스템 다이어그램)
using SeedMind.Economy;

namespace SeedMind.Gathering
{
    public struct GatherResult
    {
        public bool success;
        public GatheringItemData item;
        public CropQuality quality;
        public int quantity;
        public bool bonusTriggered;   // 숙련도 보너스 드롭 여부

        public static GatherResult Fail => new GatherResult { success = false };
    }
}
