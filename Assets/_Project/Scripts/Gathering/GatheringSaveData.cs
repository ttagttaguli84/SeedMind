// 채집 시스템 세이브 데이터 — GameSaveData.gathering 필드에 직렬화
// -> see docs/systems/gathering-architecture.md 섹션 7
using System.Collections.Generic;

namespace SeedMind.Gathering
{
    [System.Serializable]
    public class GatheringSaveData
    {
        // 통계
        public int totalGathered;
        public Dictionary<string, int> gatheredByItemId = new Dictionary<string, int>();
        public Dictionary<string, int> gatheredByZone   = new Dictionary<string, int>();
        public int rareItemsFound;

        // 포인트 상태
        public List<GatheringPointStateSaveData> pointStates = new List<GatheringPointStateSaveData>();

        // 숙련도 (ARC-031)
        // -> see docs/systems/gathering-architecture.md 섹션 4.2
        public int gatheringProficiencyXP;
        public int gatheringProficiencyLevel;
    }
}
