using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

/// <summary>
/// Rebuilds levels 2-4 from the whiteboard layouts while reusing the working
/// gameplay objects and presentation setup from Level_1.
/// </summary>
public static class WhiteboardLevelBuilder
{
    private const string TemplateScene = "Assets/Scenes/Level_1.unity";
    private const string CabinetPrefab = "Assets/Prefab/HidingCabinet.prefab";
    private const string SquareTile = "Assets/newCjr/testTile/tiles/Square.asset";

    private static readonly Color FloorColor = new Color32(47, 50, 59, 255);
    private static readonly Color WallColor = new Color32(18, 20, 27, 255);

    [MenuItem("Tools/Levels/Build Whiteboard Levels 2-4")]
    public static void BuildAll()
    {
        BuildLevel2();
        BuildLevel3();
        BuildLevel4();
        UpdateLevelCatalog();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Built whiteboard levels 2-4.");
    }

    public static void BuildFromCommandLine()
    {
        BuildAll();
    }

    private static void BuildLevel2()
    {
        string[] map =
        {
            "###############",
            "####.......####",
            "####.......####",
            "######.########",
            "######.########",
            "#####...#######",
            "######.########",
            "######.########",
            "#####...#######",
            "######.########",
            "######.########",
            "####....#######",
            "####....#######",
            "######.########",
            "######.########",
            "#####...#######",
            "######.########",
            "######.########",
            "#####...#######",
            "######.########",
            "######.########",
            "#####...#######",
            "######.########",
            "######.########",
            "#####...#######",
            "######.########",
            "######.########",
            "#####...#######",
            "###############",
        };

        LevelContext context = BeginLevel("Assets/Scenes/Level_2.unity", map, "第二关 · 安全区教学");
        SetPlayerSpawn(context, 6, 2);
        AddFinish(context, 6, 26);
        AddDoor(context, "D0", 6, 5, "K0");
        AddKey(context, "K0", 5, 18, "K0");
        AddCabinet(context, "L0", 4.8f, 13);
        AddMonsterEncounter(context, "Mes0", 6, 11, 5, 25);
        AddMonsterEncounter(context, "Mes1", 6, 23, 7, 26);
        FinishLevel(context);
    }

    private static void BuildLevel3()
    {
        string[] map =
        {
            "#####################",
            "#######.......#######",
            "#######.......#######",
            "##########.##########",
            "##########.##########",
            "##....##.....##....##",
            "##....##.....##....##",
            "##.................##",
            "##....##.....##....##",
            "##....##.....##....##",
            "#####.####.####.#####",
            "##....##.....##....##",
            "##....##.....##....##",
            "##.................##",
            "##....##.....##....##",
            "##....##.....##....##",
            "#####.####.####.#####",
            "##....##.....##....##",
            "##....##.....##....##",
            "##.................##",
            "##....##.....##....##",
            "##....##.....##....##",
            "##########.##########",
            "##########.##########",
            "#########...#########",
            "#########...#########",
            "#####################",
        };

        LevelContext context = BeginLevel("Assets/Scenes/Level_3.unity", map, "第三关 · 钥匙回路");
        SetPlayerSpawn(context, 10, 2);
        AddFinish(context, 10, 25);

        AddKey(context, "K0", 16, 6, "K0");
        AddDoor(context, "D0", 14, 7, "K0");
        AddKey(context, "K1", 16, 18, "K1");
        AddDoor(context, "D1", 14, 19, "K1");
        AddKey(context, "K2", 4, 7, "K2");
        AddDoor(context, "D2", 6, 7, "K2");
        AddKey(context, "K3", 16, 12, "K3");
        AddDoor(context, "D3", 14, 13, "K3");
        AddKey(context, "K4", 4, 12, "K4");
        AddDoor(context, "D4", 6, 13, "K4");
        AddKey(context, "K5", 4, 19, "K5");
        AddDoor(context, "D5", 6, 19, "K5");

        AddCabinet(context, "H0", 9, 8);
        AddCabinet(context, "H1", 11, 14);
        AddCabinet(context, "H2", 9, 20);
        AddMonsterEncounter(context, "Mes0", 10, 9, 3, 6);
        AddMonsterEncounter(context, "Mes1", 10, 15, 17, 6);
        AddMonsterEncounter(context, "Mes2", 10, 21, 3, 20);
        FinishLevel(context);
    }

