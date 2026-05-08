
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class AuraniumCounter : NumberTrackingUI
{
    [SerializeField]
    private GemManagerSO coinManager;

    
    private void OnEnable()
    {
        SetNumber(coinManager.AUs);
        coinManager.NumARNMsChanged += SetNumber;

    }

    private void OnDisable()
    {
        coinManager.NumARNMsChanged -= SetNumber;
    }
}
