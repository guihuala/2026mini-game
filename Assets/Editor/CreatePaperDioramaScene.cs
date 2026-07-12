using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class CreatePaperDioramaScene
{
    const string ScenePath = "Assets/Scenes/PaperDiorama.unity";
    const string MaterialFolder = "Assets/Art/PaperDiorama";

    [MenuItem("Tools/2.5D/Create Paper Diorama Scene")]
    public static void CreateScene()
    {
        EnsureFolder();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var root = new GameObject("PAPER DIORAMA - 2.5D SAMPLE");
        CreateLighting(root.transform);
        CreateCamera(root.transform);
        CreateWorld(root.transform);

        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes = AppendScene(EditorBuildSettings.scenes, ScenePath);
        Selection.activeGameObject = root;
        Debug.Log("Created 2.5D paper diorama at " + ScenePath);
    }

    static void CreateCamera(Transform parent)
    {
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        go.transform.SetParent(parent);
        go.transform.position = new Vector3(12, 11, -16);
        go.transform.rotation = Quaternion.Euler(29, -36, 0);
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7.3f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.94f, 0.94f, 0.91f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 80;
        cam.allowHDR = true;
    }

    static void CreateLighting(Transform parent)
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.86f, 0.86f, 0.83f);
        RenderSettings.ambientEquatorColor = new Color(0.64f, 0.64f, 0.61f);
        RenderSettings.ambientGroundColor = new Color(0.36f, 0.36f, 0.34f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.94f, 0.94f, 0.91f);
        RenderSettings.fogStartDistance = 18;
        RenderSettings.fogEndDistance = 42;

        var sun = new GameObject("Warm Sun");
        sun.transform.SetParent(parent);
        sun.transform.rotation = Quaternion.Euler(42, -32, 0);
        var light = sun.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = Color.white;
        light.intensity = 1.15f;
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.72f;
    }

    static void CreateWorld(Transform parent)
    {
        var world = NewGroup("Layered World", parent);
        var grass = Mat("Grass", new Color(0.38f, 0.62f, 0.43f));
        var grassDark = Mat("GrassDark", new Color(0.22f, 0.47f, 0.34f));
        var soil = Mat("Soil", new Color(0.52f, 0.31f, 0.22f));
        var path = Mat("Path", new Color(0.88f, 0.72f, 0.49f));
        var water = Mat("Water", new Color(0.23f, 0.67f, 0.72f));
        var white = Mat("Cream", new Color(0.95f, 0.88f, 0.72f));
        var red = Mat("Coral", new Color(0.78f, 0.26f, 0.21f));
        var roof = Mat("Roof", new Color(0.25f, 0.20f, 0.28f));
        var trunk = Mat("Trunk", new Color(0.35f, 0.20f, 0.14f));
        var leaf = Mat("Leaf", new Color(0.15f, 0.42f, 0.29f));
        var leafLight = Mat("LeafLight", new Color(0.45f, 0.68f, 0.36f));
        var yellow = Mat("Yellow", new Color(0.96f, 0.66f, 0.20f));

        // Raised island: thin visible strata sell the paper-cut construction.
        Box("Shadow slab", world, new Vector3(0, -0.48f, 0), new Vector3(12.8f, 0.28f, 9.2f), soil);
        // The visible top is assembled from horizontal pixel-sprite cards; the thin soil slab below
        // remains 3D so the island still reads as a pop-up book with physical thickness.
        CreatePixelTileGround(world, new Vector3(0, -0.12f, 0), 13, 9, 1f);
        Box("Path sheet", world, new Vector3(0, -0.14f, 0.3f), new Vector3(1.55f, 0.06f, 8.1f), path, Quaternion.Euler(0, -8, 0));
        Box("Pond", world, new Vector3(-3.7f, -0.12f, 1.6f), new Vector3(3.0f, 0.05f, 2.4f), water, Quaternion.Euler(0, 12, 0));
        Box("Pond inset", world, new Vector3(-3.7f, -0.17f, 1.6f), new Vector3(3.35f, 0.08f, 2.75f), grassDark, Quaternion.Euler(0, 12, 0));

        CreateHouse(world, new Vector3(2.9f, 0, 1.0f), white, red, roof);
        CreateHouse(world, new Vector3(-2.5f, 0, -2.0f), white, yellow, roof, .78f);

        PixelTree(world, new Vector3(-4.7f, 0, -2.4f), 0, 1.55f, trunk, leaf, leafLight);
        PixelTree(world, new Vector3(4.7f, 0, -2.4f), 1, 1.3f, trunk, leaf, leafLight);
        PixelTree(world, new Vector3(4.7f, 0, 3.1f), 0, 1.45f, trunk, leaf, leafLight);
        PixelTree(world, new Vector3(-1.1f, 0, 3.1f), 2, 1.15f, trunk, leaf, leafLight);

        // Repeated vertical cards add foreground/midground rhythm.
        for (int i = 0; i < 9; i++)
        {
            float x = -5.2f + i * 1.25f;
            PixelDecoration(world, "Assets/Art/Vegetation/Trees 3.png", "Trees 3_" + (2 + i % 2),
                "Pixel shrub", new Vector3(x, -.08f, 4.0f + Mathf.Sin(i) * .16f), .65f + (i % 3) * .08f, true);
        }

        for (int i = 0; i < 8; i++)
        {
            PixelDecoration(world, "Assets/Art/tilesets/Set 1.1.png", "Set 1.1_" + (83 + i % 3),
                "Pixel flower decal", new Vector3(-1.8f + i * .52f, -.095f, -.75f + Mathf.Sin(i * 2.2f) * .28f), .9f, false);
        }

        // A short run of sprite fence cards demonstrates fixed-orientation props.
        for (int i = 0; i < 7; i++)
            PixelDecoration(world, "Assets/Art/tilesets/fences and ladders etc.png", "fences and ladders etc_" + (14 + i % 5),
                "Pixel fence", new Vector3(1.7f + i * .48f, -.08f, -3.55f), 1f, true, Quaternion.Euler(0, 145f, 0));
    }

    static void CreateHouse(Transform parent, Vector3 pos, Material wall, Material accent, Material roof, float scale = 1f)
    {
        var house = NewGroup("Layered paper house", parent);
        house.localPosition = pos;
        house.localScale = Vector3.one * scale;
        Box("Wall slab", house, new Vector3(0, .72f, 0), new Vector3(2.25f, 1.55f, .42f), wall);
        Box("Door", house, new Vector3(.45f, .55f, -.24f), new Vector3(.5f, 1.0f, .06f), accent);
        Box("Window L", house, new Vector3(-.62f, .78f, -.24f), new Vector3(.48f, .48f, .06f), accent);
        Box("Window inset", house, new Vector3(-.62f, .78f, -.28f), new Vector3(.25f, .25f, .04f), wall);
        var roofGo = Box("Roof card", house, new Vector3(0, 1.72f, 0), new Vector3(2.7f, 1.8f, .18f), roof, Quaternion.Euler(0, 0, 45));
        roofGo.transform.localScale = new Vector3(1, .55f, 1);
        Box("Chimney", house, new Vector3(.68f, 2.0f, .03f), new Vector3(.28f, .72f, .30f), accent);
    }

    static void Tree(Transform parent, Vector3 pos, Material trunk, Material dark, Material light, float scale)
    {
        var tree = NewGroup("Cross-card tree", parent);
        tree.localPosition = pos;
        tree.localScale = Vector3.one * scale;
        Box("Trunk card", tree, new Vector3(0, .65f, 0), new Vector3(.28f, 1.35f, .18f), trunk);
        // Two perpendicular polygon cards keep the silhouette dimensional while visibly planar.
        Disc("Crown front", tree, new Vector3(0, 1.62f, -.05f), .95f, dark, 9, Quaternion.Euler(90, 0, 0));
        Disc("Crown cross", tree, new Vector3(0, 1.62f, 0), .82f, light, 9, Quaternion.Euler(90, 90, 0));
        Disc("Highlight card", tree, new Vector3(-.28f, 1.9f, -.11f), .38f, light, 8, Quaternion.Euler(90, 0, 0));
    }

    static void PixelTree(Transform parent, Vector3 pos, int spriteIndex, float scale, Material trunk, Material dark, Material light)
    {
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Vegetation/Trees 3.png");
        Sprite selected = null;
        string wanted = "Tree_" + spriteIndex.ToString("00");
        foreach (var asset in sprites)
            if (asset is Sprite sprite && (sprite.name == wanted || sprite.name == "Trees 3_" + spriteIndex)) { selected = sprite; wanted = sprite.name; break; }

        // Keeping the procedural fallback makes the scene generator robust if the source atlas is moved.
        if (selected == null)
        {
            Tree(parent, pos, trunk, dark, light, scale);
            return;
        }

        var go = new GameObject("Pixel tree card " + wanted);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos + Vector3.up * .02f;
        go.transform.localScale = Vector3.one * scale;
        go.AddComponent<PixelBillboard>().followVertical = false;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = selected;
        renderer.sharedMaterial = PixelCardMaterial();
        renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
        renderer.receiveShadows = true;
    }

    static void CreatePixelTileGround(Transform parent, Vector3 center, int columns, int rows, float cellSize)
    {
        const string atlasPath = "Assets/Art/tilesets/Set 1.1.png";
        var allAssets = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
        var variants = new System.Collections.Generic.List<Sprite>();
        // These are the first light-ground tiles in the user's sliced atlas. Small deterministic
        // variations stop the surface looking mechanically repeated without changing its palette.
        for (int index = 0; index < 4; index++)
        {
            string wanted = "Set 1.1_" + index;
            foreach (var asset in allAssets)
                if (asset is Sprite sprite && sprite.name == wanted) { variants.Add(sprite); break; }
        }

        if (variants.Count == 0)
        {
            Box("Green top sheet fallback", parent, center, new Vector3(columns * cellSize, .06f, rows * cellSize), Mat("Grass", new Color(.38f, .62f, .43f)));
            return;
        }

        var group = NewGroup("Pixel tile ground", parent);
        group.localPosition = center;
        var material = PixelCardMaterial();
        float spriteWorldSize = variants[0].rect.width / variants[0].pixelsPerUnit;
        float scale = cellSize / spriteWorldSize;
        for (int z = 0; z < rows; z++)
        for (int x = 0; x < columns; x++)
        {
            var tile = new GameObject("Ground tile " + x + "," + z);
            tile.transform.SetParent(group, false);
            tile.transform.localPosition = new Vector3((x - (columns - 1) * .5f) * cellSize, 0, (z - (rows - 1) * .5f) * cellSize);
            tile.transform.localRotation = Quaternion.Euler(90, 0, 0);
            tile.transform.localScale = Vector3.one * scale;
            var renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = variants[(x * 17 + z * 31) % variants.Count];
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = true;
        }
    }

    static Material PixelCardMaterial()
    {
        const string path = MaterialFolder + "/PixelCard.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("PaperDiorama/Pixel Card URP");
        if (material == null)
        {
            material = new Material(shader) { name = "Pixel Card" };
            AssetDatabase.CreateAsset(material, path);
        }
        else if (shader != null) material.shader = shader;
        material.SetFloat("_Cutoff", .35f);
        material.SetFloat("_Grayscale", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void PixelDecoration(Transform parent, string atlasPath, string spriteName, string name,
        Vector3 position, float scale, bool upright, Quaternion? fixedRotation = null)
    {
        Sprite selected = null;
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(atlasPath))
            if (asset is Sprite sprite && sprite.name == spriteName) { selected = sprite; break; }
        if (selected == null) return;

        var go = new GameObject(name + " " + spriteName);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = position;
        go.transform.localScale = Vector3.one * scale;
        if (upright)
        {
            go.transform.localRotation = fixedRotation ?? Quaternion.identity;
            if (!fixedRotation.HasValue) go.AddComponent<PixelBillboard>().followVertical = false;
        }
        else go.transform.localRotation = Quaternion.Euler(90, 0, 0);

        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = selected;
        renderer.sharedMaterial = PixelCardMaterial();
        renderer.shadowCastingMode = upright ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off;
        renderer.receiveShadows = true;
    }

    static void Bush(Transform parent, Vector3 pos, Material mat, float scale)
    {
        var bush = NewGroup("Bush card", parent);
        bush.localPosition = pos;
        Disc("Polygon foliage", bush, new Vector3(0, scale, 0), scale, mat, 7, Quaternion.Euler(90, 0, 0));
    }

    static GameObject Disc(string name, Transform parent, Vector3 pos, float radius, Material mat, int sides, Quaternion rotation)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rotation;
        var mesh = new Mesh { name = name + " Mesh" };
        var vertices = new Vector3[sides + 1];
        var triangles = new int[sides * 3];
        vertices[0] = Vector3.zero;
        for (int i = 0; i < sides; i++)
        {
            float a = i * Mathf.PI * 2f / sides;
            vertices[i + 1] = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
            triangles[i * 3] = 0; triangles[i * 3 + 1] = i + 1; triangles[i * 3 + 2] = (i + 1) % sides + 1;
        }
        mesh.vertices = vertices; mesh.triangles = triangles; mesh.RecalculateNormals(); mesh.RecalculateBounds();
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>(); renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = ShadowCastingMode.TwoSided; renderer.receiveShadows = true;
        return go;
    }

    static GameObject Box(string name, Transform parent, Vector3 pos, Vector3 size, Material mat, Quaternion? rotation = null)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localRotation = rotation ?? Quaternion.identity;
        go.transform.localScale = size;
        var renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = ShadowCastingMode.On; renderer.receiveShadows = true;
        return go;
    }

    static Transform NewGroup(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static Material Mat(string name, Color color)
    {
        string path = MaterialFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("PaperDiorama/Monochrome Toon URP");
        if (material == null)
        {
            material = new Material(shader != null ? shader : Shader.Find("Standard")) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        else if (shader != null)
        {
            material.shader = shader;
        }
        material.color = color;
        if (material.HasProperty("_ShadowTone")) material.SetFloat("_ShadowTone", .42f);
        if (material.HasProperty("_Threshold")) material.SetFloat("_Threshold", .54f);
        if (material.HasProperty("_OutlineWidth")) material.SetFloat("_OutlineWidth", .018f);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void EnsureFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Art")) AssetDatabase.CreateFolder("Assets", "Art");
        if (!AssetDatabase.IsValidFolder(MaterialFolder)) AssetDatabase.CreateFolder("Assets/Art", "PaperDiorama");
    }

    static EditorBuildSettingsScene[] AppendScene(EditorBuildSettingsScene[] scenes, string path)
    {
        foreach (var s in scenes) if (s.path == path) return scenes;
        var result = new EditorBuildSettingsScene[scenes.Length + 1];
        scenes.CopyTo(result, 0);
        result[result.Length - 1] = new EditorBuildSettingsScene(path, true);
        return result;
    }
}
