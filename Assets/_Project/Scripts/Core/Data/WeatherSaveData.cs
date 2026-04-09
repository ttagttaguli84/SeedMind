// S-08: 날씨 시스템 저장 데이터
// -> see docs/systems/time-season-architecture.md 섹션 7.2
namespace SeedMind.Core
{
    [System.Serializable]
    public class WeatherSaveData
    {
        public int weatherSeed;
        public int currentWeatherIndex;   // (int)WeatherType
        public int tomorrowWeatherIndex;  // (int)WeatherType
        public int consecutiveSameWeatherDays;
        public int totalElapsedDays;
    }
}
