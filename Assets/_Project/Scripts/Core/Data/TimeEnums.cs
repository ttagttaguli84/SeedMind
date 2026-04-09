// 시간/계절 시스템 Enum 정의
// -> see docs/systems/time-season-architecture.md 섹션 1
namespace SeedMind.Core
{
    /// <summary>계절 (Spring=0, Summer=1, Autumn=2, Winter=3)</summary>
    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }

    /// <summary>시간대 (새벽/아침/낮/저녁/밤)</summary>
    public enum DayPhase
    {
        Dawn,       // 06:00 ~ 08:00 미만
        Morning,    // 08:00 ~ 11:59
        Afternoon,  // 12:00 ~ 16:59
        Evening,    // 17:00 ~ 19:59
        Night       // 20:00 ~ 23:59
    }

    /// <summary>날씨 (7종) — canonical: docs/systems/time-season.md 섹션 3.1</summary>
    public enum WeatherType
    {
        Clear,       // 맑음
        Cloudy,      // 흐림
        Rain,        // 비
        HeavyRain,   // 폭우
        Storm,       // 폭풍
        Snow,        // 눈
        Blizzard     // 폭설
    }
}
