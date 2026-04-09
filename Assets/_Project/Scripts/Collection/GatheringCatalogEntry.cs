using System;

namespace SeedMind.Collection
{
    [Serializable]
    public class GatheringCatalogEntry
    {
        public string itemId;
        public bool isDiscovered;
        public int totalGathered;
        public int bestQuality;       // 0=Normal, 1=Silver, 2=Gold, 3=Iridium
        public int firstGatheredDay;  // -1 = undiscovered
        public int firstGatheredSeason;
        public int firstGatheredYear;

        [NonSerialized]
        public bool isNewBestQuality;

        public GatheringCatalogEntry(string id)
        {
            itemId = id;
            isDiscovered = false;
            totalGathered = 0;
            bestQuality = 0;
            firstGatheredDay = -1;
            firstGatheredSeason = -1;
            firstGatheredYear = -1;
            isNewBestQuality = false;
        }
    }
}
