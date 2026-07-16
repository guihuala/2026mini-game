using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public sealed class GreyboxHudPanel : BasePanel
{
    [SerializeField] private Text promptText;
    [SerializeField] private GameObject promptRoot;

    [Header("Prompt Motion")]
    [SerializeField] private float promptFadeDuration = 0.18f;
    [SerializeField] private float promptMoveDistance = 18f;
    [SerializeField] private float promptPunchScale = 0.08f;

    private CanvasGroup promptCanvasGroup;
    private RectTransform promptRect;
    private Vector2 promptRestPosition;
    private string currentPrompt = string.Empty;
    private bool promptVisible;

    protected override void Awake()
    {
        base.Awake();
        if (promptRoot == null) return;

        promptCanvasGroup = promptRoot.GetComponent<CanvasGroup>();
        if (promptCanvasGroup == null) promptCanvasGroup = promptRoot.AddComponent<CanvasGroup>();
        promptRect = promptRoot.transform as RectTransform;
        if (promptRect != null) promptRestPosition = promptRect.anchoredPosition;

        promptCanvasGroup.alpha = 0f;
        promptRoot.SetActive(false);
    }

    public void SetPrompt(string value)
    {
        bool visible = !string.IsNullOrEmpty(value);
        value = value ?? string.Empty;
        if (visible == promptVisible && value == currentPrompt) return;

        bool contentChanged = visible && value != currentPrompt;
        currentPrompt = value;
        if (promptText != null) promptText.text = value;
        if (promptRoot == null || promptCanvasGroup == null) return;

        promptRoot.transform.DOKill();
        promptCanvasGroup.DOKill();

        if (visible && !promptVisible)
        {
            promptRoot.SetActive(true);
            promptCanvasGroup.alpha = 0f;
            promptRoot.transform.localScale = Vector3.one * 0.92f;
            if (promptRect != null) promptRect.anchoredPosition = promptRestPosition - Vector2.up * promptMoveDistance;

            promptCanvasGroup.DOFade(1f, promptFadeDuration).SetEase(Ease.OutQuad).SetUpdate(true);
            promptRoot.transform.DOScale(Vector3.one, promptFadeDuration + 0.08f).SetEase(Ease.OutBack).SetUpdate(true);
            if (promptRect != null)
            {
                promptRect.DOAnchorPos(promptRestPosition, promptFadeDuration + 0.04f).SetEase(Ease.OutCubic).SetUpdate(true);
            }
        }
        else if (!visible && promptVisible)
        {
            promptCanvasGroup.DOFade(0f, promptFadeDuration * 0.8f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (!promptVisible && promptRoot != null) promptRoot.SetActive(false);
                });
            promptRoot.transform.DOScale(0.96f, promptFadeDuration * 0.8f).SetEase(Ease.InQuad).SetUpdate(true);
        }
        else if (contentChanged)
        {
            promptRoot.transform.localScale = Vector3.one;
            promptRoot.transform.DOPunchScale(Vector3.one * promptPunchScale, 0.22f, 5, 0.45f).SetUpdate(true);
        }

        promptVisible = visible;
    }

    protected override void OnDestroy()
    {
        if (promptRoot != null) promptRoot.transform.DOKill();
        if (promptCanvasGroup != null) promptCanvasGroup.DOKill();
        base.OnDestroy();
    }
}
