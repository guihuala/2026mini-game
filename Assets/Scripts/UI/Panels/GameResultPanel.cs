using UnityEngine;
using UnityEngine.UI;

public class GameResultPanel : BasePanel
{
    [Header("Result")]
    [SerializeField] private Text resultTitle;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    protected override void Awake()
    {
        base.Awake();

        if (resumeButton != null) resumeButton.onClick.AddListener(OnNextLevelClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnReplayClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void Start()
    {
        Refresh();
    }

    private bool isVictory;

    public void Configure(bool victory)
    {
        isVictory = victory;
        Refresh();
    }

    private void Refresh()
    {
        if (resultTitle != null)
            resultTitle.text = isVictory ? "你胜利了" : "挑战失败";

        if (resumeButton != null)
        {
            resumeButton.gameObject.SetActive(isVictory);
            resumeButton.interactable = isVictory && GetNextLevel() != null;
        }
    }

    private void OnNextLevelClicked()
    {
        LevelDefinition nextLevel = GetNextLevel();
        if (nextLevel != null && SceneLoader.Instance != null)
        {
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
            SceneLoader.Instance.LoadLevel(nextLevel.id);
        }
    }

    private void OnReplayClicked()
    {
        if (SceneLoader.Instance != null)
        {
            if (GameManager.Instance != null) GameManager.Instance.StartGame();
            SceneLoader.Instance.LoadCurrentLevel();
        }
    }

    private void OnMainMenuClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMainMenu();
    }

    private static LevelDefinition GetNextLevel()
    {
        LevelCatalog catalog = LevelProgress.Catalog;
        LevelDefinition current = LevelProgress.GetCurrentLevel();
        if (catalog == null || current == null) return null;

        int currentIndex = catalog.IndexOf(current.id);
        return currentIndex >= 0 ? catalog.Get(currentIndex + 1) : null;
    }
}
