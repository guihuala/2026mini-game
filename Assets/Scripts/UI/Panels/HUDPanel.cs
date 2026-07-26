using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : BasePanel
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private Text levelNameText;

    protected override void Awake()
    {
        base.Awake();
        if (pauseButton != null) pauseButton.onClick.AddListener(OpenPause);
    }

    private void Start()
    {
        LevelDefinition level = LevelProgress.GetCurrentLevel();
        if (levelNameText != null)
            levelNameText.text = level != null ? level.displayName : string.Empty;
    }

    private void OpenPause()
    {
        if (GameManager.Instance != null) GameManager.Instance.PauseGame();
    }
}
