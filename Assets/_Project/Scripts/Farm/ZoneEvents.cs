// 구역 해금/개간 관련 이벤트 중앙 집중 관리 (static class)
// FarmEvents 패턴과 동일하게 public static Action (event 키워드 없음) 사용
// -> see docs/systems/farm-expansion-architecture.md 섹션 7.1
using System;
using UnityEngine;

namespace SeedMind.Farm
{
    public static class ZoneEvents
    {
        // 구역 해금 성공 — (zoneId, ZoneData)
        public static Action<string, ZoneData> OnZoneUnlocked;

        // 구역 해금 실패 — (zoneId, reason)
        public static Action<string, ZoneUnlockFailReason> OnZoneUnlockFailed;

        // 장애물 타격 — (tilePos, remainingHP)
        public static Action<Vector2Int, int> OnObstacleHit;

        // 장애물 제거 완료 — (tilePos, obstacleType)
        public static Action<Vector2Int, ObstacleType> OnObstacleCleared;

        // 구역 완전 개간 — (zoneId)
        public static Action<string> OnZoneFullyCleared;
    }
}
