
using System.Runtime.CompilerServices;
using UnityEngine;
using System;

public class ObjectActiveController : MonoBehaviour
{
    public event Action Activate, Deactivate;
        
    [SerializeField]
    bool activeOnStart = false;

    [SerializeField]
    private BooleanEventChannelSO activeControlChannel;

    [SerializeField]
    private EventChannelSO[] onEventChannel;

    [SerializeField]
    private EventChannelSO[] offEventChannel;

    private bool hasCommand = false;

    private void Awake()
    {
        activeControlChannel.Subscribe(SetActive);

        foreach (var channel in onEventChannel)
        {
            channel.Subscribe(On);
        }

        foreach (var channel in offEventChannel)
        {
            channel.Subscribe(Off);
        }
    }

    private void Start()
    {
        // awake와 start사이에 SetActive가 호출됐는지 -> hasCommand가 true가 되었는지 확인
        if (!activeOnStart && !hasCommand)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        activeControlChannel.Unsubscribe(SetActive);

        foreach (var channel in onEventChannel)
        {
            channel.Unsubscribe(On);
        }

        foreach (var channel in offEventChannel)
        {
            channel.Unsubscribe(Off);
        }
    }
    
    public void SetActive(bool val)
    { 
        hasCommand = true;
        gameObject.SetActive(val);
        if (val)
        {
            Activate?.Invoke();
        }
        else
        {
            Deactivate?.Invoke();
        }
    }

    private void On()
    {
        SetActive(true);
    }

    private void Off()
    {
        SetActive(false);
    }
}
