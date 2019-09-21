namespace JumpGame
{
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;

    public class DeathWindow : BaseWindow
    {
        [SerializeField]
        private Button _reviveButton;
        [SerializeField]
        private Button _restartButton;
        [SerializeField]
        private Button _mainMenuButton;

        private void Start()
        {
            _mainMenuButton.onClick.AddListener(OnMainMenu);
            _reviveButton.onClick.AddListener(OnRevive);
            _restartButton.onClick.AddListener(OnRestart);
        }

        private void OnDestroy()
        {
            _mainMenuButton.onClick.RemoveAllListeners();
            _reviveButton.onClick.RemoveAllListeners();
            _restartButton.onClick.RemoveAllListeners();
        }

        private void OnMainMenu()
        {
            GuiManager.HideWindow("UI_MAIN_MENU");
            SceneManager.LoadScene("Init");
        }

        private void OnRevive()
        {

        }

        private void OnRestart()
        {

        }
    }
}