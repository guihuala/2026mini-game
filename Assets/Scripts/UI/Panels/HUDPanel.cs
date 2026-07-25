using UnityEngine;
using UnityEngine.UI;

public class HUDPanel : MonoBehaviour
{
    private static HUDPanel _instance;
    private Text _levelNameText;

    public static void Show(Transform parent)
    {
        if (_instance == null) _instance = Create(parent);
        _instance.gameObject.SetActive(true);

        LevelDefinition level = LevelProgress.GetCurrentLevel();
        _instance._levelNameText.text = level != null ? level.displayName : string.Empty;
    }

    public static void Hide()
    {
        if (_instance != null) _instance.gameObject.SetActive(false);
    }

    private static HUDPanel Create(Transform parent)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        GameObject root = UIObject("HUDPanel", parent);
        Stretch(root.GetComponent<RectTransform>());
        HUDPanel hud = root.AddComponent<HUDPanel>();

        hud._levelNameText = TextObject("LevelName", root.transform, font, 26, TextAnchor.MiddleLeft);
        SetRect(hud._levelNameText.rectTransform, new Vector2(0.035f, 0.89f), new Vector2(0.35f, 0.97f));
        hud._levelNameText.color = new Color(0.95f, 0.93f, 0.82f);

        GameObject pauseObject = UIObject("PauseButton", root.transform);
        SetRect(pauseObject.GetComponent<RectTransform>(), new Vector2(0.83f, 0.89f), new Vector2(0.965f, 0.97f));
        Image background = pauseObject.AddComponent<Image>();
        background.color = new Color(0.08f, 0.12f, 0.14f, 0.88f);
        Button pauseButton = pauseObject.AddComponent<Button>();
        pauseButton.targetGraphic = background;
        pauseButton.onClick.AddListener(() =>
        {
            if (GameManager.Instance != null) GameManager.Instance.PauseGame();
        });

        Text label = TextObject("Text", pauseObject.transform, font, 20, TextAnchor.MiddleCenter);
        label.text = "暂停  ESC";
        Stretch(label.rectTransform);
        return hud;
    }

    private static GameObject UIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    private static Text TextObject(string name, Transform parent, Font font, int size, TextAnchor alignment)
    {
        Text text = UIObject(name, parent).AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }

    private static void SetRect(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
    }
}
