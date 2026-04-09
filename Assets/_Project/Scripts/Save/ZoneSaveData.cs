// 전체 구역 세이브 데이터
// -> see docs/systems/farm-expansion-architecture.md 섹션 9.1
// PATTERN-005: JSON 필드 1개(zones) = C# 필드 1개 — 일치
using System;

namespace SeedMind.Save
{
    [Serializable]
    public class ZoneSaveData
    {
        public ZoneEntrySaveData[] zones;
    }
}
