
using System;
using UnityEngine;

public class GlobalFogController : MonoBehaviour
{
    public static readonly string FOG_COLOR_ID = "Color_58E0201D";

    [SerializeField]
    private EventChannelSO[] onEvent, offEvent;

    [SerializeField]
    private BooleanEventChannelSO fogControlChannel;

    private Renderer fogRenderer;
    private Color startColor, endColor;
    private float changeTime, timeCount;

    private void Awake()
    {
        fogRenderer = GetComponent<Renderer>();
        fogControlChannel.Subscribe(SetFog);

        foreach (EventChannelSO on in onEvent)
        {
            on.Subscribe(On);
        }

        foreach (EventChannelSO off in offEvent)
        {
            off.Subscribe(Off);
        }
    }

    private void OnDestroy()
    {
        fogControlChannel.Unsubscribe(SetFog);

        foreach (EventChannelSO on in onEvent)
        {
            on.Unsubscribe(On);
        }

        foreach (EventChannelSO off in offEvent)
        {
            off.Unsubscribe(Off);
        }
    }

    private void Update()
    {
        if (timeCount < changeTime)
        {
            timeCount += Time.deltaTime;
            fogRenderer.material.SetColor(FOG_COLOR_ID, Color.Lerp(startColor, endColor, timeCount / changeTime));
        }
    }

    private void On()
    {
        SetFog(true);
    }

    private void Off()
    {
        SetFog(false);
    }

    private void SetFog(bool value)
    {
        gameObject.SetActive(value);
    }
    public void SetFogMaterial(Material material)
    {
        fogRenderer.material = material;
    }

    public void SetFogColor(Color color, float changeTime)
    {
        startColor = fogRenderer.material.GetColor(FOG_COLOR_ID);
        endColor = color;
        this.changeTime = changeTime;
        timeCount = 0;

    }
}
