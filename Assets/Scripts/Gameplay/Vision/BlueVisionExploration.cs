using UnityEngine;

/// <summary>
/// Permanently reveals a wall-occluded cone in front of the player.
/// The reveal texture is sampled in world space by every blue-map renderer.
/// </summary>
public class BlueVisionExploration : MonoBehaviour
{
    [Header("View Cone")]
    [SerializeField, Min(0.1f)] private float viewDistance = 8f;
    [SerializeField, Range(1f, 360f)] private float viewAngle = 90f;
    [SerializeField, Range(8, 360)] private int rayCount = 121;
    [SerializeField] private LayerMask wallLayers = ~0;

    [Header("Exploration Texture")]
    [SerializeField, Range(1f, 32f)] private float pixelsPerUnit = 8f;

    private readonly RaycastHit2D[] hits = new RaycastHit2D[32];
    private Renderer[] mapRenderers;
    private Material[] originalMaterials;
    private Material[] revealMaterials;
    private Transform player;
    private Rigidbody2D playerBody;
    private Vector2 facingDirection = Vector2.down;
    private Texture2D exploredTexture;
    private Color32[] exploredPixels;
    private Bounds mapBounds;
    private bool initialized;

    public void Initialize(
        Renderer[] renderers,
        float distance,
        float angle,
        int rays,
        LayerMask blockingLayers)
    {
        mapRenderers = renderers;
        viewDistance = Mathf.Max(0.1f, distance);
        viewAngle = Mathf.Clamp(angle, 1f, 360f);
        rayCount = Mathf.Clamp(rays, 8, 360);
        wallLayers = blockingLayers;
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        player = playerObject != null ? playerObject.transform : null;
        playerBody = playerObject != null ? playerObject.GetComponent<Rigidbody2D>() : null;

        if (player == null || mapRenderers == null || !TryCalculateMapBounds())
        {
            Debug.LogWarning("Blue vision could not find its player or map renderers.", this);
            enabled = false;
            return;
        }

        mapBounds.Expand(0.5f);
        int width = Mathf.Clamp(Mathf.CeilToInt(mapBounds.size.x * pixelsPerUnit), 1, 2048);
        int height = Mathf.Clamp(Mathf.CeilToInt(mapBounds.size.y * pixelsPerUnit), 1, 2048);
        exploredTexture = new Texture2D(width, height, TextureFormat.RGBA32, false, true)
        {
            name = "Blue Vision Explored Area",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        exploredPixels = new Color32[width * height];
        exploredTexture.SetPixels32(exploredPixels);
        exploredTexture.Apply(false, false);

        Shader revealShader = Resources.Load<Shader>("BlueVisionReveal");
        if (revealShader == null)
        {
            Debug.LogError("Resources/BlueVisionReveal.shader is missing.", this);
            enabled = false;
            return;
        }

        originalMaterials = new Material[mapRenderers.Length];
        revealMaterials = new Material[mapRenderers.Length];
        Vector4 worldRect = new Vector4(
            mapBounds.min.x, mapBounds.min.y, mapBounds.size.x, mapBounds.size.y);

        for (int i = 0; i < mapRenderers.Length; i++)
        {
            Renderer mapRenderer = mapRenderers[i];
            if (mapRenderer == null)
                continue;

            originalMaterials[i] = mapRenderer.sharedMaterial;
            Material material = new Material(revealShader)
            {
                name = mapRenderer.name + " Blue Vision Material"
            };
            material.SetTexture("_ExploredTex", exploredTexture);
            material.SetVector("_WorldRect", worldRect);
            revealMaterials[i] = material;
            mapRenderer.sharedMaterial = material;
        }

        initialized = true;
        RevealVisibleArea();
    }

    private void Update()
    {
        RevealVisibleArea();
    }

    private bool TryCalculateMapBounds()
    {
        bool foundRenderer = false;
        for (int i = 0; i < mapRenderers.Length; i++)
        {
            if (mapRenderers[i] == null)
                continue;

            if (!foundRenderer)
            {
                mapBounds = mapRenderers[i].bounds;
                foundRenderer = true;
            }
            else
            {
                mapBounds.Encapsulate(mapRenderers[i].bounds);
            }
        }
        return foundRenderer;
    }

    private void RevealVisibleArea()
    {
        if (!initialized || player == null)
            return;

        Vector2 origin = player.position;
        UpdateFacingDirection();
        Vector2 facing = facingDirection;
        float centerAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        int currentRayCount = Mathf.Max(2, rayCount);
        bool changed = MarkExplored(origin);

        for (int i = 0; i < currentRayCount; i++)
        {
            float t = i / (float)(currentRayCount - 1);
            float angle = centerAngle + Mathf.Lerp(-viewAngle * 0.5f, viewAngle * 0.5f, t);
            float radians = angle * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            changed |= MarkRay(origin, direction, GetVisibleDistance(origin, direction));
        }

        if (!changed)
            return;

        exploredTexture.SetPixels32(exploredPixels);
        exploredTexture.Apply(false, false);
    }

    private float GetVisibleDistance(Vector2 origin, Vector2 direction)
    {
        int hitCount = Physics2D.RaycastNonAlloc(
            origin, direction, hits, viewDistance, wallLayers);
        float nearest = viewDistance;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = hits[i].collider;
            if (hitCollider == null || hitCollider.isTrigger ||
                hitCollider.transform == player ||
                hitCollider.transform.IsChildOf(player))
                continue;

            nearest = Mathf.Min(nearest, hits[i].distance);
        }
        return nearest;
    }

