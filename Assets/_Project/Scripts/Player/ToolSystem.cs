using UnityEngine;
using UnityEngine.InputSystem;
using SeedMind.Farm;
using SeedMind.Player.Data;

namespace SeedMind.Player
{
    public class ToolSystem : MonoBehaviour
    {
        public ToolData[] tools;
        public int currentToolIndex = 0;

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
                    return true; // 기존 타일 액션 실행하지 않음
                }
            }

            // [기존 타일 액션] 장애물 없으면 기본 도구 로직으로 진행
            return false;
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
