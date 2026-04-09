// AnimalCareUI — 동물 돌봄 패널 UI
// -> see docs/systems/livestock-architecture.md 섹션 6.2
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Livestock;

namespace SeedMind.UI
{
    public class AnimalCareUI : ScreenBase
    {
        [SerializeField] private GameObject _carePanel;
        [SerializeField] private TextMeshProUGUI _animalNameText;
        [SerializeField] private Slider _happinessBar;
        [SerializeField] private Image _productIcon;
        [SerializeField] private Button _feedButton;
        [SerializeField] private Button _petButton;
        [SerializeField] private Button _collectButton;

        private AnimalInstance _currentAnimal;

        protected override void Awake()
        {
            base.Awake();
            if (_feedButton != null)    _feedButton.onClick.AddListener(DoFeed);
            if (_petButton != null)     _petButton.onClick.AddListener(DoPet);
            if (_collectButton != null) _collectButton.onClick.AddListener(DoCollect);
        }

        private void OnEnable()
        {
            var manager = AnimalManager.Instance;
            if (manager == null) return;
            manager.OnAnimalFed      += RefreshDisplay;
            manager.OnAnimalPetted   += RefreshDisplay;
            manager.OnProductCollected += (a, p) => RefreshDisplay(a);
        }

        private void OnDisable()
        {
            var manager = AnimalManager.Instance;
            if (manager == null) return;
            manager.OnAnimalFed      -= RefreshDisplay;
            manager.OnAnimalPetted   -= RefreshDisplay;
        }

        public void ShowAnimal(AnimalInstance animal)
        {
            _currentAnimal = animal;
            Show();
            UpdateDisplay();
        }

        public override void Show()
        {
            base.Show();
            if (_carePanel != null) _carePanel.SetActive(true);
        }

        public override void Hide()
        {
            base.Hide();
            if (_carePanel != null) _carePanel.SetActive(false);
            _currentAnimal = null;
        }

        private void UpdateDisplay()
        {
            if (_currentAnimal == null) return;
            if (_animalNameText != null) _animalNameText.text = _currentAnimal.displayName;
            if (_happinessBar != null)   _happinessBar.value  = _currentAnimal.happiness / 100f;
            if (_collectButton != null)  _collectButton.interactable = _currentAnimal.isProductReady;
        }

        private void RefreshDisplay(AnimalInstance animal)
        {
            if (_currentAnimal == null || _currentAnimal.instanceId != animal.instanceId) return;
            _currentAnimal = animal;
            UpdateDisplay();
        }

        private void DoFeed()
        {
            if (_currentAnimal == null) return;
            AnimalManager.Instance?.FeedAnimal(_currentAnimal);
        }

        private void DoPet()
        {
            if (_currentAnimal == null) return;
            AnimalManager.Instance?.PetAnimal(_currentAnimal);
        }

        private void DoCollect()
        {
            if (_currentAnimal == null) return;
            AnimalManager.Instance?.CollectProduct(_currentAnimal);
        }
    }
}
