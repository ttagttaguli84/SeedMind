// S-02: 시간 시스템 저장 데이터
// -> see docs/systems/time-season-architecture.md 섹션 7.1
namespace SeedMind.Core
{
    [System.Serializable]
    public class TimeSaveData
    {
        public int year;
        public int seasonIndex;   // (int)Season
        public int day;
        public float hour;
        public int dayPhaseIndex; // (int)DayPhase
        public float timeScale;
    }
}
