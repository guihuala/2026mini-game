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
    private GUIStyle titleStyle;
    private GUIStyle promptStyle;
    private GUIStyle centerStyle;

    public bool IsRunning { get; private set; }

    private void Awake()
    {
        titleStyle = new GUIStyle { fontSize = 25, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        promptStyle = new GUIStyle { fontSize = 48, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, normal = { textColor = new Color(.25f, 1f, .75f) } };
        centerStyle = new GUIStyle { fontSize = 18, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
    }

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
        PickNext();
    }

    private void Update()
    {
        if (!IsRunning) return;
        remaining -= Time.unscaledDeltaTime;
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

    private void Finish(bool succeeded, bool cancelled, string tag)
    {
        if (!IsRunning) return;
        IsRunning = false;
        ExplorationControlLock.Release(this);
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

    private void OnGUI()
    {
        if (!IsRunning) return;
        float width = 520f;
        float left = (Screen.width - width) * .5f;
        float top = (Screen.height - 300f) * .5f;
        GUI.Box(new Rect(left, top, width, 300f), "");
        GUI.Label(new Rect(left, top + 20, width, 40), "方向校准 · 占位小游戏", titleStyle);
        GUI.Label(new Rect(left, top + 72, width, 70), Labels[currentKey], promptStyle);
        GUI.Label(new Rect(left, top + 150, width, 30), $"进度  {progress} / {requiredInputs}", centerStyle);
        GUI.Label(new Rect(left, top + 182, width, 30), $"剩余时间  {remaining:0.0}s", centerStyle);
        GUI.Label(new Rect(left, top + 214, width, 28), feedback, centerStyle);
        GUI.Label(new Rect(left, top + 252, width, 28), "按对应 WASD · Backspace 退出", centerStyle);
    }
}
