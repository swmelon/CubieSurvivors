using System.Collections;
using UnityEngine;

public class CharacterVFXController : MonoBehaviour 
{
    public float liquidOffset = -0.5f;

    [SerializeField]
    GameObject waterInSplash, waterOutSplash, lavaSplash, poisoning;

    [SerializeField]
    GameObject steam, sugarSplash;

    private Coroutine stopSteamCoroutine;

    private void Awake()
    {
        waterInSplash.transform.parent = null;
        waterOutSplash.transform.parent = null;
        lavaSplash.transform.parent = null;
        sugarSplash.transform.parent = null;
    }

    private void OnEnable()
    {
        waterInSplash.SetActive(false);
        waterOutSplash.SetActive(false);
        lavaSplash.SetActive(false);
        poisoning.SetActive(false);
        steam.SetActive(false);
    }

    public void WaterInSplash()
    {
      ActivateEffectOnLiquidSurface(waterInSplash);
    }

    public void WaterOutSplash()
    {
        ActivateEffectOnLiquidSurface(waterOutSplash);
    }

    public void LavaSplash()
    {
        ActivateEffectOnLiquidSurface(lavaSplash);
    }

    public void Poisoning()
    {
        poisoning.SetActive(false);
        poisoning.SetActive(true);
    }

    public void FinishPoisoning()
    {
        poisoning.SetActive(false);
    }

    public void PlaySteam(float time = 1.5f)
    {
        steam.SetActive(false);
        steam.SetActive(true);
        
        if (stopSteamCoroutine != null)
        {
            StopCoroutine(stopSteamCoroutine);
        }

        stopSteamCoroutine = StartCoroutine(StopSteamAfter(time));
        ActivateEffectOnLiquidSurface(sugarSplash);
    }

    private IEnumerator StopSteamAfter(float time)
    {
        yield return new WaitForSeconds(time);
        steam.SetActive(false);
        stopSteamCoroutine = null;
    }

    public void StopSteam()
    {
        steam.SetActive(false);
    }

    private void ActivateEffectOnLiquidSurface(GameObject vfx)
    {
        vfx.SetActive(false);
        vfx.SetActive(true);
        Vector3 pos = transform.position;
        pos.y = liquidOffset;
        vfx.transform.position = pos;
    }
}
