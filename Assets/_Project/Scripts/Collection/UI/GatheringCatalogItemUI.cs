using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SeedMind.Collection.UI
{
    /// <summary>
    /// 채집 도감 목록 아이템 UI 슬롯.
    /// -> see docs/systems/collection-architecture.md 섹션 6.3
    /// </summary>
    public class GatheringCatalogItemUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _rarityBadge;
        [SerializeField] private TMP_Text _gatheredCountText;
        [SerializeField] private GameObject _lockOverlay;
        [SerializeField] private Button _button;

        private string _itemId;
        private GatheringCatalogUI _parentPanel;

        private void Awake()
        {
            _button ??= GetComponent<Button>();
            _button?.onClick.AddListener(OnClick);
        }

        public void SetData(GatheringCatalogData data, GatheringCatalogEntry entry, GatheringCatalogUI parent)
        {
            _itemId = data.itemId;
            _parentPanel = parent;

            bool discovered = entry != null && entry.isDiscovered;

            if (_lockOverlay != null)
                _lockOverlay.SetActive(!discovered);

            if (_nameText != null)
                _nameText.text = discovered ? data.name : "???";

            if (_gatheredCountText != null)
                _gatheredCountText.text = discovered ? $"x{entry.totalGathered}" : "";

            if (_icon != null && data.catalogIcon != null)
                _icon.sprite = data.catalogIcon;
        }

        private void OnClick()
        {
            _parentPanel?.SelectItem(_itemId);
        }
    }
}
