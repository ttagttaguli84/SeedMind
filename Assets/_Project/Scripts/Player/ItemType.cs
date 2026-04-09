namespace SeedMind
{
    /// <summary>
    /// 인벤토리 아이템 분류 enum.
    /// -> see docs/systems/inventory-architecture.md 섹션 3.2
    /// </summary>
    public enum ItemType
    {
        Crop,        // 수확물 (감자, 당근 등)
        Seed,        // 씨앗 (씨앗 형태 CropData)
        Tool,        // 도구 (호미, 물뿌리개 등)
        Fertilizer,  // 비료
        Fish,        // 물고기
        Processed,   // 가공품 (잼, 주스 등)
        Gather,      // 채집품 (버섯, 나뭇가지 등)
        Building,    // 건물 아이템
        Misc         // 기타
    }
}
