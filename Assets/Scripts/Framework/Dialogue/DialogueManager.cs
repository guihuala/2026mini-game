using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : SingletonPersistent<DialogueManager>
{
    [SerializeField] private string dialoguePanelName = "DialoguePanel";
    [SerializeField] private UIPanelLayer panelLayer = UIPanelLayer.Top;

    public bool IsPlaying { get; private set; }
    public event Action DialogueStarted;
    public event Action DialogueFinished;
    public DialoguePanel CurrentPanel { get; private set; }

    public void Play(DialogueSequence sequence)
    {
        if (sequence != null) Play(sequence.lines);
    }

    public void Play(DialogueSequenceAsset sequenceAsset)
    {
        if (sequenceAsset != null) Play(sequenceAsset.Lines);
    }

    public void Play(IList<DialogueLine> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("Dialogue lines are empty.");
            return;
        }

        CurrentPanel = UIManager.Instance != null
            ? UIManager.Instance.OpenPanel(dialoguePanelName, null, panelLayer) as DialoguePanel
            : null;
        if (CurrentPanel == null)
        {
            Debug.LogWarning($"Dialogue panel {dialoguePanelName} is missing.");
            return;
        }

        IsPlaying = true;
        DialogueStarted?.Invoke();
        CurrentPanel.Play(lines, FinishDialogue);
    }

    public void Stop() => FinishDialogue();

    private void FinishDialogue()
    {
        if (!IsPlaying) return;
        IsPlaying = false;
        CurrentPanel = null;
        if (UIManager.Instance != null && UIManager.Instance.IsPanelOpen(dialoguePanelName))
            UIManager.Instance.ClosePanel(dialoguePanelName);
        DialogueFinished?.Invoke();
    }
}
