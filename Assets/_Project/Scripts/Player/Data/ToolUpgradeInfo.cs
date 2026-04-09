// T-03: 업그레이드 이벤트 페이로드 및 검증 구조체
// -> see docs/systems/tool-upgrade-architecture.md 섹션 3.2~3.4
namespace SeedMind.Player.Data
{
    /// <summary>
    /// OnUpgradeStarted / OnUpgradeCompleted 이벤트 페이로드.
    /// </summary>
    public struct ToolUpgradeInfo
    {
        public ToolType toolType;
        public ToolData previousTool;   // 업그레이드 전 SO
        public ToolData upgradedTool;   // 업그레이드 후 SO (Complete 시 유효)
        public int newTier;             // 새 등급 1~3 (-> see docs/systems/tool-upgrade.md 섹션 1.1)
    }

    /// <summary>
    /// ToolUpgradeSystem.CanUpgrade() 반환값.
    /// </summary>
    public struct UpgradeCheckResult
    {
        public bool canUpgrade;
        public ToolUpgradeFailReason failReason; // None이면 업그레이드 가능
        public UpgradeCostInfo cost;             // 필요 비용 정보
    }

    /// <summary>
    /// 업그레이드 불가 사유.
    /// </summary>
    public enum ToolUpgradeFailReason
    {
        None,                   // 업그레이드 가능
        AlreadyMaxTier,         // 최고 등급 도달
        InsufficientGold,       // 골드 부족
        InsufficientMaterials,  // 재료 부족
        AlreadyUpgrading,       // 해당 도구가 이미 업그레이드 중
        ToolNotOwned,           // 해당 도구를 소유하지 않음
        LevelTooLow,            // 플레이어 레벨 부족 (PlayerLevel)
        MasteryTooLow           // 숙련도 레벨 부족 (GatheringMastery 등, FIX-086)
    }

    /// <summary>
    /// 업그레이드에 필요한 비용 정보. UI 표시 및 검증에 사용.
    /// </summary>
    public struct UpgradeCostInfo
    {
        public int goldCost;                    // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
        public UpgradeMaterial[] materials;     // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
        public int timeDays;                    // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
        public LevelReqType levelReqType;       // 해금 조건 타입 (FIX-086)
        public int requiredLevel;               // 해금 조건 레벨 값
    }
}
