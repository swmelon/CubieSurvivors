using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityDelegatee : MonoBehaviour
{
    protected bool activated = false;
    protected float distance;
    protected float gravity;
    protected float verticalVelocity;
    protected float terminalVelocity;
    protected Vector3 startPosition;
    protected GravityDelegator delegator;

    public virtual void Delegate(GravityDelegator delegator, float moveDistance)
    {
        if (activated)
        {
            Debug.LogError("Delegatee can't delegate when enabled.");
            return;
        }

        gravity = -delegator.Gravity;
        verticalVelocity = -delegator.VerticalVelocity;
        terminalVelocity = delegator.TerminalVelocity;
        startPosition = transform.position;
        distance = moveDistance;
        activated = true;

        delegator.Gravity = 0f;
        delegator.VerticalVelocity = 0f;
        delegator.Activated = false;

        this.delegator = delegator;
    }

    public virtual void Release()
    {
        delegator.Gravity = -gravity;
        delegator.VerticalVelocity = -verticalVelocity;
        delegator.Activated = true;
        Reset();
    }

    public void Release(float delay)
    {
        activated = false;
        Invoke(nameof(Release), delay);
    }
    
    private void Reset()
    {
        activated = false;
        gravity = 0f;
        verticalVelocity = 0f;
    }
}
