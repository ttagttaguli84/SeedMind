using UnityEngine;
using SeedMind.Core;
using SeedMind.Farm.Data;

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

        private void OnEnable()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.RegisterOnDayChanged(20, OnDayChanged);
        }

        private void OnDisable()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.UnregisterOnDayChanged(OnDayChanged);
        }

        private void OnDayChanged(int day) => AdvanceDay();

        /// <summary>
        /// 모든 심어진/물 준 타일의 작물을 하루 성장시킨다.
        /// 완전 성장한 작물은 해당 타일을 Harvestable 상태로 전환.
        /// Watered 타일은 Planted로 리셋(물 지속 효과 없음).
        /// </summary>
        public void AdvanceDay()
        {
            if (farmGrid == null) farmGrid = FindObjectOfType<FarmGrid>();
            if (farmGrid == null) return;

            for (int x = 0; x < farmGrid.gridWidth; x++)
            {
                for (int y = 0; y < farmGrid.gridHeight; y++)
                {
                    var tile = farmGrid.GetTile(x, y);
                    if (tile == null || tile.cropInstance == null) continue;

                    var state = tile.State;
                    if (state != TileState.Planted && state != TileState.Watered) continue;

                    bool fullyGrown = tile.cropInstance.AdvanceDay();

                    if (fullyGrown)
                    {
                        tile.SetState(TileState.Harvestable);
                        Debug.Log($"[GrowthSystem] ({x},{y}) 수확 가능! 작물: {tile.cropInstance.cropData?.cropId}");
                    }
                    else if (state == TileState.Watered)
                    {
                        // 물 효과는 하루만 유지 — 다음 날 Planted(건조)로 복귀
                        tile.SetState(TileState.Planted);
                    }
                }
            }
        }
    }
}
