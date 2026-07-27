using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Finish : MonoBehaviour
{
    [Header("通关 CG（可选）")]
    [Tooltip("配置后，胜利结算时会依次全屏播放这些图片；留空则直接显示胜利面板。")]
    [SerializeField] private Sprite[] endingCGFrames;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        OnPlayerReachFinish();
    }

    private bool triggered;

    private void OnPlayerReachFinish()
    {
        if (triggered) return;
        triggered = true;

        foreach (var monster in FindObjectsOfType<MonsterMovement>(true))
            monster.Pause();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteLevel();

            if (endingCGFrames != null && endingCGFrames.Length > 0)
                EndingCGPlayer.Play(endingCGFrames);
        }
        else
        {
            Debug.LogError("到达终点时未找到 GameManager，无法完成关卡。", this);
        }
    }
}
