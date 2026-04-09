using System.Collections.Generic;
using UnityEngine;
using SeedMind.Farm;

namespace SeedMind.Building
{
    /// <summary>
    /// 온실의 내부 경작 타일 관리 및 계절 보정 Override를 처리하는 서브시스템.
    /// ISeasonOverrideProvider를 구현하여 GrowthSystem에 주입된다.
    /// -> see docs/systems/facilities-architecture.md 섹션 5.1
    /// </summary>
    public class GreenhouseSystem : ISeasonOverrideProvider
    {
        private readonly List<BuildingInstance> _greenhouses = new List<BuildingInstance>();
        private FarmGrid _farmGrid;
        private readonly HashSet<Vector2Int> _greenhouseTiles = new HashSet<Vector2Int>();

        public void SetFarmGrid(FarmGrid farmGrid) => _farmGrid = farmGrid;

        public void RegisterGreenhouse(BuildingInstance greenhouse)
        {
            if (_greenhouses.Contains(greenhouse)) return;
            _greenhouses.Add(greenhouse);
            foreach (var tile in GetInteriorTiles(greenhouse))
                _greenhouseTiles.Add(tile);
        }

        public void UnregisterGreenhouse(BuildingInstance greenhouse)
        {
            _greenhouses.Remove(greenhouse);
            foreach (var tile in GetInteriorTiles(greenhouse))
                _greenhouseTiles.Remove(tile);
        }

        /// <summary>
        /// 특정 타일이 온실 내부인지 확인. GrowthSystem이 계절 제약 검사 시 사용.
        /// </summary>
        public bool IsSeasonOverridden(int tileX, int tileY)
        {
            return _greenhouseTiles.Contains(new Vector2Int(tileX, tileY));
        }

        /// <summary>
        /// 온실 내부의 경작 가능 타일 좌표 목록 반환.
        /// 테두리(1타일 벽)를 제외한 내부 영역.
        /// -> see docs/content/facilities.md 섹션 4.2 for 레벨별 내부 타일 수
        /// </summary>
        public List<Vector2Int> GetInteriorTiles(BuildingInstance greenhouse)
        {
            var result = new List<Vector2Int>();
            int xStart = greenhouse.GridX + 1;
            int yStart = greenhouse.GridY + 1;
            int xEnd = greenhouse.GridX + greenhouse.Data.tileSize.x - 2;
            int yEnd = greenhouse.GridY + greenhouse.Data.tileSize.y - 2;
            for (int x = xStart; x <= xEnd; x++)
                for (int y = yStart; y <= yEnd; y++)
                    result.Add(new Vector2Int(x, y));
            return result;
        }
    }
}
