using UnityEngine;
using UnityEngine.InputSystem;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Farm;
using SeedMind.Farm.Data;
using SeedMind.Player.Data;

namespace SeedMind.Player
{
    public class ToolSystem : MonoBehaviour
    {
        public ToolData[] tools;
        public int currentToolIndex = 0;

        [Header("씨앗 기본 작물 ID (SeedBag 사용 시)")]
        public string defaultSeedCropId = "crop_potato"; // 씬에서 설정 가능

        public ToolData CurrentTool =>
            (tools != null && tools.Length > 0) ? tools[currentToolIndex] : null;

        public void SelectTool(int index)
        {
            if (tools == null || index < 0 || index >= tools.Length) return;
            currentToolIndex = index;
        }

        /// <summary>
        /// 지정 타일에 현재 도구를 사용한다.
        /// 장애물이 있으면 FarmZoneManager.ClearObstacle로 우선 분기.
        /// -> see docs/systems/farm-expansion-architecture.md 섹션 8
        /// </summary>
        public bool TryUseToolAt(Vector2Int gridPos)
        {
            var tool = CurrentTool;
            if (tool == null) return false;

            // [장애물 체크] 장애물이 있으면 개간 우선
            var fzm = FarmZoneManager.Instance;
            if (fzm != null)
            {
                var obs = fzm.GetObstacleAt(gridPos);
                if (obs != null && !obs.isCleared)
                {
                    fzm.ClearObstacle(gridPos, tool.toolType, tool.tier);
                    return true;
                }
            }

            // [농사 액션]
            var grid = FarmGrid.FindFirstObjectByType(typeof(FarmGrid)) as FarmGrid;
            if (grid == null) return false;
            var tile = grid.GetTile(gridPos.x, gridPos.y);
            if (tile == null) return false;

            return ApplyToolToTile(tool, tile);
        }

        private bool ApplyToolToTile(ToolData tool, FarmTile tile)
        {
            var state = tile.State;
            switch (tool.toolType)
            {
                case ToolType.Hoe:
                    if (state == TileState.Empty)
                    {
                        tile.SetState(TileState.Tilled);
                        return true;
                    }
                    break;

                case ToolType.SeedBag:
                    if (state == TileState.Tilled)
                    {
                        PlantCrop(tile);
                        return true;
                    }
                    break;

                case ToolType.WateringCan:
                    if (state == TileState.Planted || state == TileState.Dry)
                    {
                        tile.SetState(TileState.Watered);
                        FarmEvents.OnTileWatered?.Invoke(tile);
                        return true;
                    }
                    break;

                case ToolType.Sickle:
                case ToolType.Hand:
                    if (state == TileState.Harvestable)
                    {
                        HarvestCrop(tile);
                        return true;
                    }
                    break;
            }
            return false;
        }

        private void PlantCrop(FarmTile tile)
        {
            // DataRegistry에서 작물 데이터 조회
            var cropData = DataRegistry.Instance != null
                ? DataRegistry.Instance.Get<Farm.Data.CropData>(defaultSeedCropId)
                : null;

            // CropInstance 생성
            var cropGO = new GameObject($"Crop_{tile.gridX}_{tile.gridY}");
            cropGO.transform.SetParent(tile.transform);
            cropGO.transform.localPosition = Vector3.zero;
            var cropInst = cropGO.AddComponent<CropInstance>();
            if (cropData != null) cropInst.Initialize(cropData);
            tile.cropInstance = cropInst;

            tile.SetState(TileState.Planted);
            Debug.Log($"[ToolSystem] ({tile.gridX},{tile.gridY}) 파종 완료: {defaultSeedCropId}");
        }

        private void HarvestCrop(FarmTile tile)
        {
            var cropInst = tile.cropInstance;
            string cropId = cropInst?.cropData?.cropId ?? defaultSeedCropId;
            int salePrice = cropInst?.cropData?.sellPrice ?? 50;

            // 인벤토리에 추가
            var inv = InventoryManager.Instance;
            if (inv != null)
                inv.AddItem(cropId, 1);

            // 골드 지급
            var econ = EconomyManager.Instance;
            if (econ != null)
                econ.AddGold(salePrice);

            // 이벤트 발행
            FarmEvents.OnCropHarvested?.Invoke(tile);

            // 타일 초기화
            if (cropInst != null) Destroy(cropInst.gameObject);
            tile.cropInstance = null;
            tile.SetState(TileState.Empty);

            Debug.Log($"[ToolSystem] ({tile.gridX},{tile.gridY}) 수확 완료: {cropId} +{salePrice}G");
        }

        private void Update()
        {
            if (tools == null || tools.Length == 0) return;
            var mouse = Mouse.current;
            if (mouse == null) return;
            float scroll = mouse.scroll.ReadValue().y;
            if (scroll > 0f) SelectTool((currentToolIndex + 1) % tools.Length);
            else if (scroll < 0f) SelectTool((currentToolIndex - 1 + tools.Length) % tools.Length);
        }
    }
}
