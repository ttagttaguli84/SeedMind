using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using SeedMind.Player;

namespace SeedMind.UI
{
    /// <summary>
    /// 인벤토리/툴바 공용 슬롯 UI 컴포넌트.
    /// 아이템 아이콘, 수량, 품질 테두리, 선택 하이라이트를 표시한다.
    /// 드래그 앤 드롭과 호버 툴팁을 지원한다.
    /// -> see docs/systems/inventory-architecture.md 섹션 2 (SlotUI 클래스 다이어그램)
    /// </summary>
    public class SlotUI : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("참조")]
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _quantityText;
        [SerializeField] private Image _qualityBorder;
        [SerializeField] private Image _selectedHighlight;

        [Header("상태")]
        private ItemSlot _currentSlot;
        private SlotLocation _location;
        private int _slotIndex;
        private bool _isEmpty = true;

        // 드래그 관련
        private static SlotUI _dragSource;
        private static GameObject _dragIcon;
        private Canvas _rootCanvas;

        // ── 공개 API ─────────────────────────────────────────────────

        /// <summary>슬롯 데이터를 설정하고 UI를 갱신한다.</summary>
        public void SetSlot(ItemSlot slot, SlotLocation location, int index)
        {
            _currentSlot = slot;
            _location    = location;
            _slotIndex   = index;
            _isEmpty     = slot.IsEmpty;
            Refresh();
        }

        /// <summary>슬롯을 빈 상태로 초기화한다.</summary>
        public void SetEmpty(SlotLocation location, int index)
        {
            _currentSlot = ItemSlot.Empty;
            _location    = location;
            _slotIndex   = index;
            _isEmpty     = true;
            Refresh();
        }

        /// <summary>선택(툴바) 하이라이트를 설정한다.</summary>
        public void SetSelected(bool selected)
        {
            if (_selectedHighlight != null)
                _selectedHighlight.enabled = selected;
        }

        // ── 내부 갱신 ─────────────────────────────────────────────────

        private void Refresh()
        {
            if (_isEmpty)
            {
                if (_icon != null) { _icon.sprite = null; _icon.enabled = false; }
                if (_quantityText != null) _quantityText.text = "";
                if (_qualityBorder != null) _qualityBorder.enabled = false;
                return;
            }

            // 아이콘
            var item = SeedMind.Core.DataRegistry.Instance?.GetInventoryItem(_currentSlot.itemId);
            if (_icon != null)
            {
                _icon.sprite = item?.Icon;
                _icon.enabled = _icon.sprite != null;
            }

            // 수량 (1이면 숨김)
            if (_quantityText != null)
                _quantityText.text = _currentSlot.quantity > 1 ? _currentSlot.quantity.ToString() : "";

            // 품질 테두리 색상
            if (_qualityBorder != null)
            {
                _qualityBorder.enabled = true;
                _qualityBorder.color = GetQualityColor(_currentSlot.quality);
            }
        }

        private static Color GetQualityColor(SeedMind.Economy.CropQuality quality)
        {
            return quality switch
            {
                SeedMind.Economy.CropQuality.Silver  => new Color(0.75f, 0.75f, 0.75f),
                SeedMind.Economy.CropQuality.Gold    => new Color(1f, 0.84f, 0f),
                SeedMind.Economy.CropQuality.Iridium => new Color(0.4f, 0.8f, 1f),
                _ => Color.clear
            };
        }

        // ── 드래그 앤 드롭 ────────────────────────────────────────────

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isEmpty) return;
            _dragSource = this;

            // 드래그 아이콘 생성 (Canvas 최상단)
            _rootCanvas = GetComponentInParent<Canvas>();
            if (_rootCanvas != null && _icon != null && _icon.sprite != null)
            {
                _dragIcon = new GameObject("DragIcon");
                _dragIcon.transform.SetParent(_rootCanvas.transform, false);
                _dragIcon.transform.SetAsLastSibling();
                var img = _dragIcon.AddComponent<Image>();
                img.sprite = _icon.sprite;
                img.raycastTarget = false;
                var rt = _dragIcon.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(48, 48);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_dragIcon == null) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rootCanvas.GetComponent<RectTransform>(),
                eventData.position, eventData.pressEventCamera,
                out var localPos);
            _dragIcon.GetComponent<RectTransform>().localPosition = localPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_dragIcon != null) { Destroy(_dragIcon); _dragIcon = null; }
            _dragSource = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_isEmpty)
                ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipUI.HideGlobal();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 우클릭: 컨텍스트 메뉴 (향후 구현)
            }
            else if (_dragSource != null && _dragSource != this)
            {
                // 드롭 처리
                InventoryManager.Instance?.MoveSlot(_dragSource._location, _dragSource._slotIndex, _location, _slotIndex);
            }
        }

        private void ShowTooltip()
        {
            var item = SeedMind.Core.DataRegistry.Instance?.GetInventoryItem(_currentSlot.itemId);
            if (item != null)
                TooltipUI.ShowGlobal(item, _currentSlot.quality, transform.position);
        }
    }
}
