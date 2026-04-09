using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Player.Data
{
    public enum ToolType
    {
        Hoe        = 0,
        WateringCan = 1,
        SeedBag    = 2,
        Sickle     = 3,
        Hand       = 4
    }

    /// <summary>
    /// 도구 데이터 ScriptableObject.
    /// GameDataSO 상속, IInventoryItem 구현.
    /// -> see docs/systems/inventory-architecture.md 섹션 4.2
    /// -> see docs/systems/farming-architecture.md 섹션 4.3 for 필드 정의
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Tool", menuName = "SeedMind/Player/ToolData")]
    public class ToolData : GameDataSO, SeedMind.IInventoryItem
    {
        // dataId, displayName, icon 은 GameDataSO에서 상속

        [Header("도구 속성")]
        public ToolType toolType;
        public int tier;    // 등급 (1=기본, 2=중급, 3=고급) -> see docs/mcp/tool-upgrade-tasks.md
        public int range;   // 사용 범위 (타일 수) -> see docs/mcp/farming-tasks.md section B-4

        // ── IInventoryItem 구현 ─────────────────────────────────────
        // -> see docs/systems/inventory-architecture.md 섹션 4.2
        public string ItemId => dataId;
        public string ItemName => displayName;
        public SeedMind.ItemType ItemType => SeedMind.ItemType.Tool;
        public Sprite Icon => icon;
        public int MaxStackSize => 1;    // 도구는 스택 불가 -> see docs/systems/inventory-system.md 섹션 1.1
        public bool Sellable => false;   // 도구는 판매 불가
    }
}
