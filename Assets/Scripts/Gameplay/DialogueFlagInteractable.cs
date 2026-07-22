using System.Collections.Generic;
using UnityEngine;

public sealed class DialogueFlagInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "E  与守门人交谈";
    [SerializeField] private string completionFlag = "greybox.gate_open";
    [SerializeField] private string speakerName = "守门人";

    public bool CanInteract(PlayerInteractor interactor) => DialogueManager.Instance != null;

    public string GetInteractionPrompt(PlayerInteractor interactor)
    {
        DemoQuestChain.SyncLegacyState();
        if (DemoQuestChain.State == DemoQuestChain.ReportBack) return "E  向守门人复命";
        if (DemoQuestChain.State == DemoQuestChain.CollectFlowers) return "E  询问纸花位置";
        if (DemoQuestChain.State == DemoQuestChain.Calibrate) return "E  询问委托目标";
        return DemoQuestChain.State == DemoQuestChain.Completed ? "E  再次交谈" : prompt;
    }

    public void Interact(PlayerInteractor interactor)
    {
        DemoQuestChain.SyncLegacyState();
        string state = DemoQuestChain.State;
        var lines = new List<DialogueLine>
        {
            new DialogueLine { speakerName = speakerName, text = GetDialogueText(state), style = DialogueBoxStyle.Normal }
        };

        System.Action finished = null;
        finished = () =>
        {
            DialogueManager.Instance.DialogueFinished -= finished;
            if (state == DemoQuestChain.NotStarted) DemoQuestChain.Accept();
            else if (state == DemoQuestChain.ReportBack) DemoQuestChain.Complete();
            else if (!DialogueRuntimeState.HasFlag(completionFlag)) DialogueRuntimeState.SetFlag(completionFlag, true);

            ExplorationSession session = FindObjectOfType<ExplorationSession>();
            if (session != null) session.SaveNow(null);
        };
        DialogueManager.Instance.DialogueFinished += finished;
        DialogueManager.Instance.Play(lines);
    }

    private static string GetDialogueText(string state)
    {
        switch (state)
        {
            case DemoQuestChain.CollectFlowers: return "先在附近收集三朵纸花。它们可以为校准台补充能量。";
            case DemoQuestChain.Calibrate: return "校准台就在前面。按提示完成五次方向输入，再回来找我。";
            case DemoQuestChain.ReportBack: return "校准恢复了，做得很好。出口现在已经为你开放。";
            case DemoQuestChain.Completed: return "委托已经完成。沿着前面的路前往出口吧。";
            default: return "前路的方向校准台失灵了。先收集三朵纸花补充能量，再完成校准并回来向我复命。";
        }
    }
}
