using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GameAmbientManager : MonoBehaviour
{
    [SerializeField]
    private CameraController cameraController;

    [SerializeField]
    private LightController lightController;

    [SerializeField]
    private GlobalFogController fogController;

    [SerializeField]
    private AmbientDataChannelSO ambientDataChannel;

    [SerializeField]
    private EventChannelSO startStageMoveChannel;

    [SerializeField ]
    private CameraController mainCameraController;

    [SerializeField]
    private GameScenarioManagerSO gameScenarioManager;

    private AmbientData ambientData;
    private int count;

    private void OnEnable()
    {
        ambientDataChannel.Subscribe(SetAmbientData);
        startStageMoveChannel.Subscribe(ChangeAmbientGradually);
    }
    
    private void OnDisable()
    {
        ambientDataChannel.Unsubscribe(SetAmbientData);
        startStageMoveChannel.Unsubscribe(ChangeAmbientGradually);
    }

    public void SetAmbientData(AmbientData ambientData)
    {
        count = 0;

        if (ReferenceEquals(ambientData, null))
        {
            return;
        }

        this.ambientData = ambientData;

        if (ambientData.useBackground)
        {
            // set camera background color
            if (ambientData.staticFog)
            {
                cameraController.SetBackgroundColor(ambientData.startFogColor);

            }
            else
            {
                cameraController.SetBackgroundColor(ambientData.endFogColor);
            }
        }
        else
        {
            fogController.SetFogColor(ambientData.startFogColor, 5f);
            cameraController.SetBackgroundColor(Color.black);
        }

        lightController.SetLightColorAndIntensity(ambientData.startLightColor, ambientData.startLightIntensity);
    }

    public void ChangeAmbientGradually()
    {
        if (ReferenceEquals(ambientData, null))
        {
            return;
        }

        count++;
        int bossInterval = gameScenarioManager.GetCurrentScenario().bossInterval;

        if (!ambientData.staticFog)
        { 
            Color fogColor = Color.Lerp(ambientData.startFogColor, ambientData.endFogColor, (float) count/ bossInterval);
            fogController.SetFogColor(fogColor, 5f);
        }

        if (!ambientData.staticLight)
        {
            Color lightColor = Color.Lerp(ambientData.startLightColor, ambientData.endLightColor, (float) count/ bossInterval);
            float lightIntensity = Mathf.Lerp(ambientData.startLightIntensity, ambientData.endLightIntensity, (float) count/ bossInterval);
            lightController.SetLightColorAndIntensity(lightColor, lightIntensity);
        }


      
    }
}