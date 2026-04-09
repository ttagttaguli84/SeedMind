// S-17: NPC 의뢰 등장/쿨다운 관리
// -> see docs/systems/quest-architecture.md 섹션 7.1
// -> see docs/systems/quest-system.md 섹션 4.1 for 의뢰 규칙
using System.Collections.Generic;
using SeedMind.Quest.Data;

namespace SeedMind.Quest
{
    public class NPCRequestScheduler
    {
        private QuestData[] _npcRequestPool;
        private QuestManager _manager;
        private Dictionary<string, int> _npcCooldowns;
        private int _activeRequestCount;

        public NPCRequestScheduler(QuestData[] npcRequestPool,
            QuestManager manager)
        {
            _npcRequestPool = npcRequestPool;
            _manager = manager;
            _npcCooldowns = new Dictionary<string, int>();
        }

        public void TryOfferNewRequests(int currentDay,
            /* Season season, */ int playerLevel) { /* 의뢰 제안 로직 */ }
        public void UpdateCooldowns() { /* 쿨다운 1일 감소 */ }
        public void OnRequestCompleted(string questId) { /* 완료 처리 */ }
        public void OnRequestFailed(string questId) { /* 실패 처리, 쿨다운 시작 */ }

        public NPCRequestSaveState GetSaveState()
        {
            return new NPCRequestSaveState
            {
                cooldowns = new Dictionary<string, int>(_npcCooldowns),
                activeRequestCount = _activeRequestCount
            };
        }

        public void LoadSaveState(NPCRequestSaveState state)
        {
            if (state == null) return;
            _npcCooldowns = state.cooldowns
                ?? new Dictionary<string, int>();
            _activeRequestCount = state.activeRequestCount;
        }
    }
}