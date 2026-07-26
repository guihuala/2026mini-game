using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class FixedPathMonsterExampleBuilder
{
    private const string ScenePath = "Assets/Scenes/FixedPathMonsterExample.unity";
    private const string LineMaterialPath = "Assets/Scenes/FixedPathMonsterExampleLine.mat";

    [MenuItem("Tools/Examples/Rebuild Fixed Path Monster Scene")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        Transform pathRoot = CreatePath();
        CreateMonster(pathRoot);
        CreateInstructions();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Debug.Log($"固定路径怪物示例场景已生成：{ScenePath}");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.5f;
        camera.backgroundColor = new Color(0.07f, 0.09f, 0.12f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static Transform CreatePath()
    {
        GameObject root = new GameObject("Fixed Path (按顺序放置路径点)");
        Vector2[] positions =
        {
            new Vector2(-4f, 2.5f),
            new Vector2(4f, 2.5f),
            new Vector2(4f, -2.5f),
            new Vector2(-4f, -2.5f)
        };

        LineRenderer line = root.AddComponent<LineRenderer>();
        line.loop = true;
        line.useWorldSpace = true;
        line.positionCount = positions.Length;
        line.startWidth = 0.06f;
        line.endWidth = 0.06f;
        line.startColor = new Color(0.2f, 0.9f, 0.55f, 0.55f);
        line.endColor = line.startColor;
        line.sharedMaterial = GetOrCreateLineMaterial();

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject point = CreateColoredSquare(
                $"Path Point {i + 1}",
                positions[i],
                new Vector2(0.3f, 0.3f),
                new Color(0.2f, 0.95f, 0.55f));
            point.transform.SetParent(root.transform);
            line.SetPosition(i, positions[i]);
        }

        return root.transform;
    }

    private static Material GetOrCreateLineMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(LineMaterialPath);
        if (material != null)
            return material;

        material = new Material(Shader.Find("Sprites/Default"));
        AssetDatabase.CreateAsset(material, LineMaterialPath);
        return material;
    }

    private static void CreateMonster(Transform pathRoot)
    {
        GameObject monster = CreateColoredSquare(
            "Fixed Path Monster (橙色)",
            new Vector2(-4f, 2.5f),
            new Vector2(0.75f, 0.75f),
            new Color(1f, 0.38f, 0.08f));

        BoxCollider2D collider = monster.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;

        Rigidbody2D body = monster.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;

        monster.AddComponent<Monster>();
        FixedPathMonster movement = monster.AddComponent<FixedPathMonster>();

        SerializedObject serializedMovement = new SerializedObject(movement);
        SerializedProperty points = serializedMovement.FindProperty("pathPoints");
        points.arraySize = pathRoot.childCount;
        for (int i = 0; i < pathRoot.childCount; i++)
            points.GetArrayElementAtIndex(i).objectReferenceValue = pathRoot.GetChild(i);

        serializedMovement.FindProperty("pathMode").enumValueIndex =
            (int)FixedPathMonster.PathMode.Loop;
        serializedMovement.FindProperty("speed").floatValue = 2f;
        serializedMovement.FindProperty("waitAtPoint").floatValue = 0.25f;
        serializedMovement.FindProperty("playOnStart").boolValue = true;
        serializedMovement.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateInstructions()
    {
        GameObject canvasObject = new GameObject("使用说明");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject = new GameObject("说明文字");
        textObject.transform.SetParent(canvasObject.transform, false);
        Text text = textObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 22;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.text =
            "固定路径怪物示例\n" +
            "橙色方块会沿绿色路径循环移动\n" +
            "选中怪物，在 Fixed Path Monster 组件中调整模式、速度和路径点";

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(24f, -24f);
        rect.sizeDelta = new Vector2(760f, 120f);
    }

    private static GameObject CreateColoredSquare(
        string objectName,
        Vector2 position,
        Vector2 size,
        Color color)
    {
        GameObject gameObject = new GameObject(objectName);
        gameObject.transform.position = position;
        gameObject.transform.localScale = size;

        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = color;
        return gameObject;
    }
}
