using System.Collections.Generic;
using UnityEngine;


public class GravityDelegatorSO : ScriptableObject
{
    [SerializeField] 
    private float gravity;
    
    [SerializeField]
    private float verticalVelocity;
    
    [SerializeField]
    private List<GravityDelegatorSO> syncedDelegators = new List<GravityDelegatorSO>();
    
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

    public void SyncState(GravityDelegatorSO delegator)
    {
        syncedDelegators.Add(delegator);
        delegator.Gravity = gravity;
        delegator.VerticalVelocity = verticalVelocity;
    }
}
