// S-11: NPC 정적 이벤트 허브
// -> see docs/systems/npc-shop-architecture.md 섹션 4
using System;
using SeedMind.NPC.Data;

namespace SeedMind.NPC
{
    public static class NPCEvents
    {
        // --- 대화 ---
        public static event Action<string, DialogueData> OnDialogueStarted;
        public static event Action<string> OnDialogueEnded;

        // --- 상점 ---
        public static event Action<string> OnShopOpened;
        public static event Action<string> OnShopClosed;

        // --- NPC 상태 ---
        public static event Action<string, NPCActivityState> OnNPCStateChanged;

        // --- 여행 상인 ---
        public static event Action OnTravelingMerchantArrived;
        public static event Action OnTravelingMerchantDeparted;

        // --- 발행 메서드 (internal) ---
        internal static void RaiseDialogueStarted(string npcId, DialogueData data)
            => OnDialogueStarted?.Invoke(npcId, data);
        internal static void RaiseDialogueEnded(string npcId)
            => OnDialogueEnded?.Invoke(npcId);
        internal static void RaiseShopOpened(string npcId)
            => OnShopOpened?.Invoke(npcId);
        internal static void RaiseShopClosed(string npcId)
            => OnShopClosed?.Invoke(npcId);
        internal static void RaiseNPCStateChanged(string npcId, NPCActivityState state)
            => OnNPCStateChanged?.Invoke(npcId, state);
        internal static void RaiseTravelingMerchantArrived()
            => OnTravelingMerchantArrived?.Invoke();
        internal static void RaiseTravelingMerchantDeparted()
            => OnTravelingMerchantDeparted?.Invoke();
    }
}