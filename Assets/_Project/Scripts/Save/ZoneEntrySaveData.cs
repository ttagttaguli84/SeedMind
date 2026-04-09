// 구역별 세이브 데이터
// -> see docs/systems/farm-expansion-architecture.md 섹션 9.1
// PATTERN-005: JSON 필드 3개(zoneId, isUnlocked, obstacles) = C# 필드 3개 — 일치
using System;

namespace SeedMind.Save
{
    [Serializable]
    public class ZoneEntrySaveData
    {
        public string zoneId;               // ZoneData.zoneId 참조
        public bool isUnlocked;             // 해금 여부
        public ObstacleSaveData[] obstacles; // 장애물 상태 배열
    }
}
