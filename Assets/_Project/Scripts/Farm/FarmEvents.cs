using System;
using SeedMind.Farm.Data;

namespace SeedMind.Farm
{
    public static class FarmEvents
    {
        public static Action<FarmTile, TileState> OnTileStateChanged;
        public static Action<FarmTile> OnCropHarvested;
        public static Action<FarmTile> OnCropWithered;
    }
}
