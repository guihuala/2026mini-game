using System.Collections.Generic;
using UnityEngine;

public sealed class DialogueFlagInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string prompt = "E  与守门人交谈";
    [SerializeField] private string completionFlag = "greybox.gate_open";
    [SerializeField] private string speakerName = "守门人";

    public bool CanInteract(PlayerInteractor interactor) => DialogueManager.Instance != null;
    public string GetInteractionPrompt(PlayerInteractor interactor) => DialogueRuntimeState.HasFlag(completionFlag) ? "E  再次交谈" : prompt;

    public void Interact(PlayerInteractor interactor)
    {
        bool completed = DialogueRuntimeState.HasFlag(completionFlag);
        var lines = new List<DialogueLine>
        {
            new DialogueLine
            {
                speakerName = speakerName,
                text = completed ? "前面的路已经打开了。去出口看看吧。" : "这是一段灰盒剧情。交谈结束后，前方的路会打开。",
                style = DialogueBoxStyle.Normal
            }
        };
        System.Action finished = null;
        finished = () =>
        {
            DialogueManager.Instance.DialogueFinished -= finished;
            DialogueRuntimeState.SetFlag(completionFlag, true);
        };
        DialogueManager.Instance.DialogueFinished += finished;
        DialogueManager.Instance.Play(lines);
    }
}
