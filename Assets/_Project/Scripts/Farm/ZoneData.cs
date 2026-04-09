// 농장 구역 정적 데이터 ScriptableObject
// -> see docs/systems/farm-expansion-architecture.md 섹션 3
using UnityEngine;

namespace SeedMind.Farm
{
    [CreateAssetMenu(fileName = "SO_Zone", menuName = "SeedMind/Farm/ZoneData")]
    public class ZoneData : ScriptableObject
    {
        [Header("식별")]
        public string zoneId;       // 고유 식별자 (예: "zone_home")
        public string zoneName;     // UI 표시용 이름
        public int sortOrder;       // 해금 순서, UI 정렬용

        [Header("해금 조건")]
        // -> see docs/systems/farm-expansion.md 섹션 2.1
        public int requiredLevel;       // 레벨 요건 (0 = 없음)
        public int unlockCost;          // 해금 골드 비용
        public string prerequisiteZoneId; // 선행 구역 ID (빈 문자열이면 선행 없음)

        [Header("구역 특성")]
        public ZoneType zoneType;

        [Header("타일 배치")]
        // 이 구역에 속하는 FarmGrid 절대 좌표 목록
        // -> see docs/systems/farm-expansion.md 섹션 1.2, 1.3
        public Vector2Int[] tilePositions;

        [Header("장애물")]
        // 초기 장애물 배치 — 구역 해금 시 스폰
        // -> see docs/systems/farm-expansion.md 섹션 3.2
        public ObstacleEntry[] obstacleMap;

        [Header("비주얼 (선택)")]
        public Material lockedOverlayMaterial;
        public GameObject unlockVFXPrefab;
    }
}
