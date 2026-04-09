// T-02: 업그레이드 진행 상태 데이터 클래스
// -> see docs/systems/tool-upgrade-architecture.md 섹션 3.1
namespace SeedMind.Player.Data
{
    /// <summary>
    /// 진행 중인 도구 업그레이드 런타임 상태.
    /// ToolUpgradeSystem._pendingUpgrades 딕셔너리의 값.
    /// </summary>
    [System.Serializable]
    public class PendingUpgrade
    {
        public ToolType toolType;        // 업그레이드 중인 도구 종류
        public string currentToolId;     // 업그레이드 전 도구 dataId
        public string targetToolId;      // 업그레이드 후 도구 dataId
        public int remainingDays;        // 남은 소요 일수 (-> see docs/systems/tool-upgrade.md)
        public int totalDays;            // 총 소요 일수 (-> see docs/systems/tool-upgrade.md)
    }
}
