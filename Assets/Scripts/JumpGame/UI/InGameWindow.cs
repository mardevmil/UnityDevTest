namespace JumpGame
{
    using UnityEngine;
    using UnityEngine.UI;

    public class InGameWindow : BaseWindow
    {
        [SerializeField]
        private Button _pauseButton;
        [SerializeField]
        private Button _settingsButton;
        [SerializeField]
        private Button _tapToPlayButton;

        private void Start()
        {
            _pauseButton.onClick.AddListener(OnPause);
            _settingsButton.onClick.AddListener(OnSettings);
            _tapToPlayButton.onClick.AddListener(OnTapToPlay);
        }

        private void OnDestroy()
        {
            _pauseButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _tapToPlayButton.onClick.RemoveAllListeners();
        }

        private void OnPause()
        {
            GuiManager.ShowWindow("UI_PAUSE");
        }

        private void OnSettings()
        {
            GuiManager.ShowWindow("UI_SETTINGS_MENU");
        }

        private void OnTapToPlay()
        {
            if(GameManager.Instance.gameStatus == GameManager.GameStatus.OnStart)
            {
                _tapToPlayButton.gameObject.SetActive(false);
                EventManager.gameStarted();
            }
        }
    }
}