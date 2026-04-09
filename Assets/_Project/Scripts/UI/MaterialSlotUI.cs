// S-08: 재료 슬롯 UI (아이콘 + 이름 + 수량)
// -> see docs/systems/blacksmith-architecture.md 섹션 3.1
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SeedMind.UI
{
    /// <summary>
    /// 업그레이드 비용 패널에서 재료 1종의 정보를 표시하는 슬롯.
    /// </summary>
    public class MaterialSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _materialIcon;
        [SerializeField] private TMP_Text _materialName;
        [SerializeField] private TMP_Text _quantityText;  // "보유/필요" 형식

        /// <summary>재료 슬롯 초기화. 부족 시 빨간색, 충족 시 흰색.</summary>
        public void Setup(string materialId, int required, int owned)
        {
            // -> see blacksmith-architecture.md 섹션 3.2 충족/부족 규칙
            if (_materialName != null)
                _materialName.text = materialId;

            if (_quantityText != null)
            {
                _quantityText.text = $"{owned}/{required}";
                _quantityText.color = owned >= required
                    ? Color.white
                    : Color.red;
            }
        }
    }
}
