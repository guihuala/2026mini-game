using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    public Button newGameButton;
    public Button settingsButton;
    public Button exitButton;

    private void Awake()
    {
        BindButton(newGameButton, OnNewGameButtonClicked);
        BindButton(settingsButton, OnSettingsButtonClicked);
        BindButton(exitButton, OnExitButtonClicked);
    }

    private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null) return;
        button.onClick.AddListener(action);
        button.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySfx("点击按钮");
        });
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
