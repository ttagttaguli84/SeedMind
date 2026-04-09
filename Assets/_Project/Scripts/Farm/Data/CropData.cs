using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Farm.Data
{
    /// <summary>
    /// 작물 데이터 ScriptableObject.
    /// GameDataSO 상속, IInventoryItem 구현.
    /// -> see docs/mcp/crop-content-tasks.md Part I 섹션 1
    /// -> see docs/systems/farming-architecture.md 섹션 4.1
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Crop", menuName = "SeedMind/Farm/CropData")]
    public class CropData : GameDataSO, SeedMind.IInventoryItem
    {
        // dataId, displayName, icon 은 GameDataSO에서 상속

        [Header("작물 식별")]
        public string cropId;                    // 코드용 고유 식별자 (예: "crop_potato")
        public string cropName;                  // 표시 이름 (예: "감자") — displayName과 동일
        public CropCategory cropCategory;        // 작물 분류

        [Header("계절")]
        public SeasonFlag allowedSeasons;        // 재배 가능 계절 비트마스크
                                                 // -> see docs/systems/crop-growth.md 섹션 3.1

        [Header("성장")]
        public int growthDays;                   // 총 성장 일수
                                                 // -> see docs/design.md 섹션 4.2
        public int growthStageCount = 4;         // 시각적 단계 수 (기본 4)

        [Header("경제")]
        public int seedPrice;                    // 씨앗 구매가
                                                 // -> see docs/design.md 섹션 4.2
        public int sellPrice;                    // 수확물 판매가
                                                 // -> see docs/design.md 섹션 4.2

        [Header("수확")]
        public int baseYield = 1;                // 기본 수확량
        public float qualityChance = 0.1f;       // 고품질 확률

        [Header("반복 수확")]
        public bool isRepeating;                 // 반복 수확 가능 여부 (딸기, 표고버섯 등)
        public int regrowDays;                   // 재수확까지 일수 (isRepeating=true일 때만 유효)

        [Header("거대 작물")]
        public float giantCropChance;            // 거대 작물 변이 확률
                                                 // -> see docs/systems/crop-growth.md 섹션 5.1
        public GameObject giantCropPrefab;       // 거대 작물 프리팹 참조

        [Header("해금")]
        public int unlockLevel;                  // 해금 레벨
                                                 // -> see docs/design.md 섹션 4.2

        [Header("비주얼")]
        public GameObject[] growthStagePrefabs;  // 단계별 3D 모델 (길이 = growthStageCount)
        public Material soilMaterial;            // 심어졌을 때 토양 머티리얼 오버라이드
        public GameObject harvestParticle;       // 수확 시 파티클 프리팹

        [Header("온실")]
        public bool requiresGreenhouse;          // 온실 필수 여부 (겨울 전용 작물)

        [Header("설명")]
        [TextArea(2, 4)]
        public string description;               // UI 표시용 작물 설명

        // ── IInventoryItem 구현 ─────────────────────────────
        // -> see docs/systems/inventory-architecture.md 섹션 3.1
        public string ItemId => dataId;
        public string ItemName => displayName;
        public SeedMind.ItemType ItemType => SeedMind.ItemType.Crop;
        public Sprite Icon => icon;
        public int MaxStackSize => 99;
        public bool Sellable => true;

        public override bool Validate(out string errorMessage)
        {
            if (!base.Validate(out errorMessage)) return false;
            if (growthDays <= 0)
            {
                errorMessage = $"{name}: growthDays가 0 이하입니다.";
                return false;
            }
            return true;
        }
    }
}
