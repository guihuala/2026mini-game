using UnityEngine;

public class MainMenuVisionController : MonoBehaviour
{
    [Header("Visible Content")]
    [Tooltip("UI shown in blue vision and visually hidden in red vision.")]
    [SerializeField] private GameObject[] blueVisionObjects;
    [Tooltip("UI shown in red vision and visually hidden in blue vision.")]
    [SerializeField] private GameObject[] redVisionObjects;

    [Header("Initial State")]
    [SerializeField] private VisionMode initialMode = VisionMode.Blue;

    public VisionMode CurrentMode { get; private set; }

    private void Start()
    {
        SetVision(initialMode);
    }

    private void Update()
    {
        bool switchPressed = InputManager.Instance != null &&
                             InputManager.Instance.GetActionDown(InputActionType.SwitchVision);

        // Keep the main menu usable when opened directly without the persistent managers.
        if (switchPressed || Input.GetKeyDown(KeyCode.Tab))
            ToggleVision();
    }

    public void ToggleVision()
    {
        VisionMode nextMode = CurrentMode == VisionMode.Blue
            ? VisionMode.Red
            : VisionMode.Blue;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(nextMode == VisionMode.Red ? "切换镜片1" : "切换镜片2");

        SetVision(nextMode);
    }

    public void SetVision(VisionMode mode)
    {
        CurrentMode = mode;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayVisionModeBgm(mode);

        SetVisualState(blueVisionObjects, mode == VisionMode.Blue);
        SetVisualState(redVisionObjects, mode == VisionMode.Red);
    }

    private static void SetVisualState(GameObject[] objects, bool visible)
    {
        if (objects == null)
            return;

        for (int i = 0; i < objects.Length; i++)
        {
            GameObject target = objects[i];
            if (target == null)
                continue;

            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = target.AddComponent<CanvasGroup>();

            // Alpha only affects rendering. Raycasts and interactability deliberately
            // stay enabled, so hidden menu elements can still be hovered and clicked.
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
