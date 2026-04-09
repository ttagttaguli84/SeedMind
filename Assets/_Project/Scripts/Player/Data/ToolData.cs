using UnityEngine;
using SeedMind.Core;
using SeedMind.Player;

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
    /// -> see docs/systems/tool-upgrade-architecture.md for 업그레이드 필드
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Tool", menuName = "SeedMind/Player/ToolData")]
    public class ToolData : GameDataSO, SeedMind.IInventoryItem
    {
        // dataId, displayName, icon 은 GameDataSO에서 상속

        [Header("도구 속성")]
        public ToolType toolType;
        public int tier;        // 등급 (1=Basic, 2=Reinforced, 3=Legendary) -> see docs/systems/tool-upgrade.md 섹션 1.1
        public int range;       // 사용 범위 (타일 수) -> see docs/systems/tool-upgrade.md 섹션 3.1~3.3

        [Header("사용 비용")]
        public int energyCost;  // 에너지 소모 -> see docs/systems/tool-upgrade.md 섹션 3.1~3.3
        public float cooldown;  // 쿨다운(초) -> see docs/systems/tool-upgrade.md 섹션 3.1~3.3
        public float useSpeed;  // 사용 속도 배수 (기본 1.0)

        [Header("업그레이드")]
        public ToolData nextTier;               // 다음 등급 SO 참조 (체인: Basic→Reinforced→Legendary→null)
        public int upgradeGoldCost;             // 골드 비용 -> see docs/systems/tool-upgrade.md 섹션 2.1
        public int upgradeTimeDays;             // 소요 일수 -> see docs/systems/tool-upgrade.md 섹션 2.1
        public int requiredLevel;               // 해금 레벨 -> see docs/systems/tool-upgrade.md 섹션 2.1
        public LevelReqType levelReqType;       // 해금 조건 타입 (FIX-086) -> see 섹션 3.6
        public UpgradeMaterial[] upgradeMaterials; // 필요 재료 -> see docs/systems/tool-upgrade.md 섹션 2.1
        public ToolSpecialEffect specialEffect; // 특수 효과 [Flags] -> see docs/systems/tool-upgrade.md 섹션 3.1~3.3

        [Header("설명")]
        public string description;

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
