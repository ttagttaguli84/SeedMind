// AnimalShopUI — 동물 구매 팝업 UI
// -> see docs/systems/livestock-architecture.md 섹션 6.1
using UnityEngine;
using UnityEngine.UI;
using SeedMind.Livestock;
using SeedMind.Livestock.Data;

namespace SeedMind.UI
{
    public class AnimalShopUI : ScreenBase
    {
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private Transform _animalSlotContainer;
        [SerializeField] private GameObject _animalSlotPrefab;
        [SerializeField] private Button _closeButton;

        private AnimalData _selectedAnimal;

        protected override void Awake()
        {
            base.Awake();
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
        }

        public void Open() { Show(); RefreshSlots(); }

        public override void Show()
        {
            base.Show();
            if (_shopPanel != null) _shopPanel.SetActive(true);
        }

        public override void Hide()
        {
            base.Hide();
            if (_shopPanel != null) _shopPanel.SetActive(false);
        }

        private void RefreshSlots()
        {
            if (_animalSlotContainer == null || _animalSlotPrefab == null) return;
            foreach (Transform child in _animalSlotContainer)
                Destroy(child.gameObject);

            var allData = Resources.LoadAll<AnimalData>("Data/Animals");
            foreach (var data in allData)
            {
                var slotGO = Instantiate(_animalSlotPrefab, _animalSlotContainer);
                var slot = slotGO.GetComponent<AnimalSlotUI>();
                if (slot != null)
                    slot.Setup(data, SelectAnimal);
            }
        }

        private void SelectAnimal(AnimalData data)
        {
            _selectedAnimal = data;
            ExecuteBuy();
        }

        private void ExecuteBuy()
        {
            if (_selectedAnimal == null) return;
            var manager = AnimalManager.Instance;
            if (manager == null) return;
            if (manager.TryBuyAnimal(_selectedAnimal.animalId))
            {
                Debug.Log($"[AnimalShopUI] 구매 성공: {_selectedAnimal.animalName}");
                RefreshSlots();
            }
        }
    }
}
