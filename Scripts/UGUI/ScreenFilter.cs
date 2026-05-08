
using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFilter : MonoBehaviour
{
    private enum Mode
    {
        None,
        Fading,
        Flashing
    }
    [SerializeField]
    private EventChannelSO playerHitEventChannel;

    [SerializeField]
    private EventChannelSO defeatFinalBossEC, enterEventStageEC;
    
    private Image image;
    private Color flashColor = new Color(1, 0, 0, 0.3f); // RGBA, e.g., semi-transparent red
    public float flashDuration = 0.1f;
    public WaitForSeconds flashWait;
    public float flashFadeOutDuration = 0.5f;

    private float fadeDuration = 1f;
    private float time = 0f;
    private Mode mode = Mode.None;

    [SerializeField]
    private float darkenAlpha = 0.5f;

    private Color startColor, endColor;
    
    private void Awake()
    {
        image = GetComponent<Image>();
        flashWait = new WaitForSeconds(flashDuration);
    }

    private void IgnoreFlashEvent()
    {
        playerHitEventChannel.Unsubscribe(FlashScreen);
    }

    private void ListenFlashEvent()
    {
        playerHitEventChannel.Subscribe(FlashScreen);
    }

    private void OnEnable()
    {
        playerHitEventChannel.Subscribe(FlashScreen);
        defeatFinalBossEC.Subscribe(IgnoreFlashEvent);
        enterEventStageEC.Subscribe(ListenFlashEvent);
    }
    
    private void OnDisable()
    {
        playerHitEventChannel.Unsubscribe(FlashScreen);
        defeatFinalBossEC.Unsubscribe(IgnoreFlashEvent);
        enterEventStageEC.Unsubscribe(ListenFlashEvent);
    }

    private void FlashScreen()
    {
        if (mode == Mode.Fading) return;

        image.color = flashColor;
        time = 0f;
        mode = Mode.Flashing;
    }
    
    public void FadeIn()
    {
        startColor = Color.black;
        startColor.a = 1;

        if (mode == Mode.Fading)
        {
            startColor.a = image.color.a;
        }

        mode = Mode.Fading;
        endColor = Color.black;
        endColor.a = 0;
        time = 0f;
    }

    public void FadeOut()
    {
        startColor = Color.black;
        startColor.a = 0;

        if (mode == Mode.Fading)
        {
            startColor.a = image.color.a;
        }

        mode = Mode.Fading;
        endColor = Color.black;
        time = 0f;
    }

    public void DarkenScreen() 
    {
        FadeOut();
        endColor.a = darkenAlpha;
    }

    public void LightenScreen()
    {
        FadeIn();
        startColor.a = darkenAlpha;
    }


    public void FadeOutWhite()
    {
        startColor = Color.white;
        startColor.a = 0f;

        if (mode == Mode.Fading)
        {
            startColor.a = image.color.a;
        }

        mode = Mode.Fading;
        endColor = Color.white;
        time = 0f;
    }

    public void FadeInWhite()
    {
        startColor = Color.white;
        startColor.a = 1f;

        if (mode == Mode.Fading)
        {
            startColor.a = image.color.a;
            return;
        }

        mode = Mode.Fading;
        endColor = Color.white;
        endColor.a = 0;
        time = 0f;
    }

    public void FadeOutAndIn()
    {
        StartCoroutine(FadeOutAndInCoroutine());
    }

    private IEnumerator FadeOutAndInCoroutine()
    {
        FadeOut();
        yield return new WaitForSeconds(fadeDuration);
        FadeIn();
    }

    private void Update()
    {
        if (mode == Mode.None)
        {
            return;
        }

        time += Time.deltaTime;

        switch (mode)
        {
            case Mode.Fading:
                UpdateFade();
                break;
            case Mode.Flashing:
                UpdateFlash();
                break;
        }
    }

    private void UpdateFade()
    {
        if (time < fadeDuration)
        {
            float t = time / fadeDuration;
            image.color = Color.Lerp(startColor, endColor, t);
        }
        else
        {
            image.color = endColor;
            mode = Mode.None;
        }
    }

    private void UpdateFlash()
    {
        if (time > flashDuration)
        {
            float delta = time - flashDuration;

            if (delta < flashFadeOutDuration)
            {
                float t = delta / flashFadeOutDuration;
                image.color = Color.Lerp(flashColor, Color.clear, t);
            }
            else
            {
                image.color = Color.clear;
                mode = Mode.None;
            }
        }
    }
}
