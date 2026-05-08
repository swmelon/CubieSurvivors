
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CoinCounter : NumberTrackingUI
{
    [SerializeField]
    private GemManagerSO coinManager;

    private void OnEnable()
    {
        SetNumber(coinManager.Coins);
        coinManager.NumCoinsChanged += SetNumber;
    }

    private void OnDisable()
    {
        coinManager.NumCoinsChanged -= SetNumber;

    }
}
