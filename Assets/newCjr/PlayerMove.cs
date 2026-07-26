using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool canMove = true;
    private Coroutine disableRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.sleepMode = RigidbodySleepMode2D.NeverSleep;
    }

    private void Update()
    {
        if (!canMove) return;

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

    private void FixedUpdate()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = moveInput * moveSpeed;
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
