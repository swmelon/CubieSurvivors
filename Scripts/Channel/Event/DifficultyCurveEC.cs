using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DifficultyCurveUpdateEventChannel", menuName = "ScriptableObjects/Channels/DifficultyCurveUpdateEventChannel", order = SOAssetMenuIndex.Channel)]
public class DifficultyCurveEC : ScriptableObject
{
    private List<Action<DifficultyCurveManagerSO>> listeners = new List<Action<DifficultyCurveManagerSO>>();
    
    private List<Action<DifficultyCurveManagerSO>> lastListeners = new List<Action<DifficultyCurveManagerSO>>();
        
    public void Subscribe(Action<DifficultyCurveManagerSO> listener, bool last = false)
    {
        if (last)
        {
            lastListeners.Add(listener);
            return;
        }
        listeners.Add(listener);
    }
    
    public void Unsubscribe(Action<DifficultyCurveManagerSO> listener)
    {
        listeners.Remove(listener);
        lastListeners.Remove(listener);        
    }
    
    public void Raise(DifficultyCurveManagerSO value)
    {
        foreach (var listener in listeners)
        {
            listener.Invoke(value);
        }
        
        foreach (var listener in lastListeners)
        {
            listener.Invoke(value);
        }
    }
}