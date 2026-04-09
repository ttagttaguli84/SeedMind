using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Building.Data;

namespace SeedMind.UI
{
    /// <summary>
    /// 개별 레시피 표시 UI. 아이콘, 이름, 재료 요구량, 가공 시간 표시.
    /// -> see docs/systems/processing-architecture.md 섹션 7.2
    /// </summary>
    public class RecipeSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _recipeName;
        [SerializeField] private TMP_Text _materialText;
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private Button _selectButton;

        private ProcessingRecipeData _recipe;
        private ProcessingUI _owner;

        public void Setup(ProcessingRecipeData recipe, ProcessingUI owner)
        {
            _recipe = recipe;
            _owner = owner;

            if (_icon != null && recipe.icon != null)
                _icon.sprite = recipe.icon;
            if (_recipeName != null)
                _recipeName.text = recipe.displayName;
            if (_materialText != null)
                _materialText.text = $"{recipe.inputItemId} x{recipe.inputQuantity}";
            if (_timeText != null)
                _timeText.text = $"{recipe.processingTimeHours:0.#}시간";

            if (_selectButton != null)
            {
                _selectButton.onClick.RemoveAllListeners();
                _selectButton.onClick.AddListener(OnClick);
            }
        }

        private void OnClick()
        {
            _owner?.OnRecipeSelected(_recipe);
        }
    }
}
