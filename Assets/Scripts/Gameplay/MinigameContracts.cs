using System;

[Serializable]
public sealed class MinigameContext
{
    public string minigameId;
    public int difficulty = 1;
    public int seed;
}

[Serializable]
public sealed class MinigameResult
{
    public string minigameId;
    public bool succeeded;
    public bool cancelled;
    public int score;
    public float durationSeconds;
    public string resultTag;
}

public interface IMinigame
{
    bool IsRunning { get; }
    void StartGame(MinigameContext context, Action<MinigameResult> onCompleted);
    void CancelGame();
}
