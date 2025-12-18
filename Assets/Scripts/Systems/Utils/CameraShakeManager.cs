using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// A centralized manager for camera shake effects using Feel's MMFeedbacks system.
/// Attach this to a GameObject in your scene and reference it from other scripts to trigger camera shakes.
/// </summary>
public class CameraShakeManager : MonoBehaviour
{
    [Header("Shake References")]
    [Tooltip("Reference to the MMF_Player that contains camera shake feedbacks")]
    [SerializeField] private MMF_Player cameraShakeFeedback;

    [Header("Predefined Shake Presets")]
    [Tooltip("Light shake for minor impacts (duration, amplitude, frequency)")]
    [SerializeField] private MMCameraShakeProperties lightShake = new MMCameraShakeProperties(0.1f, 0.1f, 20f);

    [Tooltip("Medium shake for moderate impacts")]
    [SerializeField] private MMCameraShakeProperties mediumShake = new MMCameraShakeProperties(0.2f, 0.3f, 30f);

    [Tooltip("Heavy shake for major impacts")]
    [SerializeField] private MMCameraShakeProperties heavyShake = new MMCameraShakeProperties(0.3f, 0.5f, 40f);

    [Header("Channel Settings")]
    [Tooltip("The channel to broadcast shake events on (must match your camera shaker)")]
    [SerializeField] private int shakeChannel = 0;

    [Tooltip("Whether to use a MMChannel scriptable object instead of an int")]
    [SerializeField] private MMChannelModes channelMode = MMChannelModes.Int;

    [Tooltip("Optional MMChannel definition for more organized channel management")]
    [SerializeField] private MMChannel mmChannelDefinition = null;

    // Singleton instance for easy access
    public static CameraShakeManager Instance { get; private set; }

    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Trigger a light camera shake (e.g., for minor hits or pickups)
    /// </summary>
    public void ShakeLight()
    {
        TriggerShake(lightShake);
    }

    /// <summary>
    /// Trigger a medium camera shake (e.g., for player damage or enemy attacks)
    /// </summary>
    public void ShakeMedium()
    {
        TriggerShake(mediumShake);
    }

    /// <summary>
    /// Trigger a heavy camera shake (e.g., for explosions or boss attacks)
    /// </summary>
    public void ShakeHeavy()
    {
        TriggerShake(heavyShake);
    }

    /// <summary>
    /// Trigger a custom camera shake with specific properties
    /// </summary>
    /// <param name="duration">How long the shake lasts in seconds</param>
    /// <param name="amplitude">The intensity of the shake</param>
    /// <param name="frequency">How fast the shake oscillates</param>
    public void ShakeCustom(float duration, float amplitude, float frequency)
    {
        MMCameraShakeProperties customShake = new MMCameraShakeProperties(duration, amplitude, frequency);
        TriggerShake(customShake);
    }

    /// <summary>
    /// Trigger a camera shake with custom amplitude per axis
    /// </summary>
    /// <param name="duration">How long the shake lasts in seconds</param>
    /// <param name="amplitudeX">Shake intensity on X axis</param>
    /// <param name="amplitudeY">Shake intensity on Y axis</param>
    /// <param name="amplitudeZ">Shake intensity on Z axis</param>
    /// <param name="frequency">How fast the shake oscillates</param>
    public void ShakeCustomAxes(float duration, float amplitudeX, float amplitudeY, float amplitudeZ, float frequency)
    {
        MMCameraShakeProperties customShake = new MMCameraShakeProperties(
            duration,
            0f, // General amplitude not used when using per-axis
            frequency,
            amplitudeX,
            amplitudeY,
            amplitudeZ
        );
        TriggerShake(customShake);
    }

    /// <summary>
    /// Use the MMF_Player component to trigger the shake (if assigned)
    /// </summary>
    public void PlayFeedbackShake()
    {
        if (cameraShakeFeedback != null)
        {
            cameraShakeFeedback.PlayFeedbacks();
        }
        else
        {
            Debug.LogWarning("CameraShakeManager: No MMF_Player assigned! Please assign one in the inspector or use event-based methods.");
        }
    }

    /// <summary>
    /// Stop any ongoing infinite shake
    /// </summary>
    public void StopShake()
    {
        MMChannelData channelData = new MMChannelData(channelMode, shakeChannel, mmChannelDefinition);
        MMCameraShakeStopEvent.Trigger(channelData);

        if (cameraShakeFeedback != null)
        {
            cameraShakeFeedback.StopFeedbacks();
        }
    }

    /// <summary>
    /// Internal method to trigger a shake event
    /// </summary>
    private void TriggerShake(MMCameraShakeProperties shakeProperties)
    {
        MMChannelData channelData = new MMChannelData(channelMode, shakeChannel, mmChannelDefinition);

        MMCameraShakeEvent.Trigger(
            shakeProperties.Duration,
            shakeProperties.Amplitude,
            shakeProperties.Frequency,
            shakeProperties.AmplitudeX,
            shakeProperties.AmplitudeY,
            shakeProperties.AmplitudeZ,
            false, // infinite
            channelData,
            false  // useUnscaledTime
        );
    }

#if UNITY_EDITOR
    [Header("Testing")]
    [SerializeField] private bool testInEditor = false;

    private void OnValidate()
    {
        if (testInEditor)
        {
            testInEditor = false;
            if (Application.isPlaying)
            {
                ShakeMedium();
            }
        }
    }
#endif
}
