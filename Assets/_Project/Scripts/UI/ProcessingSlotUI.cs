using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Building;

namespace SeedMind.UI
{
    /// <summary>
    /// 개별 가공 슬롯 상태 UI. 프로그레스 바, 수거 버튼 포함.
    /// -> see docs/systems/processing-architecture.md 섹션 7.2
    /// </summary>
    public class ProcessingSlotUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Button _collectButton;

        private int _slotIndex;
        private ProcessingUI _owner;
        private ProcessingSlot _slot;

        public void Setup(int slotIndex, ProcessingSlot slot, ProcessingUI owner)
        {
            _slotIndex = slotIndex;
            _slot = slot;
            _owner = owner;

            if (_collectButton != null)
            {
                _collectButton.onClick.RemoveAllListeners();
                _collectButton.onClick.AddListener(OnCollect);
            }

            Refresh();
        }

        public void RefreshFromEvent()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_slot == null) return;

            switch (_slot.State)
            {
                case ProcessingSlot.SlotState.Empty:
                    if (_statusText != null) _statusText.text = "비어있음";
                    if (_progressBar != null) _progressBar.value = 0f;
                    if (_collectButton != null) _collectButton.interactable = false;
                    break;

                case ProcessingSlot.SlotState.Processing:
                    string recipeName = _slot.Recipe != null ? _slot.Recipe.displayName : "가공 중";
                    if (_statusText != null)
                        _statusText.text = $"{recipeName} ({_slot.RemainingHours:0.#}시간 남음)";
                    if (_progressBar != null) _progressBar.value = _slot.ProgressRatio;
                    if (_collectButton != null) _collectButton.interactable = false;
                    break;

                case ProcessingSlot.SlotState.Completed:
                    string output = _slot.Recipe?.outputItemId ?? "완료";
                    if (_statusText != null) _statusText.text = $"{output} 완성!";
                    if (_progressBar != null) _progressBar.value = 1f;
                    if (_collectButton != null) _collectButton.interactable = true;
                    break;
            }
        }

        private void OnCollect()
        {
            _owner?.OnCollectClicked(_slotIndex);
        }
    }
}
