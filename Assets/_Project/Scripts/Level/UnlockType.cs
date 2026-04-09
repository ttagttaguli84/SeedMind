// 해금 항목 유형 열거형
// -> see docs/systems/progression-architecture.md 섹션 3.1
namespace SeedMind.Level
{
    public enum UnlockType
    {
        Crop,           // 작물 (CropData.unlockLevel)
        Facility,       // 시설 (BuildingData.requiredLevel)
        Fertilizer,     // 비료 (FertilizerData.unlockLevel)
        Tool,           // 도구 등급 (ToolData.tier)
        Recipe,         // 가공 레시피
        FarmExpansion,  // 농장 확장 단계
    }
}
