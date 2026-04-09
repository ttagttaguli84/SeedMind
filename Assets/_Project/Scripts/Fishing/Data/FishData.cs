// FishData ScriptableObject — 어종 정의 데이터
// -> see docs/systems/fishing-architecture.md 섹션 3
using UnityEngine;
using SeedMind.Core;
using SeedMind.Farm.Data;

namespace SeedMind.Fishing.Data
{
    [CreateAssetMenu(fileName = "NewFishData", menuName = "SeedMind/Fishing/FishData")]
    public class FishData : GameDataSO, IInventoryItem
    {
        [Header("낚시 식별")]
        public string fishId;   // ex) "fish_crucian_carp" (dataId와 동일하게 설정)

        [Header("희귀도 & 가격")]
        public FishRarity rarity;
        public int basePrice;   // -> see docs/systems/fishing-system.md 섹션 4.2

        [Header("등장 조건")]
        // SeasonFlag 비트마스크 — 복수 계절 조합 가능
        // -> see docs/systems/fishing-system.md 섹션 4.2 for canonical 값
        public SeasonFlag seasonAvailability;

        // DayPhase 별 가중치 [Dawn, Morning, Afternoon, Evening, Night]
        // -> see docs/systems/fishing-system.md 섹션 5.1 for canonical 값
        [Tooltip("시간대 가중치 [Dawn, Morning, Afternoon, Evening, Night] (합계 1.0)")]
        public float[] timeWeights = new float[5] { 0.2f, 0.2f, 0.2f, 0.2f, 0.2f };

        // 날씨 조건 비트마스크 — 해당 날씨일 때 출현 가중치 보정
        // -> see docs/systems/fishing-system.md 섹션 5.2 for canonical 값
        public WeatherFlag weatherBonus;

        [Header("미니게임 파라미터")]
        // -> see docs/systems/fishing-system.md 섹션 3.3 for canonical 값
        [Range(0f, 1f)] public float minigameDifficulty;
        public float targetZoneWidthMul;    // 목표 구역 너비 배수
        public float moveSpeed;             // 목표 구역 이동 속도

        [Header("인벤토리")]
        public int maxStackSize = 99;
        public int expReward;               // 낚시 시 획득 XP -> see docs/balance/progression-curve.md

        // --- IInventoryItem 구현 ---
        public string ItemId   => dataId;
        public string ItemName => displayName;
        public SeedMind.ItemType ItemType  => SeedMind.ItemType.Fish;
        public Sprite Icon     => icon;
        public int MaxStackSize => maxStackSize;
        public bool Sellable   => true;
    }
}
