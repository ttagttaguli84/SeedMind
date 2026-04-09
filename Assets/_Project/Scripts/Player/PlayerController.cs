using UnityEngine;
using UnityEngine.InputSystem;

namespace SeedMind.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public float moveSpeed = 5.0f; // -> see docs/mcp/farming-tasks.md section C-1
        private CharacterController _cc;

        private void Awake()
        {
            _cc = GetComponent<CharacterController>();
        }

private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            float h = (keyboard.dKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed ? 1f : 0f);
            float v = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            Vector3 move = new Vector3(h, 0f, v) * moveSpeed;
            move.y -= 9.81f;
            _cc.Move(move * Time.deltaTime);
        }
    }
}
