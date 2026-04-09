using System.Collections.Generic;
using UnityEngine;
using SeedMind.Farm;
using SeedMind.Farm.Data;

namespace SeedMind.Building
{
    /// <summary>
    /// 물탱크의 자동 물주기 로직을 처리하는 서브시스템.
    /// -> see docs/systems/facilities-architecture.md 섹션 4.1
    /// </summary>
    public class WaterTankSystem
    {
        private readonly List<BuildingInstance> _waterTanks = new List<BuildingInstance>();
        private FarmGrid _farmGrid;

        public void SetFarmGrid(FarmGrid farmGrid) => _farmGrid = farmGrid;

        public void RegisterTank(BuildingInstance tank)
        {
            if (!_waterTanks.Contains(tank))
                _waterTanks.Add(tank);
        }

        public void UnregisterTank(BuildingInstance tank) => _waterTanks.Remove(tank);

        /// <summary>
        /// 매일 아침 호출. 모든 물탱크의 범위 내 경작 타일에 자동 물주기.
        /// </summary>
        public void ProcessDailyWatering()
        {
            if (_farmGrid == null) return;
            foreach (var tank in _waterTanks)
            {
                if (!tank.IsOperational) continue;
                int actualRadius = tank.Data.effectRadius + tank.UpgradeLevel;
                float centerX = tank.GridX + tank.Data.tileSize.x * 0.5f;
                float centerY = tank.GridY + tank.Data.tileSize.y * 0.5f;

                for (int x = 0; x < _farmGrid.gridWidth; x++)
                {
                    for (int y = 0; y < _farmGrid.gridHeight; y++)
                    {
                        float dist = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);
                        if (dist > actualRadius) continue;
                        var tile = _farmGrid.GetTile(x, y);
                        if (tile == null) continue;
                        if (tile.State == TileState.Planted || tile.State == TileState.Dry)
                            tile.SetState(TileState.Watered);
                    }
                }
            }
        }

        public bool IsTileCoveredByTank(int tileX, int tileY)
        {
            foreach (var tank in _waterTanks)
            {
                if (!tank.IsOperational) continue;
                int actualRadius = tank.Data.effectRadius + tank.UpgradeLevel;
                float centerX = tank.GridX + tank.Data.tileSize.x * 0.5f;
                float centerY = tank.GridY + tank.Data.tileSize.y * 0.5f;
                if (Mathf.Abs(tileX - centerX) + Mathf.Abs(tileY - centerY) <= actualRadius)
                    return true;
            }
            return false;
        }

        public List<Vector2Int> GetCoveredTiles(BuildingInstance tank)
        {
            var result = new List<Vector2Int>();
            if (_farmGrid == null) return result;
            int actualRadius = tank.Data.effectRadius + tank.UpgradeLevel;
            float centerX = tank.GridX + tank.Data.tileSize.x * 0.5f;
            float centerY = tank.GridY + tank.Data.tileSize.y * 0.5f;
            for (int x = 0; x < _farmGrid.gridWidth; x++)
                for (int y = 0; y < _farmGrid.gridHeight; y++)
                    if (Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY) <= actualRadius)
                        result.Add(new Vector2Int(x, y));
            return result;
        }
    }
}
