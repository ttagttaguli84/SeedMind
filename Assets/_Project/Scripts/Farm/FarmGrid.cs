using UnityEngine;

namespace SeedMind.Farm
{
    public class FarmGrid : MonoBehaviour
    {
        public int gridWidth = 8;  // -> see docs/mcp/farming-tasks.md section 1.1
        public int gridHeight = 8; // -> see docs/mcp/farming-tasks.md section 1.1
        private FarmTile[,] _tiles;

        private void Awake()
        {
            RebuildTileMap();
        }

        public void RebuildTileMap()
        {
            _tiles = new FarmTile[gridWidth, gridHeight];
            var allTiles = GetComponentsInChildren<FarmTile>();
            foreach (var tile in allTiles)
            {
                if (tile.gridX >= 0 && tile.gridX < gridWidth &&
                    tile.gridY >= 0 && tile.gridY < gridHeight)
                    _tiles[tile.gridX, tile.gridY] = tile;
            }
        }

        public FarmTile GetTile(int x, int y)
        {
            if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return null;
            if (_tiles == null) RebuildTileMap();
            return _tiles[x, y];
        }
    }
}
