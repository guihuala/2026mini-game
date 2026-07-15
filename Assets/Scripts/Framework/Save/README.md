# 单存档系统

`SaveManager` 为短期项目提供单一 JSON 进度文件，不包含存档位选择或管理逻辑。

## 使用

1. 场景中放置带 `SaveManager` 的管理器预制体。
2. 如需启动时自动读取，勾选 `Load On Awake`。
3. 存档固定写入 `Application.persistentDataPath/Saves/save.json`。

```csharp
SaveManager.Instance.NewGame();
SaveManager.Instance.SaveGame();
SaveManager.Instance.LoadGame();
SaveManager.Instance.DeleteSave();
```

当前数据通过 `SaveManager.Instance.CurrentData` 访问。保存时会捕获 `DialogueRuntimeState`，读取时会恢复；新游戏、删除存档和读取失败会清空旧的对话状态。

设置、音量、分辨率和输入绑定等用户偏好继续使用 `PlayerPrefs`，不进入游戏进度存档。
