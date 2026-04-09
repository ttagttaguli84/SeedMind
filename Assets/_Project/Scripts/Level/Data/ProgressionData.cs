// ProgressionData ScriptableObject — 진행 시스템 설정 데이터
// -> see docs/systems/progression-architecture.md 섹션 2.1
using UnityEngine;

namespace SeedMind.Level.Data
{
    [CreateAssetMenu(fileName = "SO_ProgressionData", menuName = "SeedMind/ProgressionData")]
    public class ProgressionData : ScriptableObject
    {
        [Header("레벨 설정")]
        public int maxLevel;                        // -> see docs/pipeline/data-pipeline.md 섹션 2.6
        public int[] expPerLevel;                   // -> see docs/balance/progression-curve.md 섹션 2.4.1

        [Header("경험치 획득 설정")]
        public int harvestExpBase;                  // -> see docs/pipeline/data-pipeline.md 섹션 2.6
        public float harvestExpPerGrowthDay;        // -> see docs/pipeline/data-pipeline.md 섹션 2.6
        public float[] qualityExpBonus;             // -> see docs/pipeline/data-pipeline.md 섹션 2.6

        [Header("비수확 XP 소스")]
        public int buildingConstructExp;            // -> see docs/balance/progression-curve.md
        public int toolUseExp;                      // -> see docs/balance/progression-curve.md
        public int facilityProcessExp;              // -> see docs/balance/progression-curve.md
        public int toolUpgradeExp;                  // -> see docs/balance/tool-upgrade-xp.md
        public int animalCareExp;                   // -> see docs/balance/progression-curve.md
        public int animalHarvestBaseExp;            // -> see docs/systems/livestock-architecture.md 섹션 7.2

        [Header("해금 테이블")]
        public LevelUnlockEntry[] unlockTable;      // 레벨별 해금 목록

        [Header("마일스톤")]
        public MilestoneData[] milestones;          // 전체 마일스톤 목록
    }

    [System.Serializable]
    public class LevelUnlockEntry
    {
        public int level;
        public UnlockItemEntry[] items;
    }

    [System.Serializable]
    public class UnlockItemEntry
    {
        public UnlockType type;
        public string itemId;
        public string displayName;
    }
}
