// S-17: 메뉴/설정 화면 (ScreenBase 파생)
// -> see docs/systems/ui-architecture.md 섹션 4.4
using UnityEngine;
using UnityEngine.UI;
using SeedMind.Save;

namespace SeedMind.UI
{
    /// <summary>
    /// Esc 키로 열리는 메뉴 화면. 게임 시간 일시 정지.
    /// 하위: ResumeButton, SaveButton, LoadButton, SettingsButton, QuitButton
    /// </summary>
    public class MenuUI : ScreenBase
    {
        [Header("메뉴 버튼")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        protected override void Awake()
        {
            base.Awake();
            _resumeButton?.onClick.AddListener(HandleResume);
            _saveButton?.onClick.AddListener(HandleSave);
            _loadButton?.onClick.AddListener(HandleLoad);
            _quitButton?.onClick.AddListener(HandleQuit);
        }

        public override void OnBeforeOpen()
        {
            // 게임 시간 정지
            Time.timeScale = 0f;
        }

        public override void OnAfterClose()
        {
            // 게임 시간 재개
            Time.timeScale = 1f;
        }

        private void HandleResume()
        {
            UIManager.Instance?.CloseCurrentScreen();
        }

        private void HandleSave()
        {
            SaveManager.Instance?.SaveAsync(0);
        }

        private void HandleLoad()
        {
            UIManager.Instance?.OpenScreen(ScreenType.SaveLoad);
        }

        private void HandleQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
