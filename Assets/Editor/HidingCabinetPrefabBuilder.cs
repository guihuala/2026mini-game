#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class HidingCabinetPrefabBuilder
{
    private const string PrefabPath = "Assets/Prefab/HidingCabinet.prefab";

    [MenuItem("Tools/Gameplay/Rebuild Hiding Cabinet Prefab")]
    public static void Build()
    {
        GameObject root = new GameObject("Hiding Cabinet");

        SpriteRenderer body = root.AddComponent<SpriteRenderer>();
        body.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        body.color = new Color(0.24f, 0.12f, 0.055f, 1f);
        body.sortingOrder = 2;
        root.transform.localScale = new Vector3(1.4f, 2.1f, 1f);

        BoxCollider2D interaction = root.AddComponent<BoxCollider2D>();
        interaction.isTrigger = true;
        interaction.size = new Vector2(1.45f, 1.2f);
        interaction.offset = new Vector2(0f, -0.1f);

        root.AddComponent<HidingCabinet>();

        GameObject hidingPoint = new GameObject("Hiding Point");
        hidingPoint.transform.SetParent(root.transform, false);
        hidingPoint.transform.localPosition = Vector3.zero;

        GameObject leftDoor = CreateDoor("Left Door", root.transform, -0.27f);
        GameObject rightDoor = CreateDoor("Right Door", root.transform, 0.27f);
        leftDoor.transform.localScale = new Vector3(0.48f, 0.9f, 1f);
        rightDoor.transform.localScale = new Vector3(0.48f, 0.9f, 1f);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created hiding cabinet prefab at " + PrefabPath);
    }

    private static GameObject CreateDoor(string objectName, Transform parent, float x)
    {
        GameObject door = new GameObject(objectName);
        door.transform.SetParent(parent, false);
        door.transform.localPosition = new Vector3(x, 0f, -0.01f);
        SpriteRenderer renderer = door.AddComponent<SpriteRenderer>();
        renderer.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        renderer.color = new Color(0.38f, 0.2f, 0.08f, 1f);
        renderer.sortingOrder = 3;
        return door;
    }

    public static void BuildFromCommandLine()
    {
        Build();
    }
}
#endif
