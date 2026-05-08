
using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class EventCameraController : MonoBehaviour
{
    [SerializeField]
    private MainCameraChannelSO mainCameraChannel;
    
    private Camera mainCamera;
    private Camera eventCamera;
    
    UniversalAdditionalCameraData baseCamData;
    UniversalAdditionalCameraData eventCamData;
    
    private void Awake()
    {
        eventCamera = GetComponent<Camera>();
        eventCamera.enabled = false;
        eventCamData = eventCamera.GetUniversalAdditionalCameraData();
        enabled = false;
    }

    private void OnEnable()
    {
        mainCameraChannel.Subscribe(SetBaseCamera);
    }
    
    private void OnDisable()
    {
        mainCameraChannel.Unsubscribe(SetBaseCamera);
    }

    private void SetBaseCamera(CameraController mainCameraController)
    {
        baseCamData = mainCameraController.Camera.
            GetUniversalAdditionalCameraData();
    }
    
    
    public void StartEvent()
    {
        print("Event Started");
        
        // stack on top of main camera
        
        enabled = true;
        eventCamera.enabled = true;
        eventCamData.renderType = CameraRenderType.Overlay;
        baseCamData.enabled = false;
        baseCamData.SetRenderer(1);
        baseCamData.enabled = true;
        baseCamData.cameraStack.Add(eventCamera);
    }
    
    public void EndEvent()
    {
        enabled = false;
        eventCamera.enabled = false;
        print("Event Ended");
        
        // remove from stack
        
        baseCamData.SetRenderer(0);
        baseCamData.cameraStack.Remove(eventCamera);
    }
}
 