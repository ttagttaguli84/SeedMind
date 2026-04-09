// T-04: 도구 업그레이드 세이브 데이터
// -> see docs/systems/tool-upgrade-architecture.md 섹션 7.2
namespace SeedMind.Player.Data
{
    /// <summary>
    /// 도구 업그레이드 진행 상태 세이브 데이터.
    /// PlayerSaveData.toolUpgradeState 필드에 포함된다.
    /// -> see docs/systems/tool-upgrade-architecture.md 섹션 7.2
    /// </summary>
    [System.Serializable]
    public class ToolUpgradeSaveData
    {
        public PendingUpgradeSaveEntry[] pendingUpgrades;
    }

    /// <summary>
    /// 진행 중인 업그레이드 1건의 직렬화 표현.
    /// </summary>
    [System.Serializable]
    public class PendingUpgradeSaveEntry
    {
        public int toolTypeIndex;       // (int)ToolType
        public string currentToolId;    // 업그레이드 전 도구 dataId
        public string targetToolId;     // 업그레이드 후 도구 dataId
        public int remainingDays;       // 남은 소요 일수
        public int totalDays;           // 총 소요 일수
    }
}
