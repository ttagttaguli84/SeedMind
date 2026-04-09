// T-06: 도구 업그레이드 이벤트 허브 (static)
// -> see docs/systems/tool-upgrade-architecture.md 섹션 3.2
using System;
using SeedMind.Player.Data;

namespace SeedMind.Player
{
    /// <summary>
    /// 도구 업그레이드 이벤트 발행/구독 허브.
    /// ToolUpgradeSystem이 발행하고, UI/ProgressionManager가 구독한다.
    /// -> see docs/systems/tool-upgrade-architecture.md 섹션 5.1
    /// </summary>
    public static class ToolUpgradeEvents
    {
        /// <summary>업그레이드 의뢰 시작 (골드/재료 차감 완료, 대기 시작)</summary>
        public static event Action<ToolUpgradeInfo> OnUpgradeStarted;

        /// <summary>업그레이드 완료 (도구 교체 완료)</summary>
        public static event Action<ToolUpgradeInfo> OnUpgradeCompleted;

        /// <summary>업그레이드 실패 (조건 불충족)</summary>
        public static event Action<ToolUpgradeFailReason> OnUpgradeFailed;

        internal static void RaiseUpgradeStarted(ToolUpgradeInfo info)
            => OnUpgradeStarted?.Invoke(info);

        internal static void RaiseUpgradeCompleted(ToolUpgradeInfo info)
            => OnUpgradeCompleted?.Invoke(info);

        internal static void RaiseUpgradeFailed(ToolUpgradeFailReason reason)
            => OnUpgradeFailed?.Invoke(reason);
    }
}
