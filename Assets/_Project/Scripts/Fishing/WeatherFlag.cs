// 낚시 날씨 조건 비트마스크 — WeatherType을 플래그로 조합
// -> see docs/systems/fishing-architecture.md 섹션 3 (FishData.weatherBonus)
namespace SeedMind.Fishing
{
    [System.Flags]
    public enum WeatherFlag
    {
        None      = 0,
        Clear     = 1 << 0,   //  1 — 맑음
        Cloudy    = 1 << 1,   //  2 — 흐림
        Rain      = 1 << 2,   //  4 — 비
        HeavyRain = 1 << 3,   //  8 — 폭우
        Storm     = 1 << 4,   // 16 — 폭풍
        Snow      = 1 << 5,   // 32 — 눈
        Blizzard  = 1 << 6,   // 64 — 폭설

        All = Clear | Cloudy | Rain | HeavyRain | Storm | Snow | Blizzard
    }
}
