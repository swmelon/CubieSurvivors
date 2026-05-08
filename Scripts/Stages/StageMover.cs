using System;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class StageMover : GravityDelegatee
{
    public UnityEvent OnFinishMove;
    
    [SerializeField]
    private EnemyManager enemyManager;

    [SerializeField]
    private EventChannelSO startStageMoveEvent;

    [SerializeField]
    private EventChannelSO finishStageMoveEvent;

    [SerializeField]
    private Transform globalFogTrasform;

    [SerializeField]
    private AnimationCurve fogMovementCurve;

    [SerializeField]
    private BooleanEventChannelSO freefallEC;

    [SerializeField]
    private BooleanChannelSO freefallChannel;

    private float adjustmentTime, timeElapsed;
    private bool adjustHeight;
    private Vector3 adjustmentPerSec;
    private CustomThirdPersonController controller;
    private float delgateCallTime;
    private bool firstCall = true;
    private float fogMaxHeight = 5f;
    private bool moveFog;

    private void Awake()
    {
        freefallEC.Raise(false);
        freefallChannel.Register(false);
    }

    /// <summary>
    /// no public call, Delegate(d, m, h) replace this
    /// </summary>
    /// <param name="delegator"></param>
    /// <param name="moveDistance"></param>
    public override void Delegate(GravityDelegator delegator, float moveDistance)
    {
        if (!firstCall && Time.time - delgateCallTime < 0.1f)
        {
            Debug.LogWarning("StageMover: Delegate called too fast.");
        }

        firstCall = false;
        delgateCallTime = Time.time;

        base.Delegate(delegator, moveDistance);

        adjustHeight = false;

        startStageMoveEvent.Raise();
        enemyManager.DisableGravity();
        freefallEC.Raise(true);
        freefallChannel.Register(true);
    }

    public void Delegate(GravityDelegator delegator, float moveDistance, float heightAdjustment, bool moveFog)
    {
        Delegate(delegator, moveDistance);

        controller = delegator as CustomThirdPersonController;


        if (!Mathf.Approximately(verticalVelocity, terminalVelocity))
        {
            adjustmentTime = CalculateTime(moveDistance, verticalVelocity);
        }
        else
        {
            adjustmentTime = Mathf.Abs((moveDistance - heightAdjustment) / terminalVelocity);
        }

        this.moveFog = moveFog;

        adjustHeight = true;
        timeElapsed = 0f;
        adjustmentPerSec = new Vector3(0.0f, heightAdjustment / adjustmentTime, 0.0f);
        enemyManager.DisableGravity();
    }

    public override void Release()
    {
        base.Release();
        
        enemyManager.EnableGravity();
        OnFinishMove.Invoke();
        finishStageMoveEvent.Raise();
        freefallEC.Raise(false);
        freefallChannel.Register(false);
    }

    private void Update()
    {
        if (!activated)
        {
            return;
        }
        
        verticalVelocity += gravity * Time.deltaTime;
        
        if (verticalVelocity > terminalVelocity)
        {
            verticalVelocity = terminalVelocity;
        }
        
        transform.position += new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime;

        if (!adjustHeight)
        {
            return;
        }

        if (timeElapsed <= adjustmentTime)
        {
            if (moveFog)
            {
                globalFogTrasform.localPosition = new Vector3(0.0f, fogMaxHeight * fogMovementCurve.Evaluate(timeElapsed / adjustmentTime), 0.0f);
            }

            timeElapsed += Time.deltaTime;
            transform.position += adjustmentPerSec * Time.deltaTime;
            controller.AddExtraForce(adjustmentPerSec, adjustmentPerSec.magnitude);

            if (timeElapsed > adjustmentTime)
            {
                globalFogTrasform.localPosition = Vector3.zero;
            }
        }

    }

    private void FixedUpdate()
    {
        if (!activated)
        {
            return;
        }
        
        if (Mathf.Abs((transform.position - startPosition).y) < distance)
        {
            return;
        }
        
        transform.position = startPosition + distance * Vector3.up;
        Release();
    }

    private float CalculateTime(float moveDistance, float verticalVelocity)
    {
        float timeToReachTerminalVelocity = (terminalVelocity - verticalVelocity) / gravity;
        float distanceToTerminalVelocity = verticalVelocity * timeToReachTerminalVelocity + 0.5f * gravity * Mathf.Pow(timeToReachTerminalVelocity, 2);

        if (distanceToTerminalVelocity < moveDistance)
        {
            float remainingDistance = moveDistance - distanceToTerminalVelocity;
            float timeAtTerminalVelocity = remainingDistance / terminalVelocity;
            return timeToReachTerminalVelocity + timeAtTerminalVelocity;
        }
        else
        {
            // Solving quadratic equation for time: s = ut + 0.5at^2
            float a = 0.5f * gravity;
            float b = verticalVelocity;
            float c = -moveDistance;

            // Calculating discriminant
            float discriminant = Mathf.Sqrt(b * b - 4f * a * c);

            // Calculating time (only one of the roots is physically meaningful - the positive one)
            float time = (-b + discriminant) / (2f * a);
            return time;
        }
    }
}
