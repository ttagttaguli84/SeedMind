// 채집 포인트 세이브 데이터 (개별 포인트 상태)
// -> see docs/systems/gathering-architecture.md 섹션 7
namespace SeedMind.Gathering
{
    [System.Serializable]
    public class GatheringPointStateSaveData
    {
        public string pointId;
        public bool isActive;
        public int respawnDaysRemaining;
        public int lastGatheredDay;
        public int lastGatheredSeason;   // Season을 int로 직렬화
    }
}
