// S-06: 상황별 힌트 발동 조건 유형
// -> see docs/systems/tutorial-architecture.md 섹션 8.2
namespace SeedMind.Tutorial.Data
{
    public enum HintConditionType
    {
        DryTilesExist      = 0,
        LowGold            = 1,
        InventoryFull      = 2,
        SeasonMismatchCrop = 3,
        UnusedFertilizer   = 4,
        ReadyToHarvest     = 5,
        NightTime          = 6,
        ProcessingReady    = 7
    }
}
