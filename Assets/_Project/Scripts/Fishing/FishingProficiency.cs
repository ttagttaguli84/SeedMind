// 낚시 숙련도 — 레벨 1~10, XP 기반 보너스 (ARC-029)
// -> see docs/systems/fishing-architecture.md 섹션 4A
using UnityEngine;

namespace SeedMind.Fishing
{
    /// <summary>
    /// 낚시 전용 숙련도 시스템.
    /// FishingManager가 보유하며, 세이브 데이터로 직렬화된다.
    /// </summary>
    public class FishingProficiency
    {
        public const int MaxLevel = 10;

        // XP 임계값 — 레벨별 누적 XP (index 0 = Lv1→Lv2)
        // -> see docs/systems/fishing-architecture.md 섹션 4A for canonical 값
        private static readonly int[] XpThresholds =
        {
            50, 120, 210, 330, 480, 660, 880, 1140, 1440, int.MaxValue
        };

        public int Level  { get; private set; } = 1;
        public int TotalXP { get; private set; } = 0;

        /// <summary>XP 추가. 레벨업 시 true 반환.</summary>
        public bool AddXP(int amount)
        {
            if (Level >= MaxLevel) return false;
            TotalXP += amount;
            bool leveledUp = false;
            while (Level < MaxLevel && TotalXP >= XpThresholds[Level - 1])
                leveledUp = true;

            // 레벨업 처리 (임계값 초과 시 레벨 반영)
            int newLevel = Level;
            int accumulated = 0;
            for (int i = 0; i < XpThresholds.Length; i++)
            {
                accumulated += (i == 0 ? XpThresholds[0] : XpThresholds[i] - XpThresholds[i - 1]);
                if (TotalXP >= XpThresholds[i])
                    newLevel = Mathf.Min(i + 2, MaxLevel);
                else
                    break;
            }

            // 단순 레벨 계산
            newLevel = 1;
            for (int i = 0; i < XpThresholds.Length && i < MaxLevel - 1; i++)
            {
                if (TotalXP >= XpThresholds[i]) newLevel = i + 2;
                else break;
            }

            if (newLevel != Level)
            {
                Level = Mathf.Min(newLevel, MaxLevel);
                return true;
            }
            return false;
        }

        // --- 보너스 보정값 ---

        /// <summary>입질 대기 시간 단축 비율 (0.0 ~ 0.3)</summary>
        public float BiteDelayReduction => Mathf.Lerp(0f, 0.3f, (Level - 1) / (float)(MaxLevel - 1));

        /// <summary>희귀 어종 출현 확률 보정 (0.0 ~ 0.5)</summary>
        public float RarityBoost => Mathf.Lerp(0f, 0.5f, (Level - 1) / (float)(MaxLevel - 1));

        /// <summary>동시 획득 확률 (Lv 8 이상에서 5%/레벨)</summary>
        public float DoubleCatchChance => Level >= 8 ? (Level - 7) * 0.05f : 0f;

        /// <summary>미니게임 성공률 보정 (0.0 ~ 0.2)</summary>
        public float MinigameBonus => Mathf.Lerp(0f, 0.2f, (Level - 1) / (float)(MaxLevel - 1));

        // --- 직렬화 ---

        public void LoadFromSave(int savedXP, int savedLevel)
        {
            TotalXP = savedXP;
            Level   = Mathf.Clamp(savedLevel, 1, MaxLevel);
        }
    }
}
