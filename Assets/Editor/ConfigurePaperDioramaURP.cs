using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ConfigurePaperDioramaURP
{
    const string Folder = "Assets/Setting/URP";
    const string PipelinePath = Folder + "/PaperDioramaURP.asset";
    const string RendererPath = Folder + "/PaperDioramaRenderer.asset";

    [MenuItem("Tools/2.5D/Configure URP and Rebuild Scene")]
    public static void Configure()
    {
        EnsureFolder("Assets/Setting", "URP");

        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, RendererPath);
        }

        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.name = "Paper Diorama URP";
            AssetDatabase.CreateAsset(pipeline, PipelinePath);
        }

        pipeline.shadowDistance = 35f;
        pipeline.msaaSampleCount = 4;
        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
        EditorUtility.SetDirty(pipeline);
        AssetDatabase.SaveAssets();

        CreatePaperDioramaScene.CreateScene();
        Debug.Log("URP configured and PaperDiorama rebuilt successfully.");
    }

    static void EnsureFolder(string parent, string child)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + child))
            AssetDatabase.CreateFolder(parent, child);
    }
}
