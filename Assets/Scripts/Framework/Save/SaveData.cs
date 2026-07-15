using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 1;
    public string lastSaveTime;
    public string createdTime;
    public string sceneName;
    public float playTimeSeconds;

    public int coin;
    public int currentLevel;
    public List<string> unlockedLevels = new List<string>();
    public DialogueSaveData dialogue = new DialogueSaveData();
    public ExplorationSaveData exploration = new ExplorationSaveData();

    public SaveData()
    {
        createdTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        lastSaveTime = createdTime;
    }
}

[Serializable]
public class ExplorationSaveData
{
    public string spawnPointId;
    public float positionX;
    public float positionY;
    public float positionZ;
    public float facingX;
    public float facingZ = 1f;
    public bool hasPosition;
}

[Serializable]
public class DialogueSaveData
{
    public List<string> flags = new List<string>();
    public List<string> items = new List<string>();
    public List<DialogueQuestSaveData> quests = new List<DialogueQuestSaveData>();
    public List<DialogueNumberSaveData> numbers = new List<DialogueNumberSaveData>();
}

[Serializable]
public class DialogueQuestSaveData
{
    public string questId;
    public string state;
}

[Serializable]
public class DialogueNumberSaveData
{
    public string key;
    public float value;
}
