using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    public Button newGameButton;
    public Button settingsButton;
    public Button exitButton;

    private void Awake()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    public void OnNewGameButtonClicked()
    {
        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.HasSave()) SaveManager.Instance.LoadGame();
            else SaveManager.Instance.NewGame();
        }

        OpenLevelSelect();
    }

    public void OpenLevelSelect()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.OpenPanel("LevelSelectPanel", null, UIPanelLayer.Popup);
    }

    public void OnSettingsButtonClicked()
    {
        UIManager.Instance.OpenPanel("SettingPanel");
    }

    public void OnExitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
