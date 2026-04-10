using UnityEngine;
using UnityEngine.InputSystem;
using SeedMind.Farm;

namespace SeedMind.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5.0f; // -> see docs/mcp/farming-tasks.md section C-1

        private CharacterController _cc;
        private ToolSystem _toolSystem;
        private FarmGrid _farmGrid;
        private bool _prevEPressed;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _toolSystem = GetComponent<ToolSystem>();
        }

        private void Update()
        {
            HandleMovement();
            HandleInteract();
        }

        private void HandleMovement()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            float h = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
            float v = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            Vector3 move = new Vector3(h, 0f, v) * moveSpeed;
            move.y -= 9.81f;
            _cc.Move(move * Time.deltaTime);
        }

        /// <summary>E 키를 누르면 발 아래 타일에 현재 도구를 사용한다.</summary>
        private void HandleInteract()
        {
            if (_toolSystem == null) return;

            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            bool ePressed = keyboard.eKey.isPressed;
            if (ePressed && !_prevEPressed)
            {
                if (_farmGrid == null)
                    _farmGrid = Object.FindFirstObjectByType<FarmGrid>();

                if (_farmGrid != null)
                {
                    var tile = _farmGrid.GetTileAtWorldPos(transform.position);
                    if (tile != null)
                    {
                        var gridPos = new Vector2Int(tile.gridX, tile.gridY);
                        _toolSystem.TryUseToolAt(gridPos);
                    }
                }
            }
            _prevEPressed = ePressed;
        }
    }
}
