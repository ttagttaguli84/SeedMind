// S-05: 대장간 NPC 고유 이벤트 허브
// -> see docs/systems/blacksmith-architecture.md 섹션 6.1
using System;

namespace SeedMind.NPC
{
    /// <summary>
    /// 대장간 NPC 고유 이벤트.
    /// NPCEvents(-> see npc-shop-architecture.md 섹션 4)를 보완한다.
    /// </summary>
    public static class BlacksmithEvents
    {
        /// <summary>업그레이드 UI에서 업그레이드 시작 요청 완료</summary>
        public static event Action<string, int> OnUpgradeRequested;
        // toolId, targetTier

        /// <summary>도구 수령(pickup) 완료</summary>
        public static event Action<string, int> OnToolPickedUp;
        // toolId, newTier

        /// <summary>친밀도 단계 상승</summary>
        public static event Action<string, int> OnAffinityLevelUp;
        // npcId, newLevel

        // --- 발행 메서드 ---
        internal static void RaiseUpgradeRequested(string toolId, int targetTier)
            => OnUpgradeRequested?.Invoke(toolId, targetTier);

        internal static void RaiseToolPickedUp(string toolId, int newTier)
            => OnToolPickedUp?.Invoke(toolId, newTier);

        internal static void RaiseAffinityLevelUp(string npcId, int newLevel)
            => OnAffinityLevelUp?.Invoke(npcId, newLevel);
    }
}
