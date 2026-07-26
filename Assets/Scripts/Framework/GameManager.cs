using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonPersistent<GameManager>
{
    public enum GameState
    {
        Playing, // 游戏进行中
        Paused, // 游戏暂停
        GameOver // 游戏结束
    }

    private GameState currentState;
    public GameState CurrentState => currentState;
    public bool IsPaused => currentState == GameState.Paused;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // 游戏开始时初始化状态
    void Start()
    {
        SetGameState(GameState.Playing);
        RefreshHud(SceneManager.GetActiveScene());
    }

    private void Update()
    {
        if (InputManager.Instance != null &&
            InputManager.Instance.GetActionDown(InputActionType.Pause))
        {
            TogglePause();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetGameState(GameState.Playing);
        RefreshHud(scene);
    }

    private void RefreshHud(Scene scene)
    {
        if (UIManager.Instance == null) return;

        bool isLevel = false;
        LevelCatalog catalog = LevelProgress.Catalog;
        if (catalog != null)
        {
            foreach (LevelDefinition level in catalog.Levels)
            {
                if (level != null && level.sceneName == scene.name)
                {
                    isLevel = true;
                    break;
                }
            }
        }

        if (isLevel)
        {
            if (!UIManager.Instance.IsPanelOpen("HUDPanel"))
                UIManager.Instance.OpenPanel("HUDPanel", null, UIPanelLayer.Bottom);
        }
        else
        {
            ClosePanelIfOpen("HUDPanel");
        }
    }

    public void SetGameState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Playing:
                SetPaused(false);
                ClosePanelIfOpen("SettingPanel");
                ClosePanelIfOpen("PausePanel");
                ClosePanelIfOpen("GameResultPanel");
                break;

            case GameState.Paused:
                SetPaused(true);
                if (UIManager.Instance != null) UIManager.Instance.OpenPanel("PausePanel", null, UIPanelLayer.Top);
                break;

            case GameState.GameOver:
                SetPaused(true);
                if (UIManager.Instance != null) UIManager.Instance.OpenPanel("GameResultPanel");
                break;
        }
    }

    #region 状态控制

    // 游戏开始
    public void StartGame()
    {
        SetGameState(GameState.Playing);
    }

    // 暂停游戏
    public void PauseGame()
    {
        if (currentState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
    }

    public void TogglePause()
    {
        if (currentState == GameState.Playing) PauseGame();
        else if (currentState == GameState.Paused) ResumeGame();
    }

    // 恢复游戏
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }

    // 游戏结束
    public void EndGame()
    {
        SetGameState(GameState.GameOver);
    }

    public void CompleteLevel()
    {
        LevelProgress.CompleteCurrentLevel();
        SetGameState(GameState.GameOver);
    }

    // 返回主菜单
    public void ReturnToMainMenu()
    {
        if (SaveManager.Instance != null) SaveManager.Instance.SaveGame();
        SetGameState(GameState.Playing);
        if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(GameScene.MainMenu);
    }

    private void ClosePanelIfOpen(string panelName)
    {
        if (UIManager.Instance != null && UIManager.Instance.IsPanelOpen(panelName))
        {
            UIManager.Instance.ClosePanel(panelName);
        }
    }

    private void SetPaused(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
    }

    #endregion
}
