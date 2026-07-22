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
    private Text questText;

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
        CreateQuestTracker();
    }

    public void SetQuest(string value)
    {
        if (questText != null && questText.text != value) questText.text = value ?? string.Empty;
    }

    private void CreateQuestTracker()
    {
        GameObject tracker = new GameObject("Quest Tracker", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        tracker.transform.SetParent(transform, false);
        RectTransform rect = tracker.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(28f, -28f);
        rect.sizeDelta = new Vector2(430f, 54f);
        Image background = tracker.GetComponent<Image>();
        background.color = new Color(0.04f, 0.05f, 0.07f, 0.82f);
        background.raycastTarget = false;

        GameObject label = new GameObject("Objective", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        label.transform.SetParent(tracker.transform, false);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(16f, 8f);
        labelRect.offsetMax = new Vector2(-16f, -8f);
        questText = label.GetComponent<Text>();
        questText.font = promptText != null ? promptText.font : null;
        questText.fontSize = 20;
        questText.color = Color.white;
        questText.alignment = TextAnchor.MiddleLeft;
        questText.raycastTarget = false;
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
