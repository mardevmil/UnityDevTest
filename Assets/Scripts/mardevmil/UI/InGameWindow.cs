using UnityEngine;
using UnityEngine.UI;

public class InGameWindow : BaseWindow
{
    [SerializeField]
    private Button _mainMenuButton;
    [SerializeField]
    private Button _settingsButton;
    
    private void Start()
    {
        _mainMenuButton.onClick.AddListener(OnMainMenu);
        _settingsButton.onClick.AddListener(OnSettings);
    }

    private void OnDestroy()
    {
        _mainMenuButton.onClick.RemoveAllListeners();
        _settingsButton.onClick.RemoveAllListeners();
    }

    private void OnMainMenu()
    {        
        GuiManager.ShowWindow("UI_MAIN_MENU");
    }

    private void OnSettings()
    {
        GuiManager.ShowWindow("UI_SETTINGS_MENU");
    }
}
