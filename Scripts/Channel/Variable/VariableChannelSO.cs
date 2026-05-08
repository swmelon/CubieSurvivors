using System;
using System.Diagnostics.Tracing;
using UnityEngine;


public class VariableChannelSO<T> : ScriptableObject where T : class
{
    private T variable;
    protected Action<T> listeners;
    protected Action<T> oneTimeActionOnRegister;
    private bool isRegistered;
    
    public virtual void Register(T variable)
    {
        this.variable = variable;
        isRegistered = true;
        listeners?.Invoke(variable);
        oneTimeActionOnRegister?.Invoke(variable);
        oneTimeActionOnRegister = null;
    }
    
    public void Unregister(T variable)
    {
        if (this.variable == variable)
        {
            this.variable = default; 
        }
        else
        {
            Debug.LogError("Deregistering wrong variable.");
        }
        
        isRegistered = false;
        listeners?.Invoke(null);
    }
    
    public virtual void Subscribe(Action<T> listener)
    {
        listeners -= listener;
        listeners += listener;
        
        if (isRegistered)
        {
            listener.Invoke(variable);    
        }
    }
    
    public virtual void Unsubscribe(Action<T> listener)
    {
        listeners -= listener;
    }

    public bool TryGetVariable(out T variable)
    {
        variable = this.variable;
        return isRegistered;
    }

}
