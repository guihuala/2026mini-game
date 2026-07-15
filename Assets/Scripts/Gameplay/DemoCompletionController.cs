using UnityEngine;

public sealed class DemoCompletionController : MonoBehaviour
{
    private bool visible;
    private GUIStyle titleStyle;
    private GUIStyle bodyStyle;
    private GUIStyle buttonStyle;

    public bool IsVisible => visible;

    private void Awake()
    {
        titleStyle = new GUIStyle
        {
            fontSize = 34,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        bodyStyle = new GUIStyle
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            normal = { textColor = new Color(.9f, .92f, .95f) }
        };
    }

    public void Show()
    {
        if (visible) return;
        visible = true;
        ExplorationControlLock.Acquire(this);
        DialogueRuntimeState.SetFlag("greybox.demo_complete", true);
    }

    private void OnGUI()
    {
        if (!visible) return;
        if (buttonStyle == null) buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 19 };
        GUI.color = new Color(0f, 0f, 0f, .72f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        const float width = 620f;
        const float height = 350f;
        Rect panel = new Rect((Screen.width - width) * .5f, (Screen.height - height) * .5f, width, height);
        GUI.Box(panel, string.Empty);
        GUI.Label(new Rect(panel.x + 30, panel.y + 42, width - 60, 58), "Demo 流程已结束", titleStyle);
        GUI.Label(new Rect(panel.x + 55, panel.y + 115, width - 110, 80),
            "感谢体验当前版本。\n你的剧情进度和小游戏结果已经保存。", bodyStyle);

        if (GUI.Button(new Rect(panel.x + 80, panel.y + 245, 210, 52), "返回主菜单", buttonStyle))
        {
            visible = false;
            ExplorationControlLock.Release(this);
            if (GameManager.Instance != null) GameManager.Instance.ReturnToMainMenu();
            else if (SceneLoader.Instance != null) SceneLoader.Instance.LoadScene(GameScene.MainMenu);
        }
        if (GUI.Button(new Rect(panel.x + 330, panel.y + 245, 210, 52), "留在场景", buttonStyle))
        {
            visible = false;
            ExplorationControlLock.Release(this);
        }
    }

    private void OnDisable()
    {
        ExplorationControlLock.Release(this);
    }
}
