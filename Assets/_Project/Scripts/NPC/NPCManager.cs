// S-12: NPC 레지스트리 관리 (MonoBehaviour Singleton)
// -> see docs/systems/npc-shop-architecture.md 섹션 3.2
using UnityEngine;
using System.Collections.Generic;
using SeedMind.NPC.Data;

namespace SeedMind.NPC
{
    public class NPCManager : MonoBehaviour
    {
        [SerializeField] private NPCData[] _npcRegistry;

        private Dictionary<string, NPCController> _activeNPCs
            = new Dictionary<string, NPCController>();
        private Dictionary<string, NPCActivityState> _npcStates
            = new Dictionary<string, NPCActivityState>();

        public IReadOnlyDictionary<string, NPCController> ActiveNPCs => _activeNPCs;

        public void Initialize() { /* NPC 등록, 초기 상태 설정 */ }
        public NPCController GetNPC(string npcId) => _activeNPCs.GetValueOrDefault(npcId);
        public bool IsNPCAvailable(string npcId)
            => _npcStates.TryGetValue(npcId, out var s) && s == NPCActivityState.Active;
        public void RefreshNPCStates(int currentHour, int currentDay) { /* 시간 기반 상태 갱신 */ }
        public NPCSaveData GetSaveData() { return new NPCSaveData(); }
        public void LoadSaveData(NPCSaveData data) { /* 세이브 복원 */ }
        // [구독] TimeManager.OnHourChanged -> RefreshNPCStates()
        // [구독] WeatherSystem.OnWeatherChanged -> HandleWeatherChange()
        // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.2
    }
}