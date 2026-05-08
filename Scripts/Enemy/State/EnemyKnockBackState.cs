using UnityEngine;

public class EnemyKnockBackState : EnemyState
{
    private const int knockBackFrameCountDefault = 2;

    private int knockBackFrameCount = 0;
    public EnemyKnockBackState(EnemyStateManager stateManager) : base(stateManager)
    {
    }
    
    public override void EnterState(Enemy enemy)
    {
        knockBackFrameCount = knockBackFrameCountDefault;
        enemy.IsKinematic = false;
    }
    
    public override void ExitState(Enemy enemy)
    {
        enemy.IsKinematic = true;
    }
    
    public override void UpdateState(Enemy enemy)
    {
        knockBackFrameCount -= 1;
        
        if (knockBackFrameCount <= 0)
        {
            stateManager.SwitchState(stateManager.ChaseState);
        }
        
        enemy.KnockBack();
    }
    
    public override void OnKnockBackTransitionCalled()
    {
        if (stateManager.UseKnockBackState)
        {
            knockBackFrameCount += 1;
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
