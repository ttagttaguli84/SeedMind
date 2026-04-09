// 채집 포인트 ScriptableObject — 포인트 정의 데이터
// -> see docs/systems/gathering-architecture.md 섹션 2.1
using UnityEngine;

namespace SeedMind.Gathering
{
    [CreateAssetMenu(fileName = "SO_GPoint_New", menuName = "SeedMind/Gathering/GatheringPointData")]
    public class GatheringPointData : ScriptableObject
    {
        [Header("식별")]
        public string pointId;       // 예: "gp_forest_01"
        public string displayName;   // 예: "숲 덤불"
        public string description;

        [Header("Zone 설정")]
        public string zoneId;                       // 소속 Zone (예: "zone_d")
        public bool requiredZoneUnlocked = true;    // Zone 해금 필요 여부

        [Header("아이템 풀")]
        public GatheringItemEntry[] availableItems;        // 기본 아이템 풀
        public SeasonalItemOverride[] seasonOverrides;     // 계절별 오버라이드

        [Header("재생성 설정")]
        public int respawnDays = 3;       // -> see docs/systems/gathering-system.md 섹션 2
        public int respawnVariance = 1;   // 재생성 일수 변동폭 (+/-)

        [Header("비주얼")]
        public GameObject pointPrefab;    // 활성 상태 프리팹
        public GameObject depletedPrefab; // 소진 상태 프리팹
        public GameObject gatherVFX;      // 채집 이펙트
    }
}
