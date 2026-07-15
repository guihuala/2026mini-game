using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class NarrativeSequenceController : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private string completionFlag = "greybox.intro_seen";
    [SerializeField] private bool playOnlyOnce = true;
    [Header("Actors")]
    [SerializeField] private PlayerMotor player;
    [SerializeField] private NpcWaypointMover npc;
    [SerializeField] private Transform npcDestination;
    [SerializeField] private FollowCamera followCamera;
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "守门人";
    [TextArea] [SerializeField] private string firstLine = "等一下，旅行者。前面的道路暂时封闭。";
    [TextArea] [SerializeField] private string secondLine = "先来和我谈谈，我会告诉你如何通过。";
    private bool running;

    public bool CanPlay => !running && (!playOnlyOnce || !DialogueRuntimeState.HasFlag(completionFlag));

    private void Start()
    {
        if (playOnlyOnce && DialogueRuntimeState.HasFlag(completionFlag) && npc != null && npcDestination != null)
        {
            Vector3 restoredPosition = npcDestination.position;
            restoredPosition.y = npc.transform.position.y;
            npc.transform.position = restoredPosition;
        }
    }

    public void Play()
    {
        if (CanPlay) StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        running = true;
        ExplorationControlLock.Acquire(this);
        if (followCamera != null && npc != null) followCamera.Focus(npc.transform);
        yield return new WaitForSeconds(.35f);
        if (npc != null && npcDestination != null) yield return npc.MoveTo(npcDestination);
        yield return new WaitForSeconds(.2f);

        if (DialogueManager.Instance != null)
        {
            bool finished = false;
            System.Action onFinished = () => finished = true;
            DialogueManager.Instance.DialogueFinished += onFinished;
            DialogueManager.Instance.Play(new List<DialogueLine>
            {
                new DialogueLine { speakerName = speakerName, text = firstLine },
                new DialogueLine { speakerName = speakerName, text = secondLine }
            });
            while (!finished) yield return null;
            DialogueManager.Instance.DialogueFinished -= onFinished;
        }

        DialogueRuntimeState.SetFlag(completionFlag, true);
        if (followCamera != null) followCamera.RestoreExplorationTarget();
        yield return new WaitForSeconds(.25f);
        ExplorationControlLock.Release(this);
        running = false;
    }

    private void OnDisable()
    {
        ExplorationControlLock.Release(this);
    }
}
