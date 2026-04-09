// AnimalSlotUI — 동물 구매 목록 슬롯 UI
// -> see docs/systems/livestock-architecture.md 섹션 6.1
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Livestock.Data;

namespace SeedMind.UI
{
    public class AnimalSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _animalIcon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private Button _selectButton;

        private AnimalData _data;
        private Action<AnimalData> _onSelected;

        private void Awake()
        {
            if (_selectButton != null)
                _selectButton.onClick.AddListener(DoSelect);
        }

        public void Setup(AnimalData data, Action<AnimalData> onSelected)
        {
            _data = data;
            _onSelected = onSelected;
            if (_nameText  != null) _nameText.text  = data.animalName;
            if (_priceText != null) _priceText.text = $"{data.purchasePrice}G";
            if (_animalIcon != null && data.animalPrefab != null)
            {
                // 아이콘은 추후 Sprite 할당으로 교체 예정
            }
        }

        private void DoSelect()
        {
            _onSelected?.Invoke(_data);
        }
    }
}
