// S-15: 여행 상인 방문 일정 관리 (MonoBehaviour)
// -> see docs/systems/npc-shop-architecture.md 섹션 3.5
using UnityEngine;
using SeedMind.NPC.Data;

namespace SeedMind.NPC
{
    public class TravelingMerchantScheduler : MonoBehaviour
    {
        [SerializeField] private NPCData _merchantNPCData;
        [SerializeField] private TravelingShopPoolData _shopPool;
        [SerializeField] private GameObject _merchantPrefab;
        [SerializeField] private Transform _spawnPosition;
        [SerializeField] private DayFlag _visitDays; // -> see docs/content/npcs.md 섹션 6.2 (토/일 고정)

        private bool _isPresent;
        private int _randomSeed;

        /// <summary>
        /// 고정 요일 스케줄 기반 방문 판단.
        /// 난수 주기 방식이 아닌 DayFlag 비트마스크로 등장 요일을 결정한다.
        /// </summary>
        public void CheckVisitSchedule(int currentDay, int currentDayOfWeek)
        {
            // currentDayOfWeek: 0=Mon ~ 6=Sun
            // DayFlag 비트마스크로 해당 요일인지 확인
            // -> see docs/content/npcs.md 섹션 6.2
            /* 등장일이면 SpawnMerchant(), 비등장일이면 DespawnMerchant() */
        }
        public void GenerateStock(int playerLevel /*, Season season */) { /* 재고 생성 */ }
        public void SpawnMerchant()
        {
            _isPresent = true;
            NPCEvents.RaiseTravelingMerchantArrived();
        }
        public void DespawnMerchant()
        {
            _isPresent = false;
            NPCEvents.RaiseTravelingMerchantDeparted();
        }
        public TravelingMerchantSaveData GetSaveData() { return new TravelingMerchantSaveData(); }
        public void LoadSaveData(TravelingMerchantSaveData data) { /* 복원 */ }
        // [구독] TimeManager.OnDayChanged -> CheckVisitSchedule()
        // 등장 요일: -> see docs/content/npcs.md 섹션 6.2 (매주 토/일 고정)
        // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.5
    }
}