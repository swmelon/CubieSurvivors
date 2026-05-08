using System;
using UnityEngine;

public class TypeEventChannelSO<T> : ScriptableObject
{
    private Action<T> listeners;
    private Action<T> lastListeners;

    public void Subscribe(Action<T> listener)
    {
        listeners -= listener;
        listeners += listener;
    }

    public void SubscribeLast(Action<T> listener)
    {
        lastListeners += listener;
    }
    
    public void Unsubscribe(Action<T> listener)
    {
        listeners -= listener;
        lastListeners -= listener;
    }

    public void Raise(T value)
    {
        listeners?.Invoke(value);
        lastListeners?.Invoke(value);
    }    
}
