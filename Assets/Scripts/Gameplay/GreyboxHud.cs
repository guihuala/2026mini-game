using UnityEngine;

public sealed class GreyboxHud : MonoBehaviour
{
    [SerializeField] private PlayerInteractor interactor;
    private GreyboxHudPanel panel;

    private void Start()
    {
        if (interactor == null) interactor = FindObjectOfType<PlayerInteractor>();
        panel = UIManager.Instance != null
            ? UIManager.Instance.OpenPanel("GreyboxHudPanel", null, UIPanelLayer.Bottom) as GreyboxHudPanel
            : null;
    }

    private void Update()
    {
        HandlePauseInput();

        if (panel != null)
        {
            bool canShow = DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying;
            panel.SetPrompt(canShow && interactor != null ? interactor.CurrentPrompt : string.Empty);
            panel.SetQuest(DemoQuestChain.GetObjectiveText());
        }
    }

    private void HandlePauseInput()
    {
        if (InputManager.Instance == null || !InputManager.Instance.GetActionDown(InputActionType.Pause)) return;
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.IsPaused) GameManager.Instance.ResumeGame();
        else GameManager.Instance.PauseGame();
    }
}
