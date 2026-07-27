using System;
using UnityEngine;

public enum VisionMode
{
    Blue,
    Red
}

public class VisionModeController : MonoBehaviour
{
    [Header("Visible Content")]
    [Tooltip("Only rendering is toggled so the map colliders remain active in both modes.")]
    [SerializeField] private Renderer[] mapRenderers;
    [Tooltip("Only the renderers under these objects are shown in red vision; their colliders and behaviours stay active.")]
    [SerializeField] private GameObject[] interactableObjects;
    [Tooltip("Automatically treats every Portal in the scene as a red-vision interactable.")]
    [SerializeField] private bool includePortalsAsRedInteractables = true;

    [Header("Blue Vision Range")]
    [Tooltip("Maximum sight distance in Unity world units.")]
    [SerializeField, Min(0.1f)] private float blueViewDistance = 8f;
    [Tooltip("Width of the sight cone in degrees.")]
    [SerializeField, Range(1f, 360f)] private float blueViewAngle = 90f;
    [Tooltip("More rays produce smoother wall edges but cost more physics queries.")]
    [SerializeField, Range(8, 360)] private int blueViewRayCount = 121;
    [Tooltip("Collider layers that block blue vision.")]
    [SerializeField] private LayerMask blueVisionWallLayers = ~0;

    [Header("Initial State")]
    [SerializeField] private VisionMode initialMode = VisionMode.Blue;

    [Header("Visual Feedback")]
    [SerializeField] private Color blueTint = new Color(0.05f, 0.3f, 1f, 0.12f);
    [SerializeField] private Color redTint = new Color(1f, 0.05f, 0.05f, 0.16f);
    [SerializeField] private bool showModeLabel = true;

    public VisionMode CurrentMode { get; private set; }
    public bool IsBlueVision => CurrentMode == VisionMode.Blue;
    public bool IsRedVision => CurrentMode == VisionMode.Red;

    public event Action<VisionMode> VisionChanged;

    private BlueVisionExploration blueVisionExploration;

    private void Start()
    {
        blueVisionExploration = GetComponent<BlueVisionExploration>();
        if (blueVisionExploration == null)
            blueVisionExploration = gameObject.AddComponent<BlueVisionExploration>();
        blueVisionExploration.Initialize(
            mapRenderers,
            blueViewDistance,
            blueViewAngle,
            blueViewRayCount,
            blueVisionWallLayers);

        SetVision(initialMode, false);
    }

    private void Update()
    {
        bool switchPressed = InputManager.Instance != null &&
                             InputManager.Instance.GetActionDown(InputActionType.SwitchVision);

        // Keeps the scene test usable even if the persistent manager has not
        // finished initializing or this scene is played directly in isolation.
        if (switchPressed || Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleVision();
        }
    }

    private void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;

        Color previousColor = GUI.color;
        GUI.color = IsBlueVision ? blueTint : redTint;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = previousColor;

        if (!showModeLabel)
            return;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Max(18, Screen.height / 32),
            fontStyle = FontStyle.Bold
        };
        labelStyle.normal.textColor = IsBlueVision
            ? new Color(0.55f, 0.8f, 1f)
            : new Color(1f, 0.55f, 0.55f);

        string label = IsBlueVision ? "BLUE VISION  [TAB]" : "RED VISION  [TAB]";
        GUI.Box(new Rect(20f, 20f, 260f, 46f), label, labelStyle);
    }

    public void ToggleVision()
    {
        VisionMode nextMode = IsBlueVision ? VisionMode.Red : VisionMode.Blue;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySfx(nextMode == VisionMode.Red ? "切换镜片1" : "切换镜片2");
        SetVision(nextMode);
    }

    public void SetVision(VisionMode mode, bool notify = true)
    {
        CurrentMode = mode;

        bool blueActive = mode == VisionMode.Blue;
        if (blueVisionExploration != null)
            blueVisionExploration.enabled = blueActive;

        for (int i = 0; i < mapRenderers.Length; i++)
        {
            if (mapRenderers[i] != null)
                mapRenderers[i].enabled = blueActive;
        }

        bool redActive = mode == VisionMode.Red;
        for (int i = 0; i < interactableObjects.Length; i++)
        {
            if (interactableObjects[i] != null)
            {
                Renderer[] renderers = interactableObjects[i].GetComponentsInChildren<Renderer>(true);
                for (int j = 0; j < renderers.Length; j++)
                {
                    renderers[j].enabled = redActive;
                }
            }
        }

        if (includePortalsAsRedInteractables)
        {
            Portal[] portals = FindObjectsOfType<Portal>(true);
            for (int i = 0; i < portals.Length; i++)
            {
                if (portals[i] != null)
                    portals[i].gameObject.SetActive(redActive);
            }
        }

        if (notify)
            VisionChanged?.Invoke(mode);
    }
}
