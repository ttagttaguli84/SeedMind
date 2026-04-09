namespace SeedMind.Building
{
    /// <summary>
    /// 창고의 개별 슬롯. 아이템 ID, 수량, 품질을 저장.
    /// -> see docs/systems/facilities-architecture.md 섹션 6.2
    /// </summary>
    public class StorageSlot
    {
        public string ItemId { get; set; }
        public int Quantity { get; set; }
        public string Quality { get; set; }
        public bool IsEmpty => string.IsNullOrEmpty(ItemId);
    }
}
