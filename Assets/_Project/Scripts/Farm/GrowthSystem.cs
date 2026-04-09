using UnityEngine;

namespace SeedMind.Farm
{
    public class GrowthSystem : MonoBehaviour
    {
        public FarmGrid farmGrid;

        private ISeasonOverrideProvider _seasonOverrideProvider;

        public void SetSeasonOverrideProvider(ISeasonOverrideProvider provider)
        {
            _seasonOverrideProvider = provider;
        }

        public void AdvanceDay()
        {
            if (farmGrid == null) farmGrid = FindObjectOfType<FarmGrid>();
            if (farmGrid == null) return;

            for (int x = 0; x < farmGrid.gridWidth; x++)
            {
                for (int y = 0; y < farmGrid.gridHeight; y++)
                {
                    var tile = farmGrid.GetTile(x, y);
                    if (tile != null && tile.cropInstance != null)
                        tile.cropInstance.AdvanceDay();
                }
            }
        }
    }
}
