using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class AttachOrbitShowcase
{
    [MenuItem("Tools/2.5D/Attach Orbit Showcase To Current Scene")]
    public static void AttachToCurrentScene()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogError("No camera tagged MainCamera was found in the current scene.");
            return;
        }

        var target = GameObject.Find("Camera Orbit Target");
        if (target == null)
        {
            target = new GameObject("Camera Orbit Target");
            target.transform.position = FindSceneCenter(camera);
        }

        var orbit = camera.GetComponent<CameraOrbitShowcase>();
        if (orbit == null) orbit = camera.gameObject.AddComponent<CameraOrbitShowcase>();
        orbit.target = target.transform;
        orbit.autoRotate = true;
        orbit.autoRotateSpeed = 8f;
        orbit.minPitch = 18f;
        orbit.maxPitch = 62f;
        orbit.minDistance = 10f;
        orbit.maxDistance = 28f;
        EditorUtility.SetDirty(camera.gameObject);
        EditorSceneManager.MarkSceneDirty(camera.gameObject.scene);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Orbit showcase attached without rebuilding the scene.");
    }

    public static void AttachToPaperDiorama()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PaperDiorama.unity", OpenSceneMode.Single);
        AttachToCurrentScene();
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Tools/2.5D/Keep Upright Sprites Vertical")]
    public static void UpgradeBillboards()
    {
        int count = 0;
        foreach (var renderer in Object.FindObjectsOfType<SpriteRenderer>())
        {
            // Horizontal tiles/decals have their local up axis nearly horizontal and must stay on the ground.
            if (Mathf.Abs(renderer.transform.up.y) < .5f) continue;
            var billboard = renderer.GetComponent<PixelBillboard>();
            if (billboard == null) billboard = renderer.gameObject.AddComponent<PixelBillboard>();
            billboard.followVertical = false;
            EditorUtility.SetDirty(renderer.gameObject);
            count++;
        }
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Set " + count + " upright sprites to Y-axis billboards.");
    }

    public static void UpgradePaperDioramaBillboards()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/PaperDiorama.unity", OpenSceneMode.Single);
        UpgradeBillboards();
    }

    static Vector3 FindSceneCenter(Camera camera)
    {
        var renderers = Object.FindObjectsOfType<Renderer>();
        bool found = false;
        var bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (var renderer in renderers)
        {
            if (!renderer.enabled || renderer.transform.IsChildOf(camera.transform)) continue;
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        var center = found ? bounds.center : Vector3.zero;
        center.y = found ? Mathf.Lerp(bounds.min.y, bounds.max.y, .32f) : 1f;
        return center;
    }
}
