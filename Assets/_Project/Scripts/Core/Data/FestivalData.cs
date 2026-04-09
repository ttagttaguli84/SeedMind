// S-10: 축제 정의 ScriptableObject
// -> see docs/systems/time-season-architecture.md 섹션 2.5 (FestivalData 박스)
// canonical 축제 목록: docs/systems/time-season.md 섹션 4.2
using UnityEngine;
using SeedMind.Farm.Data;

namespace SeedMind.Core
{
    [CreateAssetMenu(fileName = "NewFestivalData", menuName = "SeedMind/FestivalData")]
    public class FestivalData : ScriptableObject
    {
        public string festivalName;
        public string festivalId;
        public Season season;
        public int day;
        public string description;
        public float shopDiscountRate;          // 0 ~ 0.5
        public CropData specialCropBonus;       // 축제 보너스 작물 (nullable)
        public float bonusMultiplier = 1.0f;
        public string dialogueKey;
    }
}
