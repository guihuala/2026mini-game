using UnityEngine;

public sealed class MinigameStationInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DirectionSequenceMinigame minigame;
    [SerializeField] private string minigameId = "greybox.direction_sequence";
    [SerializeField] private string completionFlag = "greybox.minigame_complete";
    [SerializeField] private string prompt = "E  开始方向校准";

    public bool CanInteract(PlayerInteractor interactor) => minigame != null && !minigame.IsRunning
        && DialogueRuntimeState.HasFlag(DemoQuestChain.GateOpenedFlag)
        && DialogueRuntimeState.GetNumber(QuestObjectiveExamples.FlowerCountId) >= 3f;

    public string GetInteractionPrompt(PlayerInteractor interactor)
    {
        return DialogueRuntimeState.HasFlag(completionFlag) ? "E  重玩方向校准" : prompt;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!CanInteract(interactor)) return;
        minigame.StartGame(new MinigameContext
        {
            minigameId = minigameId,
            difficulty = 1,
            seed = 0
        }, OnCompleted);
    }

    private void OnCompleted(MinigameResult result)
    {
        if (result == null || !result.succeeded) return;
        DialogueRuntimeState.SetFlag(completionFlag, true);
        DemoQuestChain.CompleteCalibration();
        DialogueRuntimeState.SetNumber("minigame.direction_sequence.score", result.score);
        ExplorationSession session = FindObjectOfType<ExplorationSession>();
        if (session != null) session.SaveNow(null);
    }
}
