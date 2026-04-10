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

        /// <summary>
        /// 월드 좌표에서 가장 가까운 타일을 반환. 그리드 범위 밖이면 null.
        /// 타일은 1유닛 간격, FarmGrid transform이 그리드 원점(0,0) 위치 기준.
        /// </summary>
        public FarmTile GetTileAtWorldPos(Vector3 worldPos)
        {
            Vector3 local = transform.InverseTransformPoint(worldPos);
            int x = Mathf.RoundToInt(local.x);
            int y = Mathf.RoundToInt(local.z);
            return GetTile(x, y);
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
