using UnityEngine;
using UnityEngine.InputSystem;
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
