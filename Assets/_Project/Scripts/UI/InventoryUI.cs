using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SeedMind.Player;

namespace SeedMind.UI
{
    /// <summary>
    /// 인벤토리 패널 UI.
    /// InventoryManager의 슬롯 이벤트를 구독하여 SlotUI 배열을 갱신한다.
    /// -> see docs/systems/inventory-architecture.md 섹션 2 (InventoryUI 클래스 다이어그램)
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [Header("매니저 참조")]
        [SerializeField] private InventoryManager _inventoryManager;

        [Header("프리팹")]
        [SerializeField] private GameObject _slotUIPrefab;

        [Header("컨테이너")]
        [SerializeField] private Transform _backpackGrid;
        [SerializeField] private Transform _toolbarContainer;
        [SerializeField] private TooltipUI _tooltipPanel;

        // 슬롯 UI 배열
        private SlotUI[] _backpackSlotUIs;
        private SlotUI[] _toolbarSlotUIs;

        private bool _isOpen = false;

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Awake()
        {
            if (_inventoryManager == null)
                _inventoryManager = InventoryManager.Instance;
        }

        private void OnEnable()
        {
            if (_inventoryManager == null) return;
            _inventoryManager.OnBackpackChanged += OnBackpackChanged;
            _inventoryManager.OnToolbarChanged  += OnToolbarChanged;
            _inventoryManager.OnToolbarSelectionChanged += OnToolbarSelectionChanged;
        }

        private void OnDisable()
        {
            if (_inventoryManager == null) return;
            _inventoryManager.OnBackpackChanged -= OnBackpackChanged;
            _inventoryManager.OnToolbarChanged  -= OnToolbarChanged;
            _inventoryManager.OnToolbarSelectionChanged -= OnToolbarSelectionChanged;
        }

        private void Start()
        {
            BuildSlotUIs();
            RefreshAll();
            // 인벤토리 패널은 기본 닫힘
            if (gameObject.activeSelf) Close();
        }

        // ── 공개 API ─────────────────────────────────────────────────

        public void Open()
        {
            gameObject.SetActive(true);
            _isOpen = true;
            RefreshBackpack();
        }

        public void Close()
        {
            gameObject.SetActive(false);
            _isOpen = false;
            TooltipUI.HideGlobal();
        }

        public void Toggle()
        {
            if (_isOpen) Close();
            else Open();
        }

        public void RefreshAll()
        {
            RefreshBackpack();
            RefreshToolbar();
        }

        // ── 슬롯 UI 생성 ─────────────────────────────────────────────

        private void BuildSlotUIs()
        {
            if (_inventoryManager == null) return;

            // 배낭 슬롯 UI 생성
            var backpackSlots = _inventoryManager.BackpackSlots;
            _backpackSlotUIs = new SlotUI[backpackSlots.Count];
            if (_backpackGrid != null && _slotUIPrefab != null)
            {
                for (int i = 0; i < backpackSlots.Count; i++)
                {
                    var go = Instantiate(_slotUIPrefab, _backpackGrid);
                    _backpackSlotUIs[i] = go.GetComponent<SlotUI>();
                }
            }

            // 툴바 슬롯 UI 생성
            var toolbarSlots = _inventoryManager.ToolbarSlots;
            _toolbarSlotUIs = new SlotUI[toolbarSlots.Count];
            if (_toolbarContainer != null && _slotUIPrefab != null)
            {
                for (int i = 0; i < toolbarSlots.Count; i++)
                {
                    var go = Instantiate(_slotUIPrefab, _toolbarContainer);
                    _toolbarSlotUIs[i] = go.GetComponent<SlotUI>();
                }
            }
        }

        // ── 갱신 ─────────────────────────────────────────────────────

        private void RefreshBackpack()
        {
            if (_backpackSlotUIs == null || _inventoryManager == null) return;
            var slots = _inventoryManager.BackpackSlots;
            for (int i = 0; i < _backpackSlotUIs.Length; i++)
            {
                if (_backpackSlotUIs[i] == null) continue;
                if (i < slots.Count && !slots[i].IsEmpty)
                    _backpackSlotUIs[i].SetSlot(slots[i], SlotLocation.Backpack, i);
                else
                    _backpackSlotUIs[i].SetEmpty(SlotLocation.Backpack, i);
            }
        }

        private void RefreshToolbar()
        {
            if (_toolbarSlotUIs == null || _inventoryManager == null) return;
            var slots = _inventoryManager.ToolbarSlots;
            int selected = _inventoryManager.ToolbarSelectedIndex;
            for (int i = 0; i < _toolbarSlotUIs.Length; i++)
            {
                if (_toolbarSlotUIs[i] == null) continue;
                if (i < slots.Count && !slots[i].IsEmpty)
                    _toolbarSlotUIs[i].SetSlot(slots[i], SlotLocation.Toolbar, i);
                else
                    _toolbarSlotUIs[i].SetEmpty(SlotLocation.Toolbar, i);
                _toolbarSlotUIs[i].SetSelected(i == selected);
            }
        }

        // ── 이벤트 핸들러 ────────────────────────────────────────────

        private void OnBackpackChanged(InventoryChangeInfo info)
        {
            if (info.slotIndex == -1) RefreshBackpack();
            else if (_backpackSlotUIs != null && info.slotIndex < _backpackSlotUIs.Length)
            {
                var slots = _inventoryManager.BackpackSlots;
                int idx = info.slotIndex;
                if (idx < slots.Count && !slots[idx].IsEmpty)
                    _backpackSlotUIs[idx].SetSlot(slots[idx], SlotLocation.Backpack, idx);
                else
                    _backpackSlotUIs[idx].SetEmpty(SlotLocation.Backpack, idx);
            }
        }

        private void OnToolbarChanged(InventoryChangeInfo info)
        {
            RefreshToolbar();
        }

        private void OnToolbarSelectionChanged(int oldIdx, int newIdx)
        {
            if (_toolbarSlotUIs == null) return;
            if (oldIdx >= 0 && oldIdx < _toolbarSlotUIs.Length)
                _toolbarSlotUIs[oldIdx]?.SetSelected(false);
            if (newIdx >= 0 && newIdx < _toolbarSlotUIs.Length)
                _toolbarSlotUIs[newIdx]?.SetSelected(true);
        }
    }
}
