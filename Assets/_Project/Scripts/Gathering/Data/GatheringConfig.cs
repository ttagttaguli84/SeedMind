// 채집 시스템 밸런스 설정 ScriptableObject
// -> see docs/systems/gathering-architecture.md 섹션 2.3
using UnityEngine;
using SeedMind.Economy;

namespace SeedMind.Gathering
{
    [CreateAssetMenu(fileName = "SO_GatheringConfig_Default", menuName = "SeedMind/Gathering/GatheringConfig")]
    public class GatheringConfig : ScriptableObject
    {
        [Header("기본 채집 설정")]
        public int baseGatherEnergy = 5;          // -> see docs/systems/gathering-system.md 섹션 2
        public float gatherAnimationDuration = 1.5f;
        public int maxActivePointsPerZone = 5;    // -> see docs/systems/gathering-system.md 섹션 2

        [Header("재생성 설정")]
        public int defaultRespawnDays = 3;        // -> see docs/systems/gathering-system.md 섹션 2
        public bool seasonalRefreshOnChange = true; // 계절 전환 시 전체 리프레시

        [Header("품질 판정")]
        // [Normal, Silver, Gold, Iridium] 임계값
        // -> see docs/systems/gathering-system.md 섹션 4.5
        public float[] qualityThresholds = new float[] { 0f, 0.60f, 0.85f, 0.95f };

        [Header("숙련도 설정")]
        // 레벨별 누적 XP 임계값 (index 0 = Lv1→Lv2)
        // -> see docs/systems/gathering-system.md 섹션 4.2
        public int[] proficiencyXPThresholds = new int[]
        {
            50, 120, 210, 330, 480, 660, 880, 1140, 1440, int.MaxValue
        };
        public int proficiencyMaxLevel = 10; // -> see docs/systems/gathering-system.md 섹션 4.2

        // [Common, Uncommon, Rare, Legendary] 채집 숙련도 XP
        // -> see docs/systems/gathering-system.md 섹션 4.3
        public int[] gatherXPByRarity = new int[] { 5, 10, 20, 50 };

        // 레벨별 추가 수량 획득 확률 (index = level-1)
        // -> see docs/systems/gathering-system.md 섹션 4.4
        public float[] bonusQuantityByLevel = new float[]
        {
            0f, 0.05f, 0.08f, 0.12f, 0.15f, 0.18f, 0.22f, 0.26f, 0.30f, 0.35f
        };

        // 레벨별 희귀 아이템 출현 확률 보정
        // -> see docs/systems/gathering-system.md 섹션 4.4
        public float[] rarityBonusByLevel = new float[]
        {
            0f, 0.02f, 0.05f, 0.08f, 0.12f, 0.16f, 0.20f, 0.26f, 0.32f, 0.40f
        };

        // 레벨별 에너지 소모 감소량
        // -> see docs/systems/gathering-system.md 섹션 4.2
        public int[] energyCostReductionByLevel = new int[]
        {
            0, 0, 1, 1, 1, 2, 2, 2, 3, 3
        };

        // 레벨별 최대 품질
        // -> see docs/systems/gathering-system.md 섹션 4.5
        public CropQuality[] maxQualityByLevel = new CropQuality[]
        {
            CropQuality.Normal, CropQuality.Normal,
            CropQuality.Silver, CropQuality.Silver, CropQuality.Silver,
            CropQuality.Gold, CropQuality.Gold, CropQuality.Gold,
            CropQuality.Iridium, CropQuality.Iridium
        };

        // 레벨별 채집 속도 배율
        // -> see docs/systems/gathering-system.md 섹션 4.4
        public float[] gatherSpeedMultiplierByLevel = new float[]
        {
            1.0f, 1.05f, 1.1f, 1.15f, 1.2f, 1.25f, 1.3f, 1.4f, 1.5f, 1.6f
        };
    }
}
