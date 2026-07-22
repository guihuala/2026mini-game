using UnityEngine;
using UnityEngine.UI;

public sealed class GreyboxDebugPanel : BasePanel
{
    [SerializeField] private Text stateText;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button openGateButton;
    [SerializeField] private Button completeMinigameButton;
    [SerializeField] private Button startSpawnButton;
    [SerializeField] private Button returnSpawnButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    private PlayerMotor player;

    protected override void Awake()
    {
        base.Awake();
        resetButton.onClick.AddListener(ResetFlags);
        openGateButton.onClick.AddListener(DemoQuestChain.Accept);
        completeMinigameButton.onClick.AddListener(DemoQuestChain.CompleteCalibration);
        startSpawnButton.onClick.AddListener(() => WarpTo("Start"));
        returnSpawnButton.onClick.AddListener(() => WarpTo("Return"));
        saveButton.onClick.AddListener(Save);
        loadButton.onClick.AddListener(Load);
    }

    public void Configure(PlayerMotor target) => player = target;

    private void Update()
    {
        if (stateText == null) return;
        stateText.text =
            $"intro_seen: {DialogueRuntimeState.HasFlag("greybox.intro_seen")}\n" +
            $"gate_open: {DialogueRuntimeState.HasFlag("greybox.gate_open")}\n" +
            $"minigame_complete: {DialogueRuntimeState.HasFlag("greybox.minigame_complete")}\n" +
            $"ending_unlocked: {DialogueRuntimeState.HasFlag("greybox.ending_unlocked")}\n" +
            $"quest: {DialogueRuntimeState.GetQuestState(DemoQuestChain.QuestId)}\n" +
            $"example_bell: {DialogueRuntimeState.HasItem(QuestObjectiveExamples.BellItemId)}\n" +
            $"example_flowers: {DialogueRuntimeState.GetNumber(QuestObjectiveExamples.FlowerCountId):0}/3\n" +
            $"example_beacon: {DialogueRuntimeState.HasFlag(QuestObjectiveExamples.BeaconFlagId)}\n" +
            $"demo_complete: {DialogueRuntimeState.HasFlag("greybox.demo_complete")}";
    }

    private void ResetFlags()
    {
        DialogueRuntimeState.SetFlag("greybox.intro_seen", false);
        DialogueRuntimeState.SetFlag("greybox.gate_open", false);
        DialogueRuntimeState.SetFlag("greybox.minigame_complete", false);
        DialogueRuntimeState.SetFlag("greybox.ending_unlocked", false);
        DialogueRuntimeState.SetFlag("greybox.demo_complete", false);
        DialogueRuntimeState.SetQuestState(DemoQuestChain.QuestId, string.Empty);
        QuestObjectiveExamples.ResetState();
        ExplorationControlLock.Reset();
    }

    private void Save()
    {
        ExplorationSession session = FindObjectOfType<ExplorationSession>();
        if (session != null) session.SaveNow(null);
    }

    private void Load()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.LoadGame();
        WarpFromSave();
    }

    private void WarpTo(string id)
    {
        if (player == null) player = FindObjectOfType<PlayerMotor>();
        foreach (ExplorationSpawnPoint spawn in FindObjectsOfType<ExplorationSpawnPoint>())
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
