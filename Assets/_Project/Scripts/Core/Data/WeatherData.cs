// S-07: 계절별 날씨 확률 데이터 ScriptableObject
// -> see docs/systems/time-season-architecture.md 섹션 2.4
using UnityEngine;

namespace SeedMind.Core
{
    [CreateAssetMenu(fileName = "NewWeatherData", menuName = "SeedMind/WeatherData")]
    public class WeatherData : ScriptableObject
    {
        public Season season;

        // 확률 수치 canonical: docs/systems/time-season.md 섹션 3.2
        [Header("날씨 확률 (합계 = 1.0) — canonical: docs/systems/time-season.md 섹션 3.2")]
        public float clearChance;
        public float cloudyChance;
        public float rainChance;
        public float heavyRainChance;
        public float stormChance;
        public float snowChance;
        public float blizzardChance;

        // 연속 보정 — canonical: 이 섹션 (time-season-architecture.md 섹션 2.4)
        [Header("연속 날씨 보정")]
        public int maxConsecutiveSameWeatherDays = 3;
        public int maxConsecutiveExtremeWeatherDays = 2;
        public float consecutivePenalty = 0.5f;

        // 날씨 효과 — canonical: 이 섹션 (time-season-architecture.md 섹션 2.4)
        [Header("날씨 효과")]
        public float rainGrowthBonus = 0.0f;
        public float stormCropDamageChance = 0.1f;
        public float blizzardWitherChance = 0.05f;
    }
}
