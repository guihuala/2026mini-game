using UnityEngine;

public class GameManager : Singleton<GameManager>
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

    // 游戏开始时初始化状态
    void Start()
    {
        SetGameState(GameState.Playing);
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
