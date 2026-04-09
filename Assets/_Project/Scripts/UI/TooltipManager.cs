// S-15: 마우스 오버 툴팁 관리자 (Singleton)
// -> see docs/systems/ui-architecture.md 섹션 1.1
using UnityEngine;

namespace SeedMind.UI
{
    /// <summary>
    /// 마우스 오버 시 아이템/버튼 등의 툴팁을 표시한다.
    /// 상세 표시 로직은 TooltipUI (SlotUI 연동) 참조.
    /// </summary>
    public class TooltipManager : MonoBehaviour
    {
        public static TooltipManager Instance { get; private set; }

        [SerializeField] private TooltipUI _tooltipUI;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Show(string title, string description, Vector2 position)
        {
            if (_tooltipUI == null) return;
            _tooltipUI.gameObject.SetActive(true);
        }

        public void Hide()
        {
            if (_tooltipUI == null) return;
            _tooltipUI.Hide();
        }
    }
}
