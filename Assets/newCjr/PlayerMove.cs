using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [Header("Movement Animation")]
    [SerializeField, Min(1f)] private float animationFramesPerSecond = 6f;
    [SerializeField] private Sprite idleDown;
    [SerializeField] private Sprite idleUp;
    [SerializeField] private Sprite idleLeft;
    [SerializeField] private Sprite idleRight;
    [SerializeField] private Sprite[] walkDown;
    [SerializeField] private Sprite[] walkUp;
    [SerializeField] private Sprite[] walkLeft;
    [SerializeField] private Sprite[] walkRight;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector2 facingDirection = Vector2.down;
    private bool canMove = true;
    private Coroutine disableRoutine;
    private float animationTimer;
    private int animationFrame;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
        ShowIdleSprite();
    }

    private void Update()
    {
        if (canMove)
        {
            if (InputManager.Instance != null)
            {
                moveInput = InputManager.Instance.Move;
            }
            else
            {
                moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
                if (moveInput.sqrMagnitude > 1f)
                    moveInput.Normalize();
            }
        }

        UpdateMovementAnimation();
    }

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = moveInput * moveSpeed;
    }

    private void UpdateMovementAnimation()
    {
        if (!canMove || moveInput.sqrMagnitude < 0.0001f)
        {
            animationTimer = 0f;
            animationFrame = 0;
            ShowIdleSprite();
            return;
        }

        Vector2 newFacing = GetCardinalDirection(moveInput);
        if (newFacing != facingDirection)
        {
            facingDirection = newFacing;
            animationTimer = 0f;
            animationFrame = 0;
        }

        Sprite[] frames = GetWalkFrames();
        if (frames == null || frames.Length == 0)
        {
            ShowIdleSprite();
            return;
        }

        animationTimer += Time.deltaTime;
        float frameDuration = 1f / animationFramesPerSecond;
        while (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;
            animationFrame = (animationFrame + 1) % frames.Length;
        }

        spriteRenderer.sprite = frames[animationFrame];
    }

    private static Vector2 GetCardinalDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            return direction.x > 0f ? Vector2.right : Vector2.left;

        return direction.y > 0f ? Vector2.up : Vector2.down;
    }

    private Sprite[] GetWalkFrames()
    {
        if (facingDirection == Vector2.up) return walkUp;
        if (facingDirection == Vector2.left) return walkLeft;
        if (facingDirection == Vector2.right) return walkRight;
        return walkDown;
    }

    private void ShowIdleSprite()
    {
        Sprite idleSprite;
        if (facingDirection == Vector2.up)
            idleSprite = idleUp;
        else if (facingDirection == Vector2.left)
            idleSprite = idleLeft;
        else if (facingDirection == Vector2.right)
            idleSprite = idleRight;
        else
            idleSprite = idleDown;

        if (idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    public void DisableInput(float duration)
    {
        if (disableRoutine != null)
            StopCoroutine(disableRoutine);
        disableRoutine = StartCoroutine(DisableRoutine(duration));
    }

    private IEnumerator DisableRoutine(float duration)
    {
        canMove = false;
        moveInput = Vector2.zero;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(duration);
        canMove = true;
        disableRoutine = null;
    }
}
