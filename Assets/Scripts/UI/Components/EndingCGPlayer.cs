using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 全屏播放通关图片序列。使用不受 Time.timeScale 影响的时间，
/// 因此可以覆盖在已暂停的胜利结算界面上播放。
/// </summary>
public sealed class EndingCGPlayer : MonoBehaviour, IPointerClickHandler
{
    private Sprite[] frames;
    private Image frameView;
    private CanvasGroup canvasGroup;
    private int frameIndex;
    private bool acceptingInput;
    private bool finishing;

    public static void Play(Sprite[] cgFrames)
    {
        if (!HasUsableFrame(cgFrames)) return;

        GameObject root = new GameObject(
            "Ending CG",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup),
            typeof(EndingCGPlayer));
        DontDestroyOnLoad(root);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        root.GetComponent<EndingCGPlayer>().Initialize(cgFrames);
    }

    private void Initialize(Sprite[] cgFrames)
    {
        frames = cgFrames;
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;

        Image backdrop = CreateImage("Backdrop", transform, Color.black);
        Stretch(backdrop.rectTransform);

        frameView = CreateImage("Frame", transform, Color.white);
        Stretch(frameView.rectTransform);
        frameView.preserveAspect = true;

        Text hint = CreateHint(transform);
        RectTransform hintRect = hint.rectTransform;
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = Vector2.one;
        hintRect.offsetMin = new Vector2(0f, 20f);
        hintRect.offsetMax = new Vector2(-40f, -20f);

        frameIndex = FindNextFrame(-1);
        frameView.sprite = frames[frameIndex];
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        yield return null;

        const float duration = 0.35f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        acceptingInput = true;
    }

    private void Update()
    {
        if (!acceptingInput || finishing) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Finish();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            Advance();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (acceptingInput && !finishing)
            Advance();
    }

    private void Advance()
    {
        int nextIndex = FindNextFrame(frameIndex);
        if (nextIndex < 0)
        {
            Finish();
            return;
        }

        frameIndex = nextIndex;
        frameView.sprite = frames[frameIndex];
    }

    private int FindNextFrame(int currentIndex)
    {
        for (int i = currentIndex + 1; i < frames.Length; i++)
        {
            if (frames[i] != null)
                return i;
        }

        return -1;
    }

    private void Finish()
    {
        if (finishing) return;
        finishing = true;
        acceptingInput = false;
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        const float duration = 0.3f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        Destroy(gameObject);
    }

    private static bool HasUsableFrame(Sprite[] cgFrames)
    {
        if (cgFrames == null) return false;
        foreach (Sprite frame in cgFrames)
        {
            if (frame != null) return true;
        }

        return false;
    }

    private static Image CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject =
            new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static Text CreateHint(Transform parent)
    {
        GameObject textObject =
            new GameObject("Hint", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 20;
        text.alignment = TextAnchor.LowerRight;
        text.color = new Color(1f, 1f, 1f, 0.58f);
        text.text = "点击 / 空格继续    Esc 跳过";
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
