using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelDefinition
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public string sceneName;
    public Sprite preview;
}

[CreateAssetMenu(fileName = "LevelCatalog", menuName = "Template/Scene Flow/Level Catalog")]
public class LevelCatalog : ScriptableObject
{
    [SerializeField] private List<LevelDefinition> levels = new List<LevelDefinition>();

    public IReadOnlyList<LevelDefinition> Levels => levels;

    public int IndexOf(string levelId)
    {
        return levels.FindIndex(level => level != null && level.id == levelId);
    }

    public LevelDefinition Get(string levelId)
    {
        int index = IndexOf(levelId);
        return index >= 0 ? levels[index] : null;
    }

    public LevelDefinition Get(int index)
    {
        return index >= 0 && index < levels.Count ? levels[index] : null;
    }
}
