namespace JumpGame
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class PauseWindow : BaseWindow
    {
        [SerializeField]
        private Button _resumeButton;
        [SerializeField]
        private Button _restartButton;
        [SerializeField]
        private Button _mainMenuButton;

        private void Start()
        {
            _mainMenuButton.onClick.AddListener(OnMainMenu);
            _resumeButton.onClick.AddListener(OnResume);
            _restartButton.onClick.AddListener(OnRestart);
        }

        private void OnDestroy()
        {
            _mainMenuButton.onClick.RemoveAllListeners();
            _resumeButton.onClick.RemoveAllListeners();
            _restartButton.onClick.RemoveAllListeners();
        }

        private void OnMainMenu()
        {
            GuiManager.HideWindow("UI_MAIN_MENU");
            SceneManager.LoadScene("Init");
        }

        private void OnResume()
        {
            
        }

        private void OnRestart()
        {
            
        }
    }
}