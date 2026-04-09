// 구역 내 장애물 배치 정의 구조체 (ScriptableObject에 직렬화)
// -> see docs/systems/farm-expansion-architecture.md 섹션 3
using System;
using UnityEngine;

namespace SeedMind.Farm
{
    [Serializable]
    public struct ObstacleEntry
    {
        // 구역 내 상대 좌표 (좌하단 기준, Zone 로컬 좌표)
        public Vector2Int localPosition;

        // 장애물 종류
        public ObstacleType type;

        // 제거 필요 타격 수
        // -> see docs/systems/farm-expansion.md 섹션 3.1
        public int maxHP;

        // 제거 시 드랍 아이템 ID 배열
        // -> see docs/systems/farm-expansion.md 섹션 3.4
        public string[] lootDropIds;

        // 장애물 3D 모델 프리팹
        public GameObject obstaclePrefab;
    }
}
