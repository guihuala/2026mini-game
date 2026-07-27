using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    
    private MonsterMovement movement;
    [Tooltip("按顺序轮播。只配置一张时始终显示该图；留空时使用 GameManager 默认图片。")]
    [SerializeField] private Sprite[] caughtImages;
    [HideInInspector]
    [SerializeField] private Sprite caughtImageOverride;
    private bool hasCaughtPlayer;

    private void Awake()
    {
        movement = GetComponent<MonsterMovement>();
        GetComponent<Rigidbody2D>().gravityScale = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OnTouchPlayer(other);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            OnTouchPlayer(collision.collider);
        }
    }

    private void OnTouchPlayer(Collider2D playerCollider)
    {
        PlayerHidingState hidingState = playerCollider.GetComponentInParent<PlayerHidingState>();
        if (hidingState != null && hidingState.IsHidden) return;
        if (hasCaughtPlayer) return;
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        hasCaughtPlayer = true;
        Debug.Log($"怪物 {name} 碰到了玩家！");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx("吓人");

        foreach (MonsterMovement monster in FindObjectsOfType<MonsterMovement>(true))
            monster.Pause();

        Sprite[] images = caughtImages != null && caughtImages.Length > 0
            ? caughtImages
            : caughtImageOverride != null
                ? new[] { caughtImageOverride }
                : null;
        GameManager.Instance.PlayMonsterCaughtSequence(images);
    }
}