    private void UpdateFacingDirection()
    {
        if (playerBody == null || playerBody.velocity.sqrMagnitude < 0.0001f)
            return;

        Vector2 velocity = playerBody.velocity;
        if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y))
            facingDirection = velocity.x > 0f ? Vector2.right : Vector2.left;
        else
            facingDirection = velocity.y > 0f ? Vector2.up : Vector2.down;
    }

    private bool MarkRay(Vector2 origin, Vector2 direction, float distance)
    {
        bool changed = false;
        float step = 0.5f / pixelsPerUnit;
        for (float current = 0f; current <= distance; current += step)
            changed |= MarkExplored(origin + direction * current);
        return changed;
    }

    private bool MarkExplored(Vector2 worldPosition)
    {
        float normalizedX = Mathf.InverseLerp(mapBounds.min.x, mapBounds.max.x, worldPosition.x);
        float normalizedY = Mathf.InverseLerp(mapBounds.min.y, mapBounds.max.y, worldPosition.y);
        int x = Mathf.RoundToInt(normalizedX * (exploredTexture.width - 1));
        int y = Mathf.RoundToInt(normalizedY * (exploredTexture.height - 1));
        bool changed = false;

        // A small brush closes sub-pixel gaps between neighbouring rays.
        for (int offsetY = -1; offsetY <= 1; offsetY++)
        {
            int pixelY = y + offsetY;
            if (pixelY < 0 || pixelY >= exploredTexture.height)
                continue;

            for (int offsetX = -1; offsetX <= 1; offsetX++)
            {
                int pixelX = x + offsetX;
                if (pixelX < 0 || pixelX >= exploredTexture.width)
                    continue;

                int index = pixelY * exploredTexture.width + pixelX;
                if (exploredPixels[index].r == 255)
                    continue;

                exploredPixels[index] = new Color32(255, 255, 255, 255);
                changed = true;
            }
        }
        return changed;
    }

    private void OnDestroy()
    {
        if (mapRenderers != null && originalMaterials != null)
        {
            for (int i = 0; i < mapRenderers.Length; i++)
            {
                if (mapRenderers[i] != null)
                    mapRenderers[i].sharedMaterial = originalMaterials[i];
            }
        }

        if (revealMaterials != null)
        {
            for (int i = 0; i < revealMaterials.Length; i++)
            {
                if (revealMaterials[i] != null)
                    Destroy(revealMaterials[i]);
            }
        }

        if (exploredTexture != null)
            Destroy(exploredTexture);
    }
}
