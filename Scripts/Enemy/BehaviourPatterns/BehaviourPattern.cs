using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class BehaviourPattern : MonoBehaviour, IBehaviourPattern
{
    public bool IsActivated () => activated;

    public void SetAnimationController(EnemyAnimationController controller) => enemyAnimationController = controller;
    public void SetUser (Enemy enemy) => user = enemy;

    [SerializeField]
    [Range(0.25f, 4f)]
    protected float speedOfMotion = 1f;

    [SerializeReference]
    protected bool activated;

    protected EnemyAnimationController enemyAnimationController;
    protected WaitForSeconds unitDelay;
    protected WaitForSeconds halfUnitDelay;
    protected WaitForSeconds tickDelay;
    protected Enemy user;
    private const float unitDelayPeriod = 1f;
    private const float halfUnitDelayFactor = 2f;

    protected float unitDelayTime;
    private const float tickDelayTime = 0.3f;
    protected IEnumerator behaviourOnRunning;

    private event Action reservedOnFinishedBehaviour;



    private void Awake()
    {
        unitDelayTime = unitDelayPeriod / speedOfMotion;
        unitDelay = new WaitForSeconds(unitDelayTime);
        halfUnitDelay = new WaitForSeconds(unitDelayTime / halfUnitDelayFactor);
        tickDelay = new WaitForSeconds(tickDelayTime);
    }

    public virtual void StartAction(Action onActionFinished)
    {
        activated = true;
        reservedOnFinishedBehaviour = onActionFinished;
        behaviourOnRunning = StartBehaviourRoutine();
        StartCoroutine(behaviourOnRunning);
    }

    public virtual void StopAction()
    {
        if (!activated)
        {
            return;
        }

        activated = false;

        if (!ReferenceEquals(behaviourOnRunning, null))
        {
            StopCoroutine(behaviourOnRunning);
            OnFinishedBehaviour();
        }
    }

    protected abstract IEnumerator StartBehaviourRoutine();

    protected virtual void OnFinishedBehaviour()
    {
        activated = false;
        reservedOnFinishedBehaviour.Invoke();
        reservedOnFinishedBehaviour = null;
    }

    public void SetMotionSpeed(float speed)
    {
        speedOfMotion = speed;
        unitDelay = new WaitForSeconds(unitDelayPeriod / speedOfMotion);
    }
}
