namespace mardevmil.UI
{
    using UnityEngine;
    using UnityEngine.UI;

    public class SettingsWindow : BaseWindow
    {

        [SerializeField]
        private Button _soundToggleButton;
        [SerializeField]
        private Button _musicToggleButton;
        [SerializeField]
        private Button _mainMenuButton;
        [SerializeField]
        private Button _closeButton;

        private void Start()
        {
            _soundToggleButton.onClick.AddListener(OnSoundToggle);
            _musicToggleButton.onClick.AddListener(OnMusicToggle);
            _mainMenuButton.onClick.AddListener(OnMainMenu);
            _closeButton.onClick.AddListener(OnClose);
        }

        private void OnDestroy()
        {
            _soundToggleButton.onClick.RemoveAllListeners();
            _musicToggleButton.onClick.RemoveAllListeners();
            _mainMenuButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
        }

        private void OnMainMenu()
        {
            GuiManager.ShowWindow("UI_MAIN_MENU");
        }

        private void OnSoundToggle()
        {
            Debug.LogError("+++ OnSoundToggle");
        }

        private void OnMusicToggle()
        {
            Debug.LogError("+++ OnMusicToggle");
        }

        private void OnClose()
        {
            GuiManager.ShowWindow("UI_IN_GAME");
        }
    }
}
