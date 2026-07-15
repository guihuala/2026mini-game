using System;
using UnityEngine;

public sealed class DirectionSequenceMinigame : MonoBehaviour, IMinigame
{
    [SerializeField] private int requiredInputs = 5;
    [SerializeField] private float timeLimit = 10f;
    private static readonly KeyCode[] Keys = { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    private static readonly string[] Labels = { "↑  W", "←  A", "↓  S", "→  D" };
    private Action<MinigameResult> completion;
    private MinigameContext context;
    private System.Random random;
    private int currentKey;
    private int progress;
    private float startTime;
    private float remaining;
    private string feedback;
    private DirectionMinigamePanel panel;

    public bool IsRunning { get; private set; }

    public void StartGame(MinigameContext newContext, Action<MinigameResult> onCompleted)
    {
        if (IsRunning) return;
        context = newContext ?? new MinigameContext { minigameId = "direction_sequence" };
        completion = onCompleted;
        random = new System.Random(context.seed == 0 ? Environment.TickCount : context.seed);
        progress = 0;
        remaining = timeLimit;
        startTime = Time.unscaledTime;
        feedback = string.Empty;
        IsRunning = true;
        ExplorationControlLock.Acquire(this);
        panel = UIManager.Instance != null
            ? UIManager.Instance.OpenPanel("DirectionMinigamePanel", null, UIPanelLayer.Top) as DirectionMinigamePanel
            : null;
        PickNext();
        RefreshPanel();
    }

    private void Update()
    {
        if (!IsRunning) return;
        remaining -= Time.unscaledDeltaTime;
        RefreshPanel();
        if (remaining <= 0f) { Finish(false, false, "timeout"); return; }
        if (Input.GetKeyDown(KeyCode.Backspace)) { CancelGame(); return; }

        for (int i = 0; i < Keys.Length; i++)
        {
            if (!Input.GetKeyDown(Keys[i])) continue;
            if (i == currentKey)
            {
                progress++;
                feedback = "正确";
                if (progress >= requiredInputs) Finish(true, false, "completed");
                else PickNext();
            }
            else
            {
                remaining = Mathf.Max(0f, remaining - 1f);
                feedback = "按错了，时间 -1 秒";
            }
            break;
        }
    }

    public void CancelGame()
    {
        if (IsRunning) Finish(false, true, "cancelled");
    }

    private void PickNext() => currentKey = random.Next(0, Keys.Length);

    private void RefreshPanel()
    {
        if (panel != null) panel.SetView(Labels[currentKey], progress, requiredInputs, remaining, feedback);
    }

    private void Finish(bool succeeded, bool cancelled, string tag)
    {
        if (!IsRunning) return;
        IsRunning = false;
        ExplorationControlLock.Release(this);
        if (UIManager.Instance != null && UIManager.Instance.IsPanelOpen("DirectionMinigamePanel"))
            UIManager.Instance.ClosePanel("DirectionMinigamePanel");
        panel = null;
        Action<MinigameResult> callback = completion;
        completion = null;
        callback?.Invoke(new MinigameResult
        {
            minigameId = context.minigameId,
            succeeded = succeeded,
            cancelled = cancelled,
            score = progress * 100 + Mathf.CeilToInt(remaining * 10f),
            durationSeconds = Time.unscaledTime - startTime,
            resultTag = tag
        });
    }

    private void OnDisable()
    {
        if (IsRunning) CancelGame();
    }

}
