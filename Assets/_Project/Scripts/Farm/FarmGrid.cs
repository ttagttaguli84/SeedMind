using UnityEngine;

namespace SeedMind.Farm
{
    public partial class FarmGrid : MonoBehaviour
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

        /// <summary>비/폭우/폭풍 시 WeatherSystem이 호출 — 모든 Planted/Dry 타일 자동 물주기.</summary>
        public void WaterAllPlantedTiles()
        {
            if (_tiles == null) RebuildTileMap();
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    var tile = _tiles[x, y];
                    if (tile == null) continue;
                    var state = tile.State;
                    if (state == Farm.Data.TileState.Planted || state == Farm.Data.TileState.Dry)
                    {
                        tile.SetState(Farm.Data.TileState.Watered);
                        FarmEvents.OnTileWatered?.Invoke(tile);
                    }
                }
            }
        }
    }
}
