

using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class NPCEventZoneTrigger : MonoBehaviour
{
    [SerializeField]
    private EventChannelSO finishNPCEventChannel;
    
    public UnityEvent OnEnterEventZone, OnExitEventZone;
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnEnterEventZone?.Invoke();
            
            // subscribe to finish event
            finishNPCEventChannel.Subscribe(OnFinishNPCEvent);
        }
    }
    
    private void OnFinishNPCEvent()
    {
        // unsubscribe when finish event is raised
        finishNPCEventChannel.Unsubscribe(OnFinishNPCEvent);
        OnExitEventZone?.Invoke();
    }
    
}
