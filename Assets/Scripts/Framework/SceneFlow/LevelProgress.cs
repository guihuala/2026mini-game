using System;
using System.Collections.Generic;
using UnityEngine;

public static class LevelProgress
{
    private const string CatalogPath = "Data/LevelCatalog";
    private static LevelCatalog _catalog;

    public static LevelCatalog Catalog => _catalog != null
        ? _catalog
        : (_catalog = Resources.Load<LevelCatalog>(CatalogPath));

    public static bool IsUnlocked(string levelId)
    {
        LevelCatalog catalog = Catalog;
        int index = catalog != null ? catalog.IndexOf(levelId) : -1;
        if (index < 0) return false;
        if (index == 0) return true;

        SaveData data = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : null;
        return data != null && data.unlockedLevels != null && data.unlockedLevels.Contains(levelId);
    }

    public static bool SelectLevel(string levelId)
    {
        LevelCatalog catalog = Catalog;
        int index = catalog != null ? catalog.IndexOf(levelId) : -1;
        if (index < 0 || !IsUnlocked(levelId)) return false;

        SaveManager saveManager = SaveManager.Instance;
        if (saveManager != null)
        {
            EnsureInitialized(saveManager.CurrentData);
            saveManager.CurrentData.currentLevel = index;
            saveManager.CurrentData.currentLevelId = levelId;
            saveManager.SaveGame();
        }

        return true;
    }

    public static void CompleteCurrentLevel()
    {
        SaveManager saveManager = SaveManager.Instance;
        LevelCatalog catalog = Catalog;
        if (saveManager == null || catalog == null) return;

        SaveData data = saveManager.CurrentData;
        EnsureInitialized(data);
        int currentIndex = ResolveCurrentLevelIndex(data);
        LevelDefinition next = catalog.Get(currentIndex + 1);

        if (next != null && !data.unlockedLevels.Contains(next.id))
            data.unlockedLevels.Add(next.id);

        data.highestCompletedLevel = Mathf.Max(data.highestCompletedLevel, currentIndex);
        saveManager.SaveGame();
    }

    public static LevelDefinition GetCurrentLevel()
    {
        LevelCatalog catalog = Catalog;
        if (catalog == null) return null;

        SaveData data = SaveManager.Instance != null ? SaveManager.Instance.CurrentData : null;
        if (data == null) return catalog.Get(0);

        LevelDefinition byId = catalog.Get(data.currentLevelId);
        return byId ?? catalog.Get(data.currentLevel) ?? catalog.Get(0);
    }

    public static void EnsureInitialized(SaveData data)
    {
        LevelCatalog catalog = Catalog;
        if (data == null || catalog == null || catalog.Levels.Count == 0) return;

        if (data.unlockedLevels == null) data.unlockedLevels = new List<string>();
        LevelDefinition first = catalog.Get(0);
        if (first != null && !data.unlockedLevels.Contains(first.id))
            data.unlockedLevels.Add(first.id);

        if (string.IsNullOrEmpty(data.currentLevelId))
        {
            int safeIndex = Mathf.Clamp(data.currentLevel, 0, catalog.Levels.Count - 1);
            LevelDefinition current = catalog.Get(safeIndex);
            if (current != null) data.currentLevelId = current.id;
        }
    }

    private static int ResolveCurrentLevelIndex(SaveData data)
    {
        int byId = Catalog.IndexOf(data.currentLevelId);
        return byId >= 0 ? byId : Mathf.Clamp(data.currentLevel, 0, Catalog.Levels.Count - 1);
    }
}
