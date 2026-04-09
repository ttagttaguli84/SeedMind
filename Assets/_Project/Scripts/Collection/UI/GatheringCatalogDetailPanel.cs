using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SeedMind.Collection.UI
{
    /// <summary>
    /// 채집 도감 상세 패널. 선택된 아이템의 발견 여부에 따라 힌트/설명 전환.
    /// -> see docs/systems/collection-architecture.md 섹션 6.3
    /// </summary>
    public class GatheringCatalogDetailPanel : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _rarityText;
        [SerializeField] private TMP_Text _totalGatheredText;
        [SerializeField] private TMP_Text _bestQualityText;
        [SerializeField] private TMP_Text _firstGatheredText;

        private static readonly string[] QualityNames = { "보통", "실버", "골드", "이리듐" };
        private static readonly string[] SeasonNames = { "봄", "여름", "가을", "겨울" };

        public void ShowItem(GatheringCatalogData data, GatheringCatalogEntry entry)
        {
            gameObject.SetActive(true);
            bool discovered = entry != null && entry.isDiscovered;

            if (_nameText != null)
                _nameText.text = discovered ? data.name : "???";

            if (_descriptionText != null)
                _descriptionText.text = discovered ? data.descriptionUnlocked : data.hintLocked;

            if (_rarityText != null)
                _rarityText.text = discovered ? data.rarity.ToString() : "?";

            if (_icon != null && data.catalogIcon != null)
                _icon.sprite = data.catalogIcon;

            if (_totalGatheredText != null)
                _totalGatheredText.text = discovered ? $"총 채집: {entry.totalGathered}회" : "-";

            if (_bestQualityText != null)
            {
                if (discovered && entry.bestQuality >= 0 && entry.bestQuality < QualityNames.Length)
                    _bestQualityText.text = $"최고 품질: {QualityNames[entry.bestQuality]}";
                else
                    _bestQualityText.text = "-";
            }

            if (_firstGatheredText != null)
            {
                if (discovered && entry.firstGatheredDay >= 0)
                {
                    string season = entry.firstGatheredSeason >= 0 && entry.firstGatheredSeason < SeasonNames.Length
                        ? SeasonNames[entry.firstGatheredSeason] : "?";
                    _firstGatheredText.text = $"첫 발견: {entry.firstGatheredYear}년 {season} {entry.firstGatheredDay}일";
                }
                else
                {
                    _firstGatheredText.text = "-";
                }
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
