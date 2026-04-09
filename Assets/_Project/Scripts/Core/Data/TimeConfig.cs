// S-01: 시간 시스템 설정 ScriptableObject
// canonical 수치: docs/systems/time-season-architecture.md 섹션 2.2
using UnityEngine;

namespace SeedMind.Core
{
    [CreateAssetMenu(fileName = "TimeConfig", menuName = "SeedMind/TimeConfig")]
    public class TimeConfig : ScriptableObject
    {
        [Header("시간 진행")]
        public float secondsPerGameHour = 33.33f;  // 실시간 33초 = 게임 내 1시간
        public int dayStartHour = 6;                // 하루 시작 시각
        public int dayEndHour = 24;                 // 하루 종료 시각

        [Header("달력")]
        public int daysPerSeason = 28;              // 1계절 = 28일
        public int seasonsPerYear = 4;              // 1년 = 4계절

        [Header("배속")]
        public float defaultTimeScale = 1.0f;       // 기본 배속
        public float maxTimeScale = 3.0f;           // 최대 배속

        // 파생 값 (읽기 전용)
        public int DaysPerYear => daysPerSeason * seasonsPerYear;
        public float RealSecondsPerDay => secondsPerGameHour * (dayEndHour - dayStartHour);
    }
}
