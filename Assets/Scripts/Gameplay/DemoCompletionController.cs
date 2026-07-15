using UnityEngine;

public sealed class DemoCompletionController : MonoBehaviour
{
    public bool IsVisible { get; private set; }

    public void Show()
    {
        if (IsVisible || UIManager.Instance == null) return;
        DemoCompletionPanel panel = UIManager.Instance.OpenPanel("DemoCompletionPanel", null, UIPanelLayer.Top) as DemoCompletionPanel;
        if (panel == null) return;
        IsVisible = true;
        ExplorationControlLock.Acquire(this);
        DialogueRuntimeState.SetFlag("greybox.demo_complete", true);
        panel.Configure(ReturnToMainMenu, StayInScene);
    }

    private void ReturnToMainMenu()
    {
        ClosePanel();
        if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
        else if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(GameScene.MainMenu);
    }

    private void StayInScene() => ClosePanel();

    private void ClosePanel()
    {
        IsVisible = false;
        ExplorationControlLock.Release(this);
        if (UIManager.Instance != null && UIManager.Instance.IsPanelOpen("DemoCompletionPanel"))
            UIManager.Instance.ClosePanel("DemoCompletionPanel");
    }

    private void OnDisable()
    {
        ExplorationControlLock.Release(this);
    }
}
