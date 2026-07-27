using UnityEngine;
using UnityEngine.UI;

/// <summary>Blocks the outside world while leaving a small cabinet-view opening.</summary>
public class HideoutVisionMask : MonoBehaviour
{
    [SerializeField, Range(0.05f, 0.8f)] private float openingWidth = 0.28f;
    [SerializeField, Range(0.03f, 0.5f)] private float openingHeight = 0.14f;
    [SerializeField] private Color maskColor = Color.black;

    private Canvas canvas;

    private void Awake()
    {
        BuildMask();
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        if (canvas == null) BuildMask();
        canvas.enabled = visible;
    }

    private void BuildMask()
    {
        canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        if (GetComponent<CanvasScaler>() == null)
            gameObject.AddComponent<CanvasScaler>();
        if (transform.childCount > 0) return;

        float left = (1f - openingWidth) * 0.5f;
        float right = 1f - left;
        float bottom = (1f - openingHeight) * 0.5f;
        float top = 1f - bottom;

        CreatePanel("Left", new Vector2(0f, 0f), new Vector2(left, 1f));
        CreatePanel("Right", new Vector2(right, 0f), new Vector2(1f, 1f));
        CreatePanel("Top", new Vector2(left, top), new Vector2(right, 1f));
        CreatePanel("Bottom", new Vector2(left, 0f), new Vector2(right, bottom));
    }

    private void CreatePanel(string panelName, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = new GameObject(panelName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);
        RectTransform rect = (RectTransform)panel.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = panel.GetComponent<Image>();
        image.color = maskColor;
        image.raycastTarget = false;
    }
}
