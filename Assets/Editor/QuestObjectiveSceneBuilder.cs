using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class QuestObjectiveSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/PaperDiorama.unity";
    private const string RootName = "QUEST OBJECTIVE EXAMPLES";
    private const string MaterialFolder = "Assets/Art/PaperDiorama/Greybox";

    [MenuItem("Tools/Template/Quest/Build Static Objective Examples")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject existing = GameObject.Find(RootName);
        if (existing != null) Object.DestroyImmediate(existing);

        GameObject parent = GameObject.Find("GREYBOX - EXPLORATION LOOP");
        GameObject root = new GameObject(RootName);
        if (parent != null) root.transform.SetParent(parent.transform, false);

        Material itemMaterial = GetOrCreateMaterial("QuestExampleItem", new Color(1f, 0.72f, 0.18f));
        Material flowerMaterial = GetOrCreateMaterial("QuestExampleFlower", new Color(0.95f, 0.42f, 0.68f));
        Material flagMaterial = GetOrCreateMaterial("QuestExampleFlag", new Color(0.25f, 0.75f, 1f));

        Create(root.transform, "案例 1 - 拾取遗失铃铛", PrimitiveType.Sphere,
            new Vector3(-4.5f, 0.3f, -0.5f), new Vector3(0.55f, 0.35f, 0.55f), itemMaterial,
            "E  拾取遗失铃铛", QuestObjectiveType.CollectItem, QuestObjectiveExamples.BellItemId, 1f, true);

        for (int i = 0; i < 3; i++)
        {
            Create(root.transform, $"案例 2 - 收集纸花 {i + 1}", PrimitiveType.Capsule,
                new Vector3(-3.15f + i * 1.35f, 0.3f, -0.5f), new Vector3(0.35f, 0.35f, 0.35f),
                flowerMaterial, "E  收集纸花", QuestObjectiveType.Number,
                QuestObjectiveExamples.FlowerCountId, 1f, true, "flag:" + DemoQuestChain.GateOpenedFlag);
        }

        Create(root.transform, "案例 3 - 点亮信标", PrimitiveType.Cylinder,
            new Vector3(0.9f, 0.3f, -0.5f), new Vector3(0.55f, 0.35f, 0.55f), flagMaterial,
            "E  点亮信标", QuestObjectiveType.Flag, QuestObjectiveExamples.BeaconFlagId, 1f, false);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = root;
        Debug.Log("Built 5 static quest objective examples in PaperDiorama.");
    }

    private static void Create(Transform parent, string objectName, PrimitiveType primitive,
        Vector3 localPosition, Vector3 localScale, Material material, string prompt,
        QuestObjectiveType type, string targetId, float amount, bool consume)
    {
        Create(parent, objectName, primitive, localPosition, localScale, material, prompt,
            type, targetId, amount, consume, null);
    }

    private static void Create(Transform parent, string objectName, PrimitiveType primitive,
        Vector3 localPosition, Vector3 localScale, Material material, string prompt,
        QuestObjectiveType type, string targetId, float amount, bool consume, string requiredCondition)
    {
        GameObject example = GameObject.CreatePrimitive(primitive);
        example.name = objectName;
        example.transform.SetParent(parent, false);
        example.transform.localPosition = localPosition;
        example.transform.localScale = localScale;
        Renderer renderer = example.GetComponent<Renderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        QuestObjectiveInteractable interactable = example.AddComponent<QuestObjectiveInteractable>();
        interactable.Configure(prompt, type, targetId, amount, consume, null, requiredCondition);
        EditorUtility.SetDirty(example);
        EditorUtility.SetDirty(interactable);
    }

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        string path = $"{MaterialFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            material = new Material(shader) { name = materialName };
            AssetDatabase.CreateAsset(material, path);
        }
        material.color = color;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }
}
