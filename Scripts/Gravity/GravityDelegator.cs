using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class GravityDelegator : MonoBehaviour
{
    [SerializeField] 
    private float gravity;
    
    [SerializeField]
    private float verticalVelocity;

    [SerializeField] 
    private float terminalVelocity = 20f;
    
    private List<GravityDelegator> syncedDelegators = new List<GravityDelegator>();
    
    [SerializeReference]
    private bool activated = true;

    public float Gravity
    {
        get => gravity;
        set
        {
            foreach (var delegator in syncedDelegators)
            {
                if (delegator != null)
                {
                    delegator.Gravity = value;
                }
            }
            gravity = value;
        } 
    }

    public float VerticalVelocity
    {
        get => verticalVelocity;
        set
        {
            foreach (var delegator in syncedDelegators)
            {
                if (delegator != null)
                {
                    delegator.verticalVelocity = value;
                }
            }
            verticalVelocity = value;
        }
    }
    
    public float TerminalVelocity
    {
        get => terminalVelocity;
        set
        {
            foreach (var delegator in syncedDelegators)
            {
                if (delegator != null)
                {
                    delegator.terminalVelocity = value;
                }
            }
            terminalVelocity = value;
        }
    }
    
    public bool Activated
    {
        get => activated;
        set
        {
            foreach (var delegator in syncedDelegators)
            {
                if (delegator != null)
                {
                    delegator.activated = value;
                }
            }
            activated = value;
        }
    }

    public void SyncState(GravityDelegator delegator)
    {
        syncedDelegators.Add(delegator);
        delegator.Gravity = gravity;
        delegator.VerticalVelocity = verticalVelocity;
    }
}
