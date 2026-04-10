// S-14: 상시 HUD 관리자
// -> see docs/systems/ui-architecture.md 섹션 4.3
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SeedMind.UI
{
    /// <summary>
    /// 게임 플레이 중 항상 표시되는 상시 UI(시간, 골드, 툴바, 세이브 표시)를 관리.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        // --- 참조: 시간/날짜 ---
        [Header("시간/날짜")]
        [SerializeField] private TMP_Text _timeText;
        [SerializeField] private TMP_Text _dayText;
        [SerializeField] private Image _seasonIcon;
        [SerializeField] private Image _weatherIcon;

        // --- 참조: 경제 ---
        [Header("경제")]
        [SerializeField] private TMP_Text _goldText;

        // --- 참조: 진행 ---
        [Header("진행")]
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Slider _expBar;

        // --- 참조: 툴바 ---
        [Header("툴바")]
        [SerializeField] private Transform _toolbarContainer;
            // SlotUI[] 참조, 슬롯 수: -> see docs/systems/inventory-architecture.md 섹션 1.2
        [SerializeField] private Image _selectedHighlight;

        // --- 참조: 시스템 상태 ---
        [Header("시스템")]
        [SerializeField] private GameObject _saveIndicator;

        private void OnEnable()
        {
            UIEvents.OnHUDRefreshRequested += RefreshAll;
            UIEvents.OnScreenOpened += OnScreenOpened;
            UIEvents.OnScreenClosed += OnScreenClosed;
        }

        private void OnDisable()
        {
            UIEvents.OnHUDRefreshRequested -= RefreshAll;
            UIEvents.OnScreenOpened -= OnScreenOpened;
            UIEvents.OnScreenClosed -= OnScreenClosed;
        }

        public void RefreshAll()
        {
            RefreshGold();
            RefreshTime();
            RefreshLevel();
        }

        private void RefreshGold()
        {
            if (_goldText == null) return;
            var econ = Economy.EconomyManager.Instance;
            if (econ != null)
                _goldText.text = $"{econ.CurrentGold:N0}G";
        }

        private void RefreshTime()
        {
            var tm = Core.TimeManager.Instance;
            if (tm == null) return;
            if (_timeText != null)
                _timeText.text = $"{Mathf.FloorToInt(tm.CurrentHour):D2}:00";
            if (_dayText != null)
                _dayText.text = $"Day {tm.CurrentDay}";
        }

        private void RefreshLevel()
        {
            var pm = Level.ProgressionManager.Instance;
            if (pm == null) return;
            if (_levelText != null)
                _levelText.text = $"Lv.{pm.CurrentLevel}";
            if (_expBar != null)
                _expBar.value = pm.ExpProgress;
        }

        public void ShowSaveIndicator()
        {
            if (_saveIndicator != null) _saveIndicator.SetActive(true);
        }

        public void HideSaveIndicator()
        {
            if (_saveIndicator != null) _saveIndicator.SetActive(false);
        }

        private void OnScreenOpened(ScreenType type)
        {
            // 화면 열릴 때 HUD 축소 처리 (향후 구현)
        }

        private void OnScreenClosed(ScreenType type)
        {
            // 화면 닫힐 때 HUD 복원 처리
        }
    }
}
