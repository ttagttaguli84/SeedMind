using SeedMind.Economy;

namespace SeedMind.Livestock
{
    [System.Serializable]
    public struct AnimalProductInfo
    {
        public string productItemId;
        public CropQuality quality;
        public int estimatedAmount;
    }
}
