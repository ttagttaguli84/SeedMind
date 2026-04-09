using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.Economy;

namespace SeedMind.UI
{
    /// <summary>
    /// 아이템 툴팁 UI 컴포넌트.
    /// 슬롯 호버 시 아이템 이름/카테고리/설명/판매가를 표시한다.
    /// -> see docs/systems/inventory-architecture.md 섹션 2
    /// </summary>
    public class TooltipUI : MonoBehaviour
    {
        [Header("텍스트 참조")]
        [SerializeField] private TextMeshProUGUI _itemNameText;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _priceText;

        [Header("비주얼")]
        [SerializeField] private Image _qualityIcon;
        [SerializeField] private CanvasGroup _canvasGroup;

        [SerializeField] private float _showDelay = 0.3f;  // -> see docs/systems/inventory-system.md 섹션 4.3

        // 전역 인스턴스 참조 (SlotUI에서 ShowGlobal/HideGlobal 호출용)
        private static TooltipUI _instance;

        private Coroutine _showCoroutine;

        // ── Unity 생명주기 ────────────────────────────────────────────

        private void Awake()
        {
            _instance = this;
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        }

        // ── 전역 API ──────────────────────────────────────────────────

        public static void ShowGlobal(IInventoryItem item, CropQuality quality, Vector3 worldPos)
        {
            _instance?.Show(item, quality, worldPos);
        }

        public static void HideGlobal()
        {
            _instance?.Hide();
        }

        // ── 인스턴스 메서드 ──────────────────────────────────────────

        public void Show(IInventoryItem item, CropQuality quality, Vector3 screenPos)
        {
            if (_showCoroutine != null) StopCoroutine(_showCoroutine);
            _showCoroutine = StartCoroutine(ShowDelayed(item, quality, screenPos));
        }

        public void Hide()
        {
            if (_showCoroutine != null) { StopCoroutine(_showCoroutine); _showCoroutine = null; }
            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
        }

        private IEnumerator ShowDelayed(IInventoryItem item, CropQuality quality, Vector3 screenPos)
        {
            yield return new WaitForSeconds(_showDelay);

            // 내용 설정
            if (_itemNameText != null) _itemNameText.text = item.ItemName;
            if (_categoryText != null) _categoryText.text = item.ItemType.ToString();
            if (_priceText != null)
            {
                if (item.Sellable)
                    _priceText.text = $"판매가: ?G";  // 실제 가격은 EconomyManager에서 조회
                else
                    _priceText.text = "판매 불가";
            }

            // 품질 아이콘 색상
            if (_qualityIcon != null)
                _qualityIcon.color = GetQualityColor(quality);

            // 위치 설정
            transform.position = screenPos + new Vector3(20f, -20f, 0f);

            // 표시
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        }

        private static Color GetQualityColor(CropQuality quality)
        {
            return quality switch
            {
                CropQuality.Silver  => new Color(0.75f, 0.75f, 0.75f),
                CropQuality.Gold    => new Color(1f, 0.84f, 0f),
                CropQuality.Iridium => new Color(0.4f, 0.8f, 1f),
                _ => Color.white
            };
        }
    }
}
