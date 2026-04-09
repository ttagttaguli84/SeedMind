// S-18: 세이브/로드 슬롯 화면 (ScreenBase 파생)
// -> see docs/systems/ui-architecture.md 섹션 4.4
// -> see docs/systems/save-load-architecture.md for 슬롯 구조
using UnityEngine;
using UnityEngine.UI;

namespace SeedMind.UI
{
    /// <summary>
    /// Menu에서 진입하는 세이브/로드 슬롯 화면.
    /// 슬롯 수: -> see docs/systems/save-load-architecture.md
    /// </summary>
    public class SaveLoadUI : ScreenBase
    {
        [Header("슬롯")]
        [SerializeField] private Transform _slotContainer;
        [SerializeField] private Button _backButton;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(HandleBack);
        }

        public override void OnBeforeOpen()
        {
            // SaveManager에서 슬롯 데이터 로드 후 슬롯 UI 갱신 (향후 구현)
        }

        private void HandleBack()
        {
            UIManager.Instance?.OpenScreen(ScreenType.Menu);
        }
    }
}
