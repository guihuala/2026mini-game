using UnityEngine;
using UnityEngine.UI;

public sealed class DirectionMinigamePanel : BasePanel
{
    [SerializeField] private Text directionText;
    [SerializeField] private Text progressText;
    [SerializeField] private Text timeText;
    [SerializeField] private Text feedbackText;

    public void SetView(string direction, int progress, int total, float remaining, string feedback)
    {
        directionText.text = direction;
        progressText.text = $"进度  {progress} / {total}";
        timeText.text = $"剩余时间  {remaining:0.0}s";
        feedbackText.text = feedback ?? string.Empty;
    }
}
