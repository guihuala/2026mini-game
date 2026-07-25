using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class ConvertProjectToTopDown2D
{
    private const string GameplayScenePath = "Assets/Scenes/PaperDiorama.unity";

    public static void Run()
    {
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        Physics2D.gravity = Vector2.zero;

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateWorld();
        CreateSystems();
        CreateEventSystem();

        EditorSceneManager.SaveScene(scene, GameplayScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Converted gameplay scene to a clean top-down 2D foundation.");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 9f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color32(24, 27, 36, 255);
        camera.nearClipPlane = -20f;
        camera.farClipPlane = 100f;
        camera.allowHDR = false;
        camera.allowMSAA = false;

        cameraObject.AddComponent<AudioListener>();
    }

    private static void CreateWorld()
    {
        GameObject world = new GameObject("WORLD - TOP DOWN 2D");

        GameObject gridObject = new GameObject("Grid");
        gridObject.transform.SetParent(world.transform);
        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellLayout = GridLayout.CellLayout.Rectangle;
        grid.cellSize = Vector3.one;

        CreateTilemap(gridObject.transform, "Background", -30);
        CreateTilemap(gridObject.transform, "Ground", -20);
        CreateTilemap(gridObject.transform, "Collision", -10, true);
        CreateTilemap(gridObject.transform, "Foreground", 30);

        CreateMarker(world.transform, "Characters");
        CreateMarker(world.transform, "Interactables");
        CreateMarker(world.transform, "Effects");
        CreateMarker(world.transform, "Runtime");
    }

    private static void CreateTilemap(Transform parent, string name, int sortingOrder, bool collision = false)
    {
        GameObject tilemapObject = new GameObject(name);
        tilemapObject.transform.SetParent(parent);
        tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;

        if (collision)
        {
            TilemapCollider2D collider = tilemapObject.AddComponent<TilemapCollider2D>();
            collider.usedByComposite = true;
            Rigidbody2D body = tilemapObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Static;
            tilemapObject.AddComponent<CompositeCollider2D>();
        }
    }

    private static void CreateMarker(Transform parent, string name)
    {
        GameObject marker = new GameObject(name);
        marker.transform.SetParent(parent);
    }

    private static void CreateSystems()
    {
        GameObject systems = new GameObject("SYSTEMS");
        GameObject managerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefab/-----MANAGER-----.prefab");
        if (managerPrefab == null) return;

        GameObject managers = PrefabUtility.InstantiatePrefab(managerPrefab) as GameObject;
        if (managers != null)
        {
            managers.name = "Managers";
            managers.transform.SetParent(systems.transform);
        }
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
    }
}
