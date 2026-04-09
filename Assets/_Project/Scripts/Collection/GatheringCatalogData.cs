using UnityEngine;
using SeedMind.Gathering;

namespace SeedMind.Collection
{
    [CreateAssetMenu(fileName = "NewGatheringCatalogData", menuName = "SeedMind/GatheringCatalogData")]
    public class GatheringCatalogData : ScriptableObject
    {
        [Header("식별")]
        public string itemId;

        [Header("도감 힌트")]
        [TextArea(2, 4)]
        public string hintLocked;

        [TextArea(2, 4)]
        public string descriptionUnlocked;

        [Header("희귀도")]
        public GatheringRarity rarity;

        [Header("초회 채집 보상")]
        public int firstDiscoverGold;
        public int firstDiscoverXP;

        [Header("도감 표시")]
        public Sprite catalogIcon;
        public int sortOrder;
    }
}
