using System;
using System.Collections.Generic;

namespace SeedMind.Collection
{
    [Serializable]
    public class GatheringCatalogSaveData
    {
        public List<GatheringCatalogEntry> entries;
        public int discoveredCount;
    }
}
