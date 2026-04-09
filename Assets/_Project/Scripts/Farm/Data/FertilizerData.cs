using UnityEngine;

namespace SeedMind.Farm.Data
{
    [CreateAssetMenu(fileName = "SO_Fertilizer", menuName = "SeedMind/Farm/FertilizerData")]
    public class FertilizerData : ScriptableObject
    {
        public string fertilizerName;
        public string fertilizerId;
        public int price;
        public float qualityMultiplier;
        public float growthSpeedMultiplier;
    }
}
