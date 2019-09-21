namespace mardevmil.UI
{

    using UnityEngine;
    using UnityEngine.UI;

    public class MainMenuWindow : BaseWindow
    {
        [SerializeField]
        private Button _tapToPlayButton;

        // Start is called before the first frame update
        private void Start()
        {
            _tapToPlayButton.onClick.AddListener(OnTapToPlay);
        }

        private void OnDestroy()
        {
            _tapToPlayButton.onClick.RemoveAllListeners();
        }

        private void OnTapToPlay()
        {
            GuiManager.ShowWindow("UI_IN_GAME");
        }
    }
}