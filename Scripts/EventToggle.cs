using UnityEngine;
using System.Collections.Generic;

public class EventToggle : MonoBehaviour
{
    [SerializeField]
    private List<EventChannelSO> onEventChannel, offEventChannel;

    [SerializeField]
    private bool onAtStart;

    private void Awake()
    {
        for (int i = 0; i < onEventChannel.Count; i++)
        {
            onEventChannel[i].Subscribe(SwitchOn);
        }

        for (int i = 0; i < offEventChannel.Count; i++)
        {
            offEventChannel[i].Subscribe(SwitchOff);
        }
    }

    private void Start()
    {
        if (!onAtStart)
        {
            SwitchOff();
        }
    }


    private void OnEnable()
    {
        for (int i = 0; i < onEventChannel.Count; i++)
        {
            onEventChannel[i].Unsubscribe(SwitchOn);
        }

        for (int i = 0; i < offEventChannel.Count; i++)
        {
            offEventChannel[i].Subscribe(SwitchOff);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < onEventChannel.Count; i++)
        {
            onEventChannel[i].Subscribe(SwitchOn);
        }

        for (int i = 0; i < offEventChannel.Count; i++)
        {
            offEventChannel[i].Unsubscribe(SwitchOff);
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < onEventChannel.Count; i++)
        {
            onEventChannel[i].Unsubscribe(SwitchOn);
        }

        for (int i = 0; i < offEventChannel.Count; i++)
        {
            offEventChannel[i].Unsubscribe(SwitchOff);
        }
    }

    private void SwitchOn()
    {
        gameObject.SetActive(true);
    }

    private void SwitchOff()
    {
        gameObject.SetActive(false);
    }
}
