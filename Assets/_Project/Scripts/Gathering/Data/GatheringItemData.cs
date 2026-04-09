// 채집 아이템 ScriptableObject
// -> see docs/systems/gathering-architecture.md 섹션 2.2
using UnityEngine;
using SeedMind.Core;
using SeedMind.Farm.Data;
using SeedMind.Fishing;
using SeedMind.Economy;
using SeedMind.Player.Data;

namespace SeedMind.Gathering
{
    [CreateAssetMenu(fileName = "SO_GItem_New", menuName = "SeedMind/Gathering/GatheringItemData")]
    public class GatheringItemData : GameDataSO, SeedMind.IInventoryItem
    {
        // dataId, displayName, icon 은 GameDataSO에서 상속

        [Header("설명")]
        public string description;

        [Header("채집 속성")]
        public GatheringCategory gatheringCategory;
        public GatheringRarity rarity;
        public int basePrice;   // -> see docs/balance/gathering-economy.md

        [Header("등장 조건")]
        public SeasonFlag seasonAvailability;     // 출현 계절 비트 플래그
        public WeatherFlag weatherBonus;          // 날씨 보너스 (SeedMind.Fishing.WeatherFlag)
        public Vector2Int baseQuantityRange = new Vector2Int(1, 2);  // 수량 범위
        public bool qualityEnabled = true;

        [Header("인벤토리")]
        public int maxStackSize = 99;   // -> see docs/pipeline/data-pipeline.md 섹션 2.9

        [Header("채집 조건")]
        public int expReward;           // -> see docs/balance/progression-curve.md
        public float gatherTimeSec = 1.5f;  // -> see docs/systems/gathering-system.md 섹션 2
        public ToolType requiredTool = ToolType.Hand;
        public int minProficiencyLevel = 0;

        // ── IInventoryItem 구현 ─────────────────────────────────────
        public string ItemId         => dataId;
        public string ItemName       => displayName;
        public SeedMind.ItemType ItemType => SeedMind.ItemType.Gather;
        public Sprite Icon           => icon;
        public int MaxStackSize      => maxStackSize;
        public bool Sellable         => true;
    }
}
