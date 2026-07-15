using UnityEngine;

public sealed class GreyboxDebugPanel : MonoBehaviour
{
    [SerializeField] private PlayerMotor player;
    private bool visible;
    private Rect windowRect = new Rect(16, 105, 340, 310);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) visible = !visible;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(18, 88, 240, 24), "F1：剧情调试工具");
        if (visible) windowRect = GUI.Window(GetInstanceID(), windowRect, DrawWindow, "Greybox Narrative Debug");
    }

    private void DrawWindow(int id)
    {
        GUILayout.Label("intro_seen: " + DialogueRuntimeState.HasFlag("greybox.intro_seen"));
        GUILayout.Label("gate_open: " + DialogueRuntimeState.HasFlag("greybox.gate_open"));
        GUILayout.Label("minigame_complete: " + DialogueRuntimeState.HasFlag("greybox.minigame_complete"));
        GUILayout.Label("demo_complete: " + DialogueRuntimeState.HasFlag("greybox.demo_complete"));
        if (GUILayout.Button("重置剧情 Flags"))
        {
            DialogueRuntimeState.SetFlag("greybox.intro_seen", false);
            DialogueRuntimeState.SetFlag("greybox.gate_open", false);
            DialogueRuntimeState.SetFlag("greybox.minigame_complete", false);
            DialogueRuntimeState.SetFlag("greybox.demo_complete", false);
            ExplorationControlLock.Reset();
        }
        if (GUILayout.Button("直接开启道路")) DialogueRuntimeState.SetFlag("greybox.gate_open", true);
        if (GUILayout.Button("直接完成小游戏")) DialogueRuntimeState.SetFlag("greybox.minigame_complete", true);
        if (GUILayout.Button("传送到 Start")) WarpTo("Start");
        if (GUILayout.Button("传送到 Return")) WarpTo("Return");
        if (GUILayout.Button("保存当前状态") && FindObjectOfType<ExplorationSession>() is ExplorationSession session) session.SaveNow(null);
        if (GUILayout.Button("读取当前槽位") && SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
            WarpFromSave();
        }
        GUI.DragWindow(new Rect(0, 0, 10000, 24));
    }

    private void WarpTo(string id)
    {
        if (player == null) player = FindObjectOfType<PlayerMotor>();
        foreach (var spawn in FindObjectsOfType<ExplorationSpawnPoint>())
            if (spawn.Id == id) { player.Warp(spawn.transform.position, Vector3.forward); break; }
    }

    private void WarpFromSave()
    {
        if (player == null || SaveManager.Instance == null || SaveManager.Instance.CurrentData == null) return;
        ExplorationSaveData data = SaveManager.Instance.CurrentData.exploration;
        if (data == null) return;
        if (!string.IsNullOrEmpty(data.spawnPointId)) WarpTo(data.spawnPointId);
        else if (data.hasPosition) player.Warp(new Vector3(data.positionX, data.positionY, data.positionZ), new Vector3(data.facingX, 0, data.facingZ));
    }
}
