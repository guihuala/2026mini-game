using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    public Button newGameButton;
    public Button continueButton;
    public Button settingsButton;
    public Button exitButton;

    private void Awake()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGameButtonClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueButtonClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        if (exitButton != null) exitButton.onClick.AddListener(OnExitButtonClicked);
    }

    private void Start()
    {
        RefreshContinueButton();
    }

    public void OnNewGameButtonClicked()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSave())
        {
            UIManager.Instance.OpenConfirm(
                "开始新游戏",
                "已有游戏进度。要覆盖并开始新游戏吗？",
                StartNewGame);
            return;
        }

        StartNewGame();
    }

    private void StartNewGame()
    {
        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.HasSave())
            {
                if (!SaveManager.Instance.DeleteSave()) return;
            }
            else
            {
                SaveManager.Instance.NewGame();
            }
        }

        SceneLoader.Instance.LoadScene(GameScene.Gameplay);
    }

    public void OnContinueButtonClicked()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.LoadGame())
        {
            SceneLoader.Instance.LoadScene(GameScene.Gameplay);
        }
    }

    private void RefreshContinueButton()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(SaveManager.Instance != null && SaveManager.Instance.HasSave());
        }
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
