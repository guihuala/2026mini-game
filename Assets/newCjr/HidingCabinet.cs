using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HidingCabinet : MonoBehaviour
{
    [Header("Cabinet")]
    [SerializeField] private Transform hidingPoint;
    [SerializeField] private KeyCode fallbackInteractKey = KeyCode.E;
    [SerializeField] private string enterPrompt = "按 E 躲进柜子";
    [SerializeField] private string exitPrompt = "按 E 离开柜子";

    [Header("View")]
    [SerializeField] private float promptWidth = 240f;
    [SerializeField] private float promptHeight = 42f;

    private PlayerHidingState nearbyPlayer;
    private PlayerMove playerMove;
    private HideoutVisionMask visionMask;
    private bool occupied;
    private bool playerInRange;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        if (hidingPoint == null) hidingPoint = transform;
    }

    private void Update()
    {
        if ((!playerInRange && !occupied) || nearbyPlayer == null) return;

        bool interactPressed = InputManager.Instance != null
            ? InputManager.Instance.GetActionDown(InputActionType.Interact)
            : Input.GetKeyDown(fallbackInteractKey);

        if (!interactPressed) return;
        if (occupied) ExitCabinet();
        else EnterCabinet();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        GameObject playerObject = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;
        nearbyPlayer = playerObject.GetComponent<PlayerHidingState>();
        if (nearbyPlayer == null)
            nearbyPlayer = playerObject.AddComponent<PlayerHidingState>();
        playerMove = nearbyPlayer.GetComponent<PlayerMove>();
        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || occupied) return;
        playerInRange = false;
        nearbyPlayer = null;
        playerMove = null;
    }

    private void EnterCabinet()
    {
        if (nearbyPlayer == null || nearbyPlayer.IsHidden) return;

        occupied = true;
        if (playerMove != null) playerMove.SetMovementEnabled(false);

        Rigidbody2D body = nearbyPlayer.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.velocity = Vector2.zero;
            body.position = hidingPoint.position;
        }
        else
        {
            nearbyPlayer.transform.position = hidingPoint.position;
        }

        nearbyPlayer.SetHidden(this, true);
        EnsureVisionMask().SetVisible(true);
    }

    private void ExitCabinet()
    {
        if (!occupied || nearbyPlayer == null) return;

        nearbyPlayer.SetHidden(this, false);
        occupied = false;
        EnsureVisionMask().SetVisible(false);
        if (playerMove != null) playerMove.SetMovementEnabled(true);
    }

    private HideoutVisionMask EnsureVisionMask()
    {
        if (visionMask != null) return visionMask;
        GameObject maskObject = new GameObject("Cabinet Vision Mask");
        visionMask = maskObject.AddComponent<HideoutVisionMask>();
        return visionMask;
    }

    private void OnGUI()
    {
        if (nearbyPlayer == null || (!playerInRange && !occupied)) return;
        string prompt = occupied ? exitPrompt : enterPrompt;
        Rect rect = new Rect(
            (Screen.width - promptWidth) * 0.5f,
            Screen.height - promptHeight - 36f,
            promptWidth,
            promptHeight);
        GUI.Box(rect, prompt);
    }
}
