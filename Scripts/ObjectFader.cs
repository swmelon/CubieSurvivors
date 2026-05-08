using System.Collections;
using UnityEngine;

public class ObjectFader : MonoBehaviour
{
    public float fadeDuration;
    public Material opaqueMaterial; // Assign this in the inspector
    public Material transparentMaterial; // Assign this in the inspector
    private Renderer rend;
    private bool isFading = false;
    private float fadeAmount;

    public bool DoFade
    {
        get => isFading;
        set
        {
            if (isFading != value)
            {
                if (!gameObject.activeSelf)
                {
                    return;
                }

                isFading = value;
                // Start coroutine to fade
                StopAllCoroutines(); // Stop all running coroutines before starting a new one

                if (value)
                {
                    StartCoroutine(FadeTo(true, rend.material.color.a, fadeDuration));
                    rend.material = transparentMaterial;
                }
                else
                {
                    StartCoroutine(FadeTo(false, rend.material.color.a, fadeDuration));
                    // when fhinished, assign opaque material
                }
            }
        }
    }

    private void Awake()
    {
        rend = GetComponent<Renderer>();
        fadeAmount = transparentMaterial.color.a;
    }

    private void OnEnable()
    {
        rend.material = opaqueMaterial;
        rend.enabled = true;
        isFading = false;
    }

    IEnumerator FadeTo(bool fadeOut, float startAlpha, float duration)
    {
        float time = 0;
        float targetAlpha = fadeOut ? fadeAmount : 1f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            Color color = rend.material.color;
            color.a = alpha;
            rend.material.color = color;

            yield return null; // Wait for the next frame
        }

        // Ensure the final alpha is set after the loop
        Color finalColor = rend.material.color;
        finalColor.a = targetAlpha;
        rend.material.color = finalColor;

        if (!fadeOut)
        {
            rend.material = opaqueMaterial;
        }        
    }
}
