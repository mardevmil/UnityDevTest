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
        [SerializeField]
        private Text _scoreText;

        private void Start()
        {
            _pauseButton.onClick.AddListener(OnPause);
            _settingsButton.onClick.AddListener(OnSettings);
            _tapToPlayButton.onClick.AddListener(OnTapToPlay);
            EventManager.playerLandedOnBlock += OnPlayerLandedOnBlock;
        }

        private void OnDestroy()
        {
            _pauseButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _tapToPlayButton.onClick.RemoveAllListeners();
            EventManager.playerLandedOnBlock -= OnPlayerLandedOnBlock;
        }

        private void OnPause()
        {
            GuiManager.ShowWindow("UI_PAUSE");
        }

        private void OnSettings()
        {
            GuiManager.ShowWindow("UI_SETTINGS_MENU");
        }

        public override void OnFocused()
        {
            base.OnFocused();
            _tapToPlayButton.gameObject.SetActive(true);
        }

        private void OnTapToPlay()
        {
            if(GameManager.Instance.gameStatus == GameManager.GameStatus.OnStart)
            {
                _tapToPlayButton.gameObject.SetActive(false);
                EventManager.gameStarted();
            }
        }

        private void OnPlayerLandedOnBlock(BlockController blockController)
        {
            _scoreText.text = GameManager.Instance.Counter.ToString();
        }
    }
}