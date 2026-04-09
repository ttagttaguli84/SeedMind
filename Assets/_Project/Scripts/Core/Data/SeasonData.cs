// S-04: 계절별 환경 데이터 ScriptableObject
// -> see docs/systems/time-season-architecture.md 섹션 2.1 (SeasonData 박스)
using UnityEngine;

namespace SeedMind.Core
{
    [CreateAssetMenu(fileName = "NewSeasonData", menuName = "SeedMind/SeasonData")]
    public class SeasonData : ScriptableObject
    {
        [Header("기본 정보")]
        public Season season;
        public string displayName;

        [Header("환경 조명")]
        public Color sunColor = Color.white;
        public float sunIntensity = 1.0f;
        public Color ambientColor = Color.grey;
        public Color fogColor = Color.grey;
        public float fogDensity = 0.01f;

        [Header("시간대별 오버라이드 (5개: Dawn/Morning/Afternoon/Evening/Night)")]
        public DayPhaseVisual[] phaseOverrides = new DayPhaseVisual[5];

        [Header("게임플레이 배수")]
        public float growthSpeedMultiplier = 1.0f;  // -> see docs/systems/time-season.md 섹션 2.2
        public float shopPriceMultiplier = 1.0f;

        [Header("비주얼 오버라이드")]
        public Color terrainTintColor = Color.white;
        public GameObject treePrefabOverride;
        public GameObject particleEffect;
    }
}
