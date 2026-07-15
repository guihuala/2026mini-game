using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : SingletonPersistent<SaveManager>
{
    [Header("Save File")]
    [SerializeField] private string saveDirectoryName = "Saves";
    [SerializeField] private string saveFileName = "save.json";
    [SerializeField] private bool loadOnAwake;

    public SaveData CurrentData { get; private set; }
    public string SaveDirectory => Path.Combine(Application.persistentDataPath, saveDirectoryName);
    public string SavePath => Path.Combine(SaveDirectory, saveFileName);

    public event Action<SaveData> SaveLoaded;
    public event Action<SaveData> SaveCompleted;
    public event Action<Exception> SaveFailed;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        if (loadOnAwake) LoadGame();
        else CurrentData = CreateNewSaveData();
    }

    public SaveData NewGame()
    {
        DialogueRuntimeState.Reset();
        CurrentData = CreateNewSaveData();
        ApplyMetadata(CurrentData);
        return CurrentData;
    }

    public bool LoadGame()
    {
        if (!HasSave())
        {
            ResetToNewData();
            return false;
        }

        try
        {
            SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            List<SaveValidationIssue> issues = SaveValidation.Validate(data);
            if (data == null || SaveValidation.HasErrors(issues))
            {
                Debug.LogWarning($"Save validation failed:\n{SaveValidation.FormatIssues(issues)}");
                ResetToNewData();
                return false;
            }

            LogValidationWarnings(issues);
            CurrentData = data;
            ApplyMetadata(CurrentData, false);
            RestoreRuntimeState(CurrentData);
            SaveLoaded?.Invoke(CurrentData);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Load save failed: {exception.Message}");
            ResetToNewData();
            SaveFailed?.Invoke(exception);
            return false;
        }
    }

    public bool SaveGame()
    {
        if (CurrentData == null) CurrentData = CreateNewSaveData();

        try
        {
            CaptureRuntimeState(CurrentData);
            ApplyMetadata(CurrentData);
            List<SaveValidationIssue> issues = SaveValidation.Validate(CurrentData);
            if (SaveValidation.HasErrors(issues))
            {
                Debug.LogWarning($"Save validation failed before writing:\n{SaveValidation.FormatIssues(issues)}");
                SaveFailed?.Invoke(new InvalidDataException("Save data validation failed."));
                return false;
            }

            LogValidationWarnings(issues);
            Directory.CreateDirectory(SaveDirectory);
            File.WriteAllText(SavePath, JsonUtility.ToJson(CurrentData, true));
            SaveCompleted?.Invoke(CurrentData);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Save game failed: {exception.Message}");
            SaveFailed?.Invoke(exception);
            return false;
        }
    }

    public bool DeleteSave()
    {
        try
        {
            if (File.Exists(SavePath)) File.Delete(SavePath);
            ResetToNewData();
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Delete save failed: {exception.Message}");
            SaveFailed?.Invoke(exception);
            return false;
        }
    }

    public bool HasSave() => File.Exists(SavePath);

    public void SetCurrentData(SaveData data)
    {
        CurrentData = data ?? CreateNewSaveData();
        ApplyMetadata(CurrentData, false);
        RestoreRuntimeState(CurrentData);
    }

    private SaveData CreateNewSaveData() => new SaveData();

    private void ResetToNewData()
    {
        CurrentData = CreateNewSaveData();
        ApplyMetadata(CurrentData);
        DialogueRuntimeState.Reset();
    }

    private void CaptureRuntimeState(SaveData data)
    {
        if (data != null) data.dialogue = DialogueRuntimeState.Capture();
    }

    private void RestoreRuntimeState(SaveData data)
    {
        DialogueRuntimeState.Restore(data != null ? data.dialogue : null);
    }

    private void ApplyMetadata(SaveData data, bool refreshSaveTime = true)
    {
        if (data == null) return;
        if (string.IsNullOrEmpty(data.createdTime)) data.createdTime = DateTime.Now.ToString(SaveValidation.DateTimeFormat);
        if (!refreshSaveTime) return;
        data.lastSaveTime = DateTime.Now.ToString(SaveValidation.DateTimeFormat);
        data.sceneName = SceneManager.GetActiveScene().name;
    }

    private void LogValidationWarnings(List<SaveValidationIssue> issues)
    {
        if (issues != null && issues.Count > 0)
            Debug.LogWarning($"Save validation warnings:\n{SaveValidation.FormatIssues(issues)}");
    }
}