    private static void BuildLevel4()
    {
        string[] map =
        {
            "#########################",
            "#########.......#########",
            "#########.......#########",
            "####.................####",
            "####.###.#####.###.##.###",
            "####.....#...#.....#...##",
            "######.###.#.#####.###.##",
            "####...#...#.....#.....##",
            "####.###.#####.#.#####.##",
            "####.....#.....#.....#.##",
            "####.#####.#######.#.#.##",
            "####.#.....#.....#.#...##",
            "####.#.#####.###.#.###.##",
            "####.#.....#.#...#.....##",
            "####.#####.#.#.#######.##",
            "####.....#...#.......#.##",
            "######.#.#########.#.#.##",
            "####...#.....#.....#...##",
            "####.#######.#.#######.##",
            "####.........#.........##",
            "###########...###########",
            "###########...###########",
            "#########################",
        };

        LevelContext context = BeginLevel("Assets/Scenes/Level_4.unity", map, "第四关 · 藏身迷宫");
        SetPlayerSpawn(context, 12, 2);
        AddFinish(context, 12, 21);
        AddKey(context, "K0", 20, 5, "K0");
        AddDoor(context, "D0", 20, 9, "K0");
        AddKey(context, "K1", 5, 3, "K1");
        AddDoor(context, "D1", 8, 3, "K1");
        AddKey(context, "K2", 22, 7, "K2");
        AddDoor(context, "D2", 22, 13, "K2");

        AddCabinet(context, "H0", 10, 5);
        AddCabinet(context, "H1", 6, 9);
        AddCabinet(context, "H2", 14, 13);
        AddCabinet(context, "H3", 20, 17);
        AddCabinet(context, "H4", 10, 19);

        AddMonsterEncounter(context, "Mes0", 12, 7, 4, 5);
        AddMonsterEncounter(context, "Mes1", 18, 11, 21, 5);
        AddMonsterEncounter(context, "Mes2", 8, 15, 5, 17);
        AddMonsterEncounter(context, "Mes3", 16, 19, 21, 19);
        FinishLevel(context);
    }

    private static LevelContext BeginLevel(string targetPath, string[] map, string title)
    {
        ValidateMap(map);
        Scene scene = EditorSceneManager.OpenScene(TemplateScene, OpenSceneMode.Single);
        if (!EditorSceneManager.SaveScene(scene, targetPath))
            throw new InvalidOperationException("Unable to save scene: " + targetPath);

        LevelContext context = new LevelContext
        {
            Scene = scene,
            Map = map,
            Width = map[0].Length,
            Height = map.Length,
            Player = FindRequired("Player"),
            Origin = FindRequired("oriPos"),
            Manager = UnityEngine.Object.FindObjectOfType<MiniSceneManager>(true),
            KeyTemplate = UnityEngine.Object.FindObjectOfType<Key>(true),
            DoorTemplate = UnityEngine.Object.FindObjectOfType<Door>(true),
            MonsterTemplate = UnityEngine.Object.FindObjectOfType<MonsterChase>(true),
            TriggerTemplate = UnityEngine.Object.FindObjectOfType<MonsterTrigger>(true),
            FinishTemplate = UnityEngine.Object.FindObjectOfType<Finish>(true)
        };

        context.Root = new GameObject(title);
        context.Root.transform.position = new Vector3(-(context.Width - 1) * 0.5f, (context.Height - 1) * 0.5f, 0f);

        DeleteOldGameplay(context);
        BuildMap(context);
        FitCamera(context);
        return context;
    }

    private static void DeleteOldGameplay(LevelContext context)
    {
        foreach (Key item in UnityEngine.Object.FindObjectsOfType<Key>(true))
            if (item != context.KeyTemplate) UnityEngine.Object.DestroyImmediate(item.gameObject);
        foreach (Door item in UnityEngine.Object.FindObjectsOfType<Door>(true))
            if (item != context.DoorTemplate) UnityEngine.Object.DestroyImmediate(item.gameObject);
        foreach (MonsterChase item in UnityEngine.Object.FindObjectsOfType<MonsterChase>(true))
            if (item != context.MonsterTemplate) UnityEngine.Object.DestroyImmediate(item.gameObject);
        foreach (MonsterTrigger item in UnityEngine.Object.FindObjectsOfType<MonsterTrigger>(true))
            if (item != context.TriggerTemplate) UnityEngine.Object.DestroyImmediate(item.gameObject);
        foreach (Finish item in UnityEngine.Object.FindObjectsOfType<Finish>(true))
            if (item != context.FinishTemplate) UnityEngine.Object.DestroyImmediate(item.gameObject);
        foreach (HidingCabinet item in UnityEngine.Object.FindObjectsOfType<HidingCabinet>(true))
            UnityEngine.Object.DestroyImmediate(item.gameObject);
        foreach (Tilemap item in UnityEngine.Object.FindObjectsOfType<Tilemap>(true))
            UnityEngine.Object.DestroyImmediate(item.gameObject);
    }

