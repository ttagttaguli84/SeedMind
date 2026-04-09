using UnityEngine;

namespace SeedMind.Farm.Data
{
    [System.Flags]
    public enum Season
    {
        Spring = 1,
        Summer = 2,
        Autumn = 4,
        Winter = 8
    }

    [CreateAssetMenu(fileName = "SO_Crop", menuName = "SeedMind/Farm/CropData")]
    public class CropData : ScriptableObject
    {
        public string cropName;
        public string cropId;
        public int seedPrice;   // → see docs/design.md section 4.2
        public int sellPrice;   // → see docs/design.md section 4.2
        public int growthDays;  // → see docs/design.md section 4.2
        public Season allowedSeasons;
        public int unlockLevel;
        public GameObject[] growthStagePrefabs; // Stage0~3
    }
}
