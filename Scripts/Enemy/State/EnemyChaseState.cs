    
using UnityEngine;

public class EnemyChaseState : EnemyState
{
    public EnemyChaseState(EnemyStateManager stateManager) : base(stateManager)
    {
    }
    
    public override void EnterState(Enemy enemy)
    {
        enemy.AnimationController.Walk(true);
        enemy.IsKinematic = false;

        if (stateManager.UseAttackState)
        {
            stateManager.ChaseTime = 0f;
        }
    }
    
    public override void ExitState(Enemy enemy)
    {
        enemy.AnimationController.StopWalking();
        enemy.IsKinematic = true;
    }
    
    public override void UpdateState(Enemy enemy)
    {
        enemy.ChaseTarget();

        if (!stateManager.UseAttackState)
        {
            return;
        }

        stateManager.ChaseTime += Time.fixedDeltaTime;

        if (enemy.IsTargetOnMyHead())
        {
            stateManager.SwitchState(stateManager.ShakeOffState);
        }

        if (enemy.IsTargetWithInAttackRange())
        {
            stateManager.DashStateCount = 0;
            stateManager.SwitchState(stateManager.AttackState);
        }

        if (enemy.IsTargetFarAway())
        {
            stateManager.DashStateCount += 1;
            stateManager.SwitchState(stateManager.DashState);
        }
    }
    
    public override void OnKnockBackTransitionCalled()
    
    {
        if (stateManager.UseKnockBackState)
        {
            stateManager.SwitchState(stateManager.KnockBackState);
        }
    }
    
    public override void OnSleepTransitionCalled()
    {
        if (stateManager.UseSleepState)
        {
            stateManager.SwitchState(stateManager.SleepState);
        }
    }

    public override void OnDanceTransitionCalled()
    {
        stateManager.SwitchState(stateManager.DanceState);
    }
}
