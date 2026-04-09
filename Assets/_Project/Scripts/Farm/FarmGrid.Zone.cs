// FarmGrid partial class — 구역 시스템 지원 메서드
// -> see docs/systems/farm-expansion-architecture.md 섹션 6
using UnityEngine;

namespace SeedMind.Farm
{
    public partial class FarmGrid
    {
        /// <summary>구역 해금 시 해당 타일 목록을 활성화.</summary>
        public void ActivateZoneTiles(Vector2Int[] positions)
        {
            if (positions == null) return;
            foreach (var pos in positions)
            {
                var tile = GetTile(pos.x, pos.y);
                if (tile != null)
                    tile.gameObject.SetActive(true);
            }
        }

        /// <summary>구역 잠금 시 해당 타일 목록을 비활성화.</summary>
        public void DeactivateZoneTiles(Vector2Int[] positions)
        {
            if (positions == null) return;
            foreach (var pos in positions)
            {
                var tile = GetTile(pos.x, pos.y);
                if (tile != null)
                    tile.gameObject.SetActive(false);
            }
        }

        /// <summary>최대 크기로 그리드를 사전 할당 (확장 대비).</summary>
        public void InitializeFullGrid(int maxWidth, int maxHeight)
        {
            gridWidth = maxWidth;
            gridHeight = maxHeight;
            RebuildTileMap();
        }
    }
}
