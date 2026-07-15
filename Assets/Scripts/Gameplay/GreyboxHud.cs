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
        if (panel == null) return;
        bool canShow = DialogueManager.Instance == null || !DialogueManager.Instance.IsPlaying;
        panel.SetPrompt(canShow && interactor != null ? interactor.CurrentPrompt : string.Empty);
    }
}