    private static void BuildMap(LevelContext context)
    {
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(SquareTile);
        if (tile == null) throw new InvalidOperationException("Missing square Tile asset.");

        GameObject gridObject = new GameObject("Grid · Level Tilemaps");
        gridObject.transform.SetParent(context.Root.transform, false);
        Grid grid = gridObject.AddComponent<Grid>();
        grid.cellSize = Vector3.one;

        Tilemap floor = CreateTilemap(gridObject.transform, "Ground", -20, false);
        Tilemap walls = CreateTilemap(gridObject.transform, "Walls · Collision", -10, true);
        for (int y = 0; y < context.Height; y++)
        {
            for (int x = 0; x < context.Width; x++)
            {
                Vector3Int cell = new Vector3Int(x, -y, 0);
                floor.SetTile(cell, tile);
                floor.SetColor(cell, FloorColor);
                if (context.Map[y][x] != '#') continue;
                walls.SetTile(cell, tile);
                walls.SetColor(cell, WallColor);
            }
        }

        floor.CompressBounds();
        walls.CompressBounds();
    }

    private static Tilemap CreateTilemap(Transform parent, string name, int sortingOrder, bool collision)
    {
        GameObject tilemapObject = new GameObject(name);
        tilemapObject.transform.SetParent(parent, false);
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer renderer = tilemapObject.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = sortingOrder;
        if (!collision) return tilemap;

        TilemapCollider2D tilemapCollider = tilemapObject.AddComponent<TilemapCollider2D>();
        Rigidbody2D body = tilemapObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Static;
        CompositeCollider2D composite = tilemapObject.AddComponent<CompositeCollider2D>();
        composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
        tilemapCollider.usedByComposite = true;
        return tilemap;
    }

