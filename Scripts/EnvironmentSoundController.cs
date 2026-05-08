using UnityEngine;
using FMODUnity;

public class EnvironmentSoundController : MonoBehaviour
{
    public float checkRadius = 30f;  // Radius to check for burning objects
    public int minBurningObjects = 10;  // Minimum number of burning objects to trigger sound
    public StudioEventEmitter burningSoundEmitter;  // FMOD Event Emitter for burning sound

    void Update()
    {
        // Check for burning objects around the player or a central point
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, checkRadius);
        int burningCount = 0;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Burning"))  // Check if the collider has the "Burning" tag
            {
                burningCount++;
            }
        }

        // Play or adjust sound based on the number of burning objects
        if (burningCount >= minBurningObjects)
        {
            if (!burningSoundEmitter.IsPlaying())
            {
                burningSoundEmitter.Play();  // Play sound if not already playing
            }
            // Optionally, adjust sound parameters based on the count of burning objects
            burningSoundEmitter.SetParameter("Intensity", Mathf.Clamp01((float)burningCount / 50));  // Example parameter
        }
        else
        {
            burningSoundEmitter.Stop();  // Stop the sound if below the threshold
        }
    }

    private void OnDestroy()
    {
        burningSoundEmitter.Stop();  // Stop the sound when the object is destroyed
    }
}
