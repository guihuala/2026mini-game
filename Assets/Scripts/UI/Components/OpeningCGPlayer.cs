using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Runtime-built opening sequence. Keeping it in code makes the sequence resolution
/// independent and prevents the title scene from containing another large UI hierarchy.
/// </summary>
public sealed class OpeningCGPlayer : MonoBehaviour, IPointerClickHandler
{
    private static readonly string[] Lines =
    {
        "“你瞧……重获光明的感觉如何？”\n\n见鬼，这是他妈哪儿？",
        "“我说过，我们会是天作之合……”\n\n之前你可没提到过这部分！\n\n“别那么天真，朋友，一切执念都有它的价码。”",
        "狡猾的家伙……噢，真是该死！\n\n“嘿，别抱怨了……现在我们是一根绳上的蚂蚱。”",
        "“噢……如何？我想你已经适应了恢复视力的感觉。”\n\n该死的恶魔！\n\n“正是在下。”",
        "你就要把我们害死了！\n\n“噢，亲爱的，你也害死了不少人。”\n\n那也是托你的福！\n\n“彼此彼此。”",
        "嘿，告诉我，刚刚那些东西……\n\n“‘那些东西’？噢，我以为你认出他们了……不过看起来你比我想象的还要冷血。”\n\n什么意思？\n\n“哼……你知道的，他们会对我们的交易不太满意。不过也能理解，谁让你把他们的眼睛剜出来了呢……交易愉快。”\n\n操。"
    };

    private Sprite[] frames;
    private Action onComplete;
    private Image image;
    private Text dialogue;
    private Text hint;
    private CanvasGroup group;
    private int index;
    private bool typing;
    private bool advanceRequested;
    private bool finishing;
    private Coroutine typingRoutine;

    public static void Play(Sprite[] cgFrames, Font font, Action completed)
    {
        GameObject root = new GameObject("OpeningCG", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
            typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(OpeningCGPlayer));
        DontDestroyOnLoad(root);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        root.GetComponent<OpeningCGPlayer>().Initialize(cgFrames, font, completed);
    }

    private void Initialize(Sprite[] cgFrames, Font font, Action completed)
    {
        frames = cgFrames;
        onComplete = completed;
        group = GetComponent<CanvasGroup>();

        Image backdrop = CreateImage("Backdrop", transform, new Color(0.015f, 0.012f, 0.018f, 1f));
        Stretch(backdrop.rectTransform);

        image = CreateImage("CG Frame", transform, Color.white);
        RectTransform imageRect = image.rectTransform;
        imageRect.anchorMin = new Vector2(0.08f, 0.39f);
        imageRect.anchorMax = new Vector2(0.92f, 0.94f);
        imageRect.offsetMin = imageRect.offsetMax = Vector2.zero;
        image.preserveAspect = true;

        Image textBackdrop = CreateImage("Dialogue Backdrop", transform, new Color(0f, 0f, 0f, 0.82f));
        RectTransform panelRect = textBackdrop.rectTransform;
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = new Vector2(1f, 0.43f);
        panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;

        dialogue = CreateText("Dialogue", textBackdrop.transform, font, 27, TextAnchor.UpperLeft);
        RectTransform dialogueRect = dialogue.rectTransform;
        dialogueRect.anchorMin = Vector2.zero;
        dialogueRect.anchorMax = Vector2.one;
        dialogueRect.offsetMin = new Vector2(110f, 44f);
        dialogueRect.offsetMax = new Vector2(-110f, -30f);

        hint = CreateText("Hint", transform, font, 20, TextAnchor.LowerRight);
        hint.text = "点击 / 空格继续    Esc 跳过";
        hint.color = new Color(1f, 1f, 1f, 0.58f);
        RectTransform hintRect = hint.rectTransform;
        hintRect.anchorMin = new Vector2(0.5f, 0f);
        hintRect.anchorMax = Vector2.one;
        hintRect.offsetMin = new Vector2(0f, 18f);
        hintRect.offsetMax = new Vector2(-38f, -18f);

        image.sprite = frames[0];
        group.alpha = 0f;
        StartCoroutine(Begin());
    }

    private IEnumerator Begin()
    {
        yield return null; // Ignore the click that opened the CG.
        for (float time = 0f; time < 0.45f; time += Time.unscaledDeltaTime)
        {
            group.alpha = time / 0.45f;
            yield return null;
        }
        group.alpha = 1f;
        ShowLine(0);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Finish();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            RequestAdvance();
    }

    public void OnPointerClick(PointerEventData eventData) => RequestAdvance();

    private void RequestAdvance()
    {
        if (finishing || group.alpha < 1f) return;
        if (typing)
        {
            advanceRequested = true;
            return;
        }

        if (++index >= Mathf.Min(frames.Length, Lines.Length)) Finish();
        else ShowLine(index);
    }

    private void ShowLine(int lineIndex)
    {
        image.sprite = frames[lineIndex];
        advanceRequested = false;
        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(Lines[lineIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        typing = true;
        dialogue.text = string.Empty;
        for (int i = 0; i < line.Length; i++)
        {
            if (advanceRequested)
            {
                dialogue.text = line;
                break;
            }
            dialogue.text += line[i];
            yield return new WaitForSecondsRealtime(0.035f);
        }
        typing = false;
        advanceRequested = false;
    }

    private void Finish()
    {
        if (finishing) return;
        finishing = true;
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        for (float time = 0.3f; time > 0f; time -= Time.unscaledDeltaTime)
        {
            group.alpha = time / 0.3f;
            yield return null;
        }
        Action completed = onComplete;
        Destroy(gameObject);
        completed?.Invoke();
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image result = go.GetComponent<Image>();
        result.color = color;
        return result;
    }

    private static Text CreateText(string name, Transform parent, Font font, int size, TextAnchor alignment)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);
        Text result = go.GetComponent<Text>();
        result.font = font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        result.fontSize = size;
        result.alignment = alignment;
        result.color = Color.white;
        result.horizontalOverflow = HorizontalWrapMode.Wrap;
        result.verticalOverflow = VerticalWrapMode.Overflow;
        return result;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
