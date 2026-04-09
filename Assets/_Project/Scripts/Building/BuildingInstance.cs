using System.Collections.Generic;
using UnityEngine;
using SeedMind.Building.Data;

namespace SeedMind.Building
{
    /// <summary>
    /// 건설된 시설의 런타임 인스턴스. MonoBehaviour가 아닌 순수 C# 클래스.
    /// BuildingSaveData와 1:1 대응한다.
    /// -> see docs/systems/facilities-architecture.md 섹션 3.2
    /// </summary>
    public class BuildingInstance
    {
        public BuildingData Data { get; private set; }
        public int GridX { get; private set; }
        public int GridY { get; private set; }
        public bool IsOperational { get; set; }
        public int UpgradeLevel { get; set; }
        public float BuildProgress { get; set; }
        public GameObject SceneObject { get; set; }

        public BuildingInstance(BuildingData data, int gridX, int gridY)
        {
            Data = data;
            GridX = gridX;
            GridY = gridY;
            IsOperational = false;
            UpgradeLevel = 0;
            BuildProgress = 0f;
        }

        /// <summary>
        /// 이 시설이 점유하는 모든 타일 좌표를 반환.
        /// </summary>
        public IEnumerable<Vector2Int> GetOccupiedTiles()
        {
            for (int x = GridX; x < GridX + Data.tileSize.x; x++)
                for (int y = GridY; y < GridY + Data.tileSize.y; y++)
                    yield return new Vector2Int(x, y);
        }
    }
}
