using UnityEngine;

public class EnemyAttackState : EnemyState
{
    private const float attackTimeout = 5f;
    private const float sleepTransitionDelay = 1f;

    private float time = 0f;
    public EnemyAttackState(EnemyStateManager stateManager) : base(stateManager)
    {
    }
    
    public override void EnterState(Enemy enemy)
    {
        enemy.Attack();
        time = 0f;
    }
    
    public override void ExitState(Enemy enemy)
    {
       
    }

    public override void UpdateState(Enemy enemy)
    {
        time += Time.fixedDeltaTime;

        if (!enemy.IsBehaving)
        {
            stateManager.SwitchState(stateManager.ChaseState, enemy.Data.AttackToChaseDelay);
            enemy.RotateSmoothly();
        }
        else if (enemy.KeepRotatingWhileAttacking)
        {
            enemy.RotateSlowly();
        }
        else if (enemy.KeepChasingWhileAttacking)
        {
            enemy.ChaseTargetWhileAttacking();
        }

        if (enemy.UseGravity == false)
        {
            enemy.ManuallyAdjustHeightWhenFalling();
        }
    }

    public override void OnKnockBackTransitionCalled()
    {
        // ignore
    }

    public override void OnSleepTransitionCalled()
    {
        if (stateManager.UseSleepState)
        {
            stateManager.SwitchState(stateManager.SleepState, sleepTransitionDelay);
        }
    }

    public override void OnDanceTransitionCalled()
    {
         stateManager.SwitchState(stateManager.DanceState);
    }
}
