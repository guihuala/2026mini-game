using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ConfigurePixelCards
{
    const string TreesPath = "Assets/Art/Vegetation/Trees 3.png";

    [MenuItem("Tools/2.5D/Slice Pixel Assets and Rebuild Scene")]
    public static void ConfigureAndRebuild()
    {
        SliceGrid(TreesPath, 32, 40, "Tree");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        CreatePaperDioramaScene.CreateScene();
        Debug.Log("Pixel cards configured and PaperDiorama rebuilt.");
    }

    static void SliceGrid(string path, int width, int height, string prefix)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.spritePixelsPerUnit = 32f;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        var sprites = new List<SpriteMetaData>();
        int index = 0;
        // Name in visual reading order, while texture rects use bottom-left coordinates.
        for (int y = texture.height - height; y >= 0; y -= height)
        for (int x = 0; x < texture.width; x += width)
        {
            sprites.Add(new SpriteMetaData
            {
                name = prefix + "_" + index.ToString("00"),
                rect = new Rect(x, y, width, height),
                alignment = (int)SpriteAlignment.BottomCenter,
                pivot = new Vector2(.5f, 0f)
            });
            index++;
        }
#pragma warning disable 618
        importer.spritesheet = sprites.ToArray();
#pragma warning restore 618
        importer.SaveAndReimport();
    }
}
