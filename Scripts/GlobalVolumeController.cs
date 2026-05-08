using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlobalVolumeController : MonoBehaviour
{
    [SerializeField]
    private FloatEventChannelSO vignetteValueChangedChannel;

    private Volume globalVolume;
    private Vignette vignetteEffect;

    void Start()
    {
        // Find the Global Volume component on start
        globalVolume = FindObjectOfType<Volume>();

        // Try to get the Bloom effect from the Global Volume
        if (globalVolume.profile.TryGet<Vignette>(out vignetteEffect))
        {
            // Successfully retrieved Bloom effect
            Debug.Log("Vignette effect found!");
        }
    }

    private void OnEnable()
    {
        // Subscribe to the event
        vignetteValueChangedChannel.Subscribe(SetVignetteIntensity);
    }

    private void OnDisable()
    {
        // Unsubscribe from the event
        vignetteValueChangedChannel.Unsubscribe(SetVignetteIntensity);
    }

    public void SetVignetteIntensity(float intensity)
    {
        intensity = Mathf.Clamp(intensity, 0, 1);

        if (vignetteEffect != null)
        {
            // Set the new intensity for the Bloom effect
            vignetteEffect.intensity.value = intensity;
            Debug.Log($"Bloom intensity set to: {intensity}");
        }
    }
}
