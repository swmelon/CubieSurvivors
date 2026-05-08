
using System;
using UnityEngine;

public class UpgradeUICameraController : MonoBehaviour
{
    [SerializeField] 
    private EventChannelSO startUpgradeChannel, finishUpgradeChannel;

    private Camera UICamera;
    
    private void Awake()
    {
        UICamera = GetComponent<Camera>();
        TurnOffUICamera();
    }

    private void OnEnable()
    {
        startUpgradeChannel.Subscribe(TurnOnUICamera);
        finishUpgradeChannel.Subscribe(TurnOffUICamera);
    }

    private void OnDisable()
    {
        startUpgradeChannel.Unsubscribe(TurnOnUICamera);
        finishUpgradeChannel.Unsubscribe(TurnOffUICamera);
    }

    private void TurnOnUICamera()
    {
        UICamera.enabled = true;
    }
    
    private void TurnOffUICamera()
    {
        UICamera.enabled = false;
    }
}
