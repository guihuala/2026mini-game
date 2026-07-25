#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class LevelSelectSetup
{
    private const string FontPath = "Assets/Art/Font/Cubic_11_1.010_R.ttf";

    [MenuItem("Tools/Game/Build Level Select")]
    public static void Build()
    {
        BuildCatalog();
        BuildPanel();
        RegisterPanel();
        UpdateMainMenu();
        UpdateBuildScenes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Level select page and catalog built.");
    }

    private static void BuildCatalog()
    {
        const string path = "Assets/Resources/Data/LevelCatalog.asset";
        LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(path);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            AssetDatabase.CreateAsset(catalog, path);
        }

        SerializedProperty levels = new SerializedObject(catalog).FindProperty("levels");
        levels.serializedObject.Update();
        levels.arraySize = 2;
        SetLevel(levels.GetArrayElementAtIndex(0), "level_01", "第一关", "熟悉场景与基础操作", "test_cjr");
        SetLevel(levels.GetArrayElementAtIndex(1), "level_02", "第二关", "完成第一关后解锁", "viewtest");
        levels.serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
    }

    private static void SetLevel(SerializedProperty level, string id, string name, string description, string scene)
    {
        level.FindPropertyRelative("id").stringValue = id;
        level.FindPropertyRelative("displayName").stringValue = name;
        level.FindPropertyRelative("description").stringValue = description;
        level.FindPropertyRelative("sceneName").stringValue = scene;
    }

    private static void BuildPanel()
    {
        Font font = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
        GameObject root = UIObject("LevelSelectPanel", null);
        Stretch(root.GetComponent<RectTransform>());
        Image backdrop = root.AddComponent<Image>();
        backdrop.color = new Color(0.04f, 0.055f, 0.07f, 0.96f);
        root.AddComponent<CanvasGroup>();
        LevelSelectPanel panel = root.AddComponent<LevelSelectPanel>();

        GameObject content = UIObject("Content", root.transform);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.12f, 0.1f);
        contentRect.anchorMax = new Vector2(0.88f, 0.9f);
        contentRect.offsetMin = contentRect.offsetMax = Vector2.zero;

        Text heading = TextObject("Title", content.transform, font, "选择关卡", 44, TextAnchor.MiddleLeft);
        SetRect(heading.rectTransform, new Vector2(0f, 0.86f), new Vector2(0.75f, 1f));

        Button back = ButtonObject("BackButton", content.transform, font, "返回");
        SetRect(back.GetComponent<RectTransform>(), new Vector2(0.82f, 0.88f), new Vector2(1f, 0.98f));

        GameObject viewport = UIObject("LevelList", content.transform);
        SetRect(viewport.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0.82f));
        VerticalLayoutGroup layout = viewport.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 18f;
        layout.childControlHeight = false;
        layout.childForceExpandHeight = false;
        layout.childControlWidth = true;

        LevelSelectItem item = BuildItem(font);
        PrefabUtility.SaveAsPrefabAsset(item.gameObject, "Assets/Resources/Panel/LevelSelectItem.prefab");
        Object.DestroyImmediate(item.gameObject);
        LevelSelectItem itemPrefab = AssetDatabase.LoadAssetAtPath<LevelSelectItem>("Assets/Resources/Panel/LevelSelectItem.prefab");

        Text empty = TextObject("EmptyText", content.transform, font, "暂无可用关卡", 26, TextAnchor.MiddleCenter);
        SetRect(empty.rectTransform, new Vector2(0f, 0.3f), new Vector2(1f, 0.6f));

        SerializedObject so = new SerializedObject(panel);
        so.FindProperty("levelContainer").objectReferenceValue = viewport.transform;
        so.FindProperty("levelItemPrefab").objectReferenceValue = itemPrefab;
        so.FindProperty("backButton").objectReferenceValue = back;
        so.FindProperty("emptyText").objectReferenceValue = empty;
        so.FindProperty("animationContent").objectReferenceValue = contentRect;
        so.FindProperty("backdropGraphic").objectReferenceValue = backdrop;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, "Assets/Resources/Panel/LevelSelectPanel.prefab");
        Object.DestroyImmediate(root);
    }

    private static LevelSelectItem BuildItem(Font font)
    {
        GameObject root = UIObject("LevelSelectItem", null);
        root.AddComponent<LayoutElement>().preferredHeight = 128f;
        Image image = root.AddComponent<Image>();
        image.color = new Color(0.12f, 0.16f, 0.19f, 1f);
        Button button = root.AddComponent<Button>();
        button.targetGraphic = image;
        Text number = TextObject("Number", root.transform, font, "01", 34, TextAnchor.MiddleCenter);
        SetRect(number.rectTransform, new Vector2(0f, 0f), new Vector2(0.14f, 1f));
        Text title = TextObject("Title", root.transform, font, "关卡", 28, TextAnchor.LowerLeft);
        SetRect(title.rectTransform, new Vector2(0.17f, 0.48f), new Vector2(0.78f, 0.9f));
        Text description = TextObject("Description", root.transform, font, "", 20, TextAnchor.UpperLeft);
        description.color = new Color(0.68f, 0.75f, 0.78f);
        SetRect(description.rectTransform, new Vector2(0.17f, 0.1f), new Vector2(0.78f, 0.48f));
        Text locked = TextObject("Lock", root.transform, font, "未解锁", 22, TextAnchor.MiddleCenter);
        SetRect(locked.rectTransform, new Vector2(0.8f, 0f), new Vector2(1f, 1f));
        LevelSelectItem item = root.AddComponent<LevelSelectItem>();
        SerializedObject so = new SerializedObject(item);
        so.FindProperty("button").objectReferenceValue = button;
        so.FindProperty("numberText").objectReferenceValue = number;
        so.FindProperty("titleText").objectReferenceValue = title;
        so.FindProperty("descriptionText").objectReferenceValue = description;
        so.FindProperty("lockIcon").objectReferenceValue = locked.gameObject;
        so.ApplyModifiedPropertiesWithoutUndo();
        return item;
    }

    private static void RegisterPanel()
    {
        UIDatas data = AssetDatabase.LoadAssetAtPath<UIDatas>("Assets/Resources/Data/UIDataListSO.asset");
        if (data == null) return;
        if (data.uiDataList == null) data.uiDataList = new List<UIData>();
        if (!data.uiDataList.Exists(entry => entry.uiName == "LevelSelectPanel"))
            data.uiDataList.Add(new UIData { uiName = "LevelSelectPanel", uiPath = "Panel/LevelSelectPanel" });
        EditorUtility.SetDirty(data);
    }

    private static void UpdateMainMenu()
    {
        const string scenePath = "Assets/Scenes/MainMenu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        GameObject continueButton = GameObject.Find("ContinueGame");
        if (continueButton != null) Object.DestroyImmediate(continueButton);

        GameObject startButton = GameObject.Find("NewGame");
        if (startButton != null)
        {
            Text label = startButton.GetComponentInChildren<Text>(true);
            if (label != null) label.text = "开始游戏";
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void UpdateBuildScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/test_cjr.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/viewtest.unity", true)
        };
    }

    private static GameObject UIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    private static Text TextObject(string name, Transform parent, Font font, string value, int size, TextAnchor alignment)
    {
        GameObject go = UIObject(name, parent);
        Text text = go.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        return text;
    }

    private static Button ButtonObject(string name, Transform parent, Font font, string label)
    {
        GameObject go = UIObject(name, parent);
        Image image = go.AddComponent<Image>();
        image.color = new Color(0.21f, 0.35f, 0.38f);
        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        Text text = TextObject("Text", go.transform, font, label, 22, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform);
        return button;
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
#endif
