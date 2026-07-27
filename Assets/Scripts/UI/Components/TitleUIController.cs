using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    public Button newGameButton;
    public Button settingsButton;
    public Button exitButton;
    [Header("Opening CG")]
    [SerializeField] private Sprite[] openingCGFrames;
    [SerializeField] private Font openingFont;

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
        bool isNewGame = SaveManager.Instance == null || !SaveManager.Instance.HasSave();
        if (SaveManager.Instance != null)
        {
            if (!isNewGame) SaveManager.Instance.LoadGame();
            else SaveManager.Instance.NewGame();
        }

        if (isNewGame && openingCGFrames != null && openingCGFrames.Length > 0)
        {
            OpeningCGPlayer.Play(openingCGFrames, openingFont, OpenLevelSelect);
            return;
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
