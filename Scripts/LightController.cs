using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{
    [SerializeField]
    private Color darkColor;
    
    [SerializeField]
    private BooleanEventChannelSO turnOnOffLightEventChannel;

    [SerializeField]
    private FloatChannelSO lightIntensityChannel;

    [SerializeField]
    private AnimationCurve intensityAndAngleCurve;

    [SerializeField]
    private GamePauser gamePauser;

    [SerializeField]
    private float maxLightAngle = 50f;

    private Light directionalLight;
    private Color defaultColor; 

    private float timeCount, changeTime;
    private Color startColor, endColor;
    private float startIntensity, endIntensity;
    
    private void Awake()
    {
        directionalLight = GetComponent<Light>();
        defaultColor = directionalLight.color;
        turnOnOffLightEventChannel.Subscribe(SetLight);
        
        SetLightIntensity(1f);
    }

    private void TurnOffLight()
    {
        defaultColor = directionalLight.color;
        directionalLight.color = darkColor;
    }

    private void TurnOnLight()
    {
        directionalLight.color = defaultColor;
    }

    private void SetLight(bool val)
    {
        if (!val)
        {
            TurnOnLight();
        }
        else
        {
            TurnOffLight();
        }
    }

    public void SetLightColorAndIntensity(Color color, float intensity, float changeTime = 5f)
    {
        this.changeTime = changeTime;
        startColor = directionalLight.color;
        endColor = color;

        intensity = Mathf.Clamp(intensity, 0f, 1f);
        startIntensity = directionalLight.intensity;
        endIntensity = intensity;
        timeCount = 0;
    }

    public void SetLightIntensity(float intensity, float changeTime = 5f)
    {
        intensity = Mathf.Clamp(intensity, 0f, 1f);
        directionalLight.intensity = intensity;
        lightIntensityChannel.Register(intensity);
        transform.transform.rotation = Quaternion.Euler(maxLightAngle * intensityAndAngleCurve.Evaluate(intensity), transform.eulerAngles.y, 0f);
    }


    private void Update()
    {
        if (gamePauser.Pause)
        {
            return;
        }

        if (timeCount < changeTime)
        {
            timeCount += Time.deltaTime;
            directionalLight.color = Color.Lerp(startColor, endColor, timeCount / changeTime);
            directionalLight.intensity = Mathf.Lerp(startIntensity, endIntensity, timeCount / changeTime);
            lightIntensityChannel.Register(directionalLight.intensity);
            transform.transform.rotation = Quaternion.Euler(maxLightAngle * intensityAndAngleCurve.Evaluate(directionalLight.intensity), transform.eulerAngles.y, 0f);
        }
    }


    private void OnDestroy()
    {
        turnOnOffLightEventChannel.Unsubscribe(SetLight);
    }
}
