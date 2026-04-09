// UpgradeMaterial: 도구 업그레이드 재료 항목 직렬화 구조체
// -> see docs/systems/tool-upgrade.md 섹션 2.1
namespace SeedMind.Player.Data
{
    /// <summary>
    /// 업그레이드에 필요한 재료 아이템과 수량.
    /// ToolData.upgradeMaterials 배열의 원소.
    /// </summary>
    [System.Serializable]
    public class UpgradeMaterial
    {
        public string materialId;  // IInventoryItem.ItemId (예: "iron_scrap")
        public int quantity;       // 필요 수량 (-> see docs/systems/tool-upgrade.md 섹션 2.1)
    }
}