    private static void SetPlayerSpawn(LevelContext context, int x, int y)
    {
        Vector3 position = Cell(context, x, y);
        context.Player.transform.position = position;
        context.Origin.transform.position = position;
        context.Player.name = "P · Player Spawn";
        SerializedObject manager = new SerializedObject(context.Manager);
        manager.FindProperty("oriPostion").objectReferenceValue = context.Origin.transform;
        manager.FindProperty("Player").objectReferenceValue = context.Player.transform;
        manager.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddKey(LevelContext context, string name, int x, int y, string id)
    {
        Key key = UnityEngine.Object.Instantiate(context.KeyTemplate);
        key.gameObject.SetActive(true);
        key.name = name + " · Key";
        key.transform.position = Cell(context, x, y);
        SetString(key, "keyId", id);
    }

    private static void AddDoor(LevelContext context, string name, int x, int y, string id)
    {
        Door door = UnityEngine.Object.Instantiate(context.DoorTemplate);
        door.gameObject.SetActive(true);
        door.name = name + " · Door";
        door.transform.position = Cell(context, x, y);
        SetString(door, "keyId", id);
    }

    private static void AddFinish(LevelContext context, int x, int y)
    {
        Finish finish = UnityEngine.Object.Instantiate(context.FinishTemplate);
        finish.gameObject.SetActive(true);
        finish.name = "EXIT · Finish";
        finish.transform.position = Cell(context, x, y);
    }

    private static void AddCabinet(LevelContext context, string name, float x, float y)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CabinetPrefab);
        GameObject cabinet = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (cabinet == null) throw new InvalidOperationException("Unable to instantiate hiding cabinet.");
        cabinet.name = name + " · Hiding Cabinet";
        cabinet.transform.position = Cell(context, x, y);
    }

    private static void AddMonsterEncounter(LevelContext context, string name, int triggerX, int triggerY,
        int monsterX, int monsterY)
    {
        MonsterChase monster = UnityEngine.Object.Instantiate(context.MonsterTemplate);
        monster.gameObject.SetActive(true);
        monster.name = name + " · Monster";
        monster.transform.position = Cell(context, monsterX, monsterY);
        SerializedObject monsterData = new SerializedObject(monster);
        monsterData.FindProperty("target").objectReferenceValue = context.Player.transform;
        monsterData.ApplyModifiedPropertiesWithoutUndo();

        MonsterTrigger trigger = UnityEngine.Object.Instantiate(context.TriggerTemplate);
        trigger.gameObject.SetActive(true);
        trigger.name = name + " · Trigger";
        trigger.transform.position = Cell(context, triggerX, triggerY);
        SerializedObject triggerData = new SerializedObject(trigger);
        triggerData.FindProperty("monster").objectReferenceValue = monster;
        triggerData.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void FinishLevel(LevelContext context)
    {
        ConfigurePathfinding(context);
        UnityEngine.Object.DestroyImmediate(context.KeyTemplate.gameObject);
        UnityEngine.Object.DestroyImmediate(context.DoorTemplate.gameObject);
        UnityEngine.Object.DestroyImmediate(context.MonsterTemplate.gameObject);
        UnityEngine.Object.DestroyImmediate(context.TriggerTemplate.gameObject);
        UnityEngine.Object.DestroyImmediate(context.FinishTemplate.gameObject);
        EditorSceneManager.MarkSceneDirty(context.Scene);
        if (!EditorSceneManager.SaveScene(context.Scene))
            throw new InvalidOperationException("Unable to save " + context.Scene.path);
    }

    private static void ConfigurePathfinding(LevelContext context)
    {
        AstarPath astar = UnityEngine.Object.FindObjectOfType<AstarPath>(true);
        if (astar == null || astar.data == null || astar.data.gridGraph == null) return;

        GridGraph graph = astar.data.gridGraph;
        graph.center = context.Root.transform.TransformPoint(
            new Vector3((context.Width - 1) * 0.5f, -(context.Height - 1) * 0.5f, 0));
        graph.SetDimensions(context.Width + 2, context.Height + 2, 1f);
        EditorUtility.SetDirty(astar);
    }

    private static void FitCamera(LevelContext context)
    {
        Camera camera = Camera.main;
        if (camera != null) camera.orthographicSize = Mathf.Max(8f, context.Height * 0.34f);
    }

    private static Vector3 Cell(LevelContext context, float x, float y)
    {
        return context.Root.transform.TransformPoint(new Vector3(x, -y, 0));
    }

    private static void SetString(UnityEngine.Object target, string propertyName, string value)
    {
        SerializedObject data = new SerializedObject(target);
        data.FindProperty(propertyName).stringValue = value;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject FindRequired(string name)
    {
        GameObject result = SceneManager.GetActiveScene().GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Select(transform => transform.gameObject)
            .FirstOrDefault(item => item.name == name);
        if (result == null) throw new InvalidOperationException("Template object missing: " + name);
        return result;
    }

    private static void ValidateMap(IReadOnlyList<string> map)
    {
        if (map.Count == 0) throw new ArgumentException("Map cannot be empty.");
        int width = map[0].Length;
        if (map.Any(row => row.Length != width))
            throw new ArgumentException("All map rows must have equal width.");
    }

    private static void UpdateLevelCatalog()
    {
        LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>("Assets/Resources/Data/LevelCatalog.asset");
        SerializedObject data = new SerializedObject(catalog);
        SerializedProperty levels = data.FindProperty("levels");
        string[] names = { "第一关", "第二关", "第三关", "第四关" };
        string[] descriptions =
        {
            "熟悉场景与基础操作",
            "安全区教学：学习机关、怪物与藏身",
            "钥匙回路：规划多组钥匙与门的路线",
            "藏身迷宫：在追逐中寻找出口"
        };

        levels.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            SerializedProperty level = levels.GetArrayElementAtIndex(i);
            level.FindPropertyRelative("id").stringValue = $"level_{i + 1:00}";
            level.FindPropertyRelative("displayName").stringValue = names[i];
            level.FindPropertyRelative("description").stringValue = descriptions[i];
            level.FindPropertyRelative("sceneName").stringValue = $"Level_{i + 1}";
        }
        data.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
    }

    private static void UpdateBuildSettings()
    {
        string[] paths =
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Level_1.unity",
            "Assets/Scenes/Level_2.unity",
            "Assets/Scenes/Level_3.unity",
            "Assets/Scenes/Level_4.unity"
        };
        EditorBuildSettings.scenes = paths.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
    }

    private sealed class LevelContext
    {
        public Scene Scene;
        public string[] Map;
        public int Width;
        public int Height;
        public GameObject Root;
        public GameObject Player;
        public GameObject Origin;
        public MiniSceneManager Manager;
        public Key KeyTemplate;
        public Door DoorTemplate;
        public MonsterChase MonsterTemplate;
        public MonsterTrigger TriggerTemplate;
        public Finish FinishTemplate;
    }
}
