using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace SeedMind.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("버튼")]
        [SerializeField] private Button _btnNewGame;
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Button _btnSettings;
        [SerializeField] private Button _btnQuit;

        [Header("패널")]
        [SerializeField] private GameObject _settingsPanel;

        private void Awake()
        {
            _btnNewGame?.onClick.AddListener(OnNewGame);
            _btnContinue?.onClick.AddListener(OnContinue);
            _btnSettings?.onClick.AddListener(OnSettings);
            _btnQuit?.onClick.AddListener(OnQuit);

            _settingsPanel?.SetActive(false);
        }

        private void OnNewGame()
        {
            SceneManager.LoadScene("SCN_Farm");
        }

        private void OnContinue()
        {
            // TODO: 세이브 데이터 존재 여부 확인 후 로드
            SceneManager.LoadScene("SCN_Farm");
        }

        private void OnSettings()
        {
            if (_settingsPanel != null)
                _settingsPanel.SetActive(!_settingsPanel.activeSelf);
        }

        private void OnQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
