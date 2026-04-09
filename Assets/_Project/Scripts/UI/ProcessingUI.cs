using System.Collections.Generic;
using UnityEngine;
using SeedMind.Building;
using SeedMind.Building.Data;

namespace SeedMind.UI
{
    /// <summary>
    /// 가공소 인터랙션 UI. 레시피 선택 및 슬롯 상태 표시를 담당.
    /// -> see docs/systems/processing-architecture.md 섹션 7.2
    /// </summary>
    public class ProcessingUI : ScreenBase
    {
        [SerializeField] private Transform _recipeListParent;
        [SerializeField] private Transform _slotStatusParent;
        [SerializeField] private GameObject _recipeSlotPrefab;
        [SerializeField] private GameObject _processingSlotPrefab;

        private BuildingInstance _currentProcessor;
        private BuildingManager _buildingManager;

        private readonly List<RecipeSlotUI> _recipeSlots = new List<RecipeSlotUI>();
        private readonly List<ProcessingSlotUI> _processingSlots = new List<ProcessingSlotUI>();

        protected override void Awake()
        {
            base.Awake();
            if (_recipeListParent == null)
                _recipeListParent = transform.Find("RecipeListArea");
            if (_slotStatusParent == null)
                _slotStatusParent = transform.Find("SlotStatusArea");

            _buildingManager = FindObjectOfType<BuildingManager>();
        }

        private void OnEnable()
        {
            BuildingEvents.OnProcessingStarted   += OnProcessingStateChanged;
            BuildingEvents.OnProcessingComplete  += OnProcessingStateChanged;
            BuildingEvents.OnProcessingCollected += OnCollectedHandler;
        }

        private void OnDisable()
        {
            BuildingEvents.OnProcessingStarted   -= OnProcessingStateChanged;
            BuildingEvents.OnProcessingComplete  -= OnProcessingStateChanged;
            BuildingEvents.OnProcessingCollected -= OnCollectedHandler;
        }

        /// <summary>
        /// 가공소 인터랙션 시 호출.
        /// </summary>
        public void Open(BuildingInstance processor)
        {
            _currentProcessor = processor;
            gameObject.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            _currentProcessor = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 레시피 슬롯 클릭 시 UI에서 호출.
        /// </summary>
        public void OnRecipeSelected(ProcessingRecipeData recipe)
        {
            if (_currentProcessor == null || recipe == null) return;
            // 실제 StartProcessing 연결은 인벤토리 시스템(Phase E) 이후 처리
            Debug.Log($"[ProcessingUI] 레시피 선택: {recipe.displayName}");
        }

        /// <summary>
        /// 수거 버튼 클릭 시 호출.
        /// </summary>
        public void OnCollectClicked(int slotIndex)
        {
            if (_currentProcessor == null) return;
            Debug.Log($"[ProcessingUI] 수거 요청: slot {slotIndex}");
            // 실제 CollectOutput 연결은 인벤토리/이코노미 시스템(Phase E) 이후 처리
        }

        private void Refresh()
        {
            // 슬롯 UI 갱신 (간단한 갱신 — 구체적 내용은 Phase E에서 연결)
            foreach (var slot in _processingSlots)
                slot.RefreshFromEvent();
        }

        private void OnProcessingStateChanged(BuildingInstance proc, int slotIndex)
        {
            if (proc == _currentProcessor) Refresh();
        }

        private void OnCollectedHandler(BuildingInstance proc, int slotIndex, string outputItemId)
        {
            if (proc == _currentProcessor) Refresh();
        }
    }
}
