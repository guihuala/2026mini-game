using UnityEngine;

/// <summary>
/// 可叠加的 2D 相机震动。无需预先挂载组件，直接调用静态接口即可。
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(10000)]
public sealed class CameraShake : MonoBehaviour
{
    private float remainingTime;
    private float duration;
    private float strength;
    private float frequency;
    private float seedX;
    private float seedY;
    private Vector3 appliedOffset;

    /// <summary>
    /// 震动主相机。重复调用会保留更强、持续更久的震动。
    /// </summary>
    /// <param name="duration">持续时间（秒），不受 Time.timeScale 影响。</param>
    /// <param name="strength">世界空间中的最大位移。</param>
    /// <param name="frequency">每秒抖动变化次数。</param>
    public static void Shake(float duration = 0.2f, float strength = 0.12f, float frequency = 24f)
    {
        if (duration <= 0f || strength <= 0f)
            return;

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("CameraShake: 场景中没有带 MainCamera 标签的相机。");
            return;
        }

        CameraShake shaker = mainCamera.GetComponent<CameraShake>();
        if (shaker == null)
            shaker = mainCamera.gameObject.AddComponent<CameraShake>();

        shaker.Play(duration, strength, frequency);
    }

    /// <summary>
    /// 立即停止主相机震动。
    /// </summary>
    public static void Stop()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
            return;

        CameraShake shaker = mainCamera.GetComponent<CameraShake>();
        if (shaker != null)
            shaker.StopInternal();
    }

    private void Play(float newDuration, float newStrength, float newFrequency)
    {
        duration = Mathf.Max(duration, newDuration);
        remainingTime = Mathf.Max(remainingTime, newDuration);
        strength = Mathf.Max(strength, newStrength);
        frequency = Mathf.Max(1f, newFrequency);
        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
        enabled = true;
    }

    private void LateUpdate()
    {
        // 先移除上一帧偏移，以兼容同一相机上的跟随脚本。
        transform.localPosition -= appliedOffset;
        appliedOffset = Vector3.zero;

        if (remainingTime <= 0f)
        {
            ClearState();
            return;
        }

        remainingTime = Mathf.Max(0f, remainingTime - Time.unscaledDeltaTime);
        float envelope = duration <= 0f ? 0f : remainingTime / duration;
        float sampleTime = Time.unscaledTime * frequency;
        float x = Mathf.PerlinNoise(seedX, sampleTime) * 2f - 1f;
        float y = Mathf.PerlinNoise(seedY, sampleTime) * 2f - 1f;

        appliedOffset = new Vector3(x, y, 0f) * (strength * envelope);
        transform.localPosition += appliedOffset;
    }

    private void OnDisable()
    {
        transform.localPosition -= appliedOffset;
        appliedOffset = Vector3.zero;
    }

    private void StopInternal()
    {
        transform.localPosition -= appliedOffset;
        appliedOffset = Vector3.zero;
        ClearState();
    }

    private void ClearState()
    {
        remainingTime = 0f;
        duration = 0f;
        strength = 0f;
        enabled = false;
    }
}
