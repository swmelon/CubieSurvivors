public class EnemyShakeOffState : EnemyState
{
    private EnemyState nextState;
    public EnemyShakeOffState(EnemyStateManager stateManager) : base(stateManager)
    {
    }

    public override void EnterState(Enemy enemy)
    {
        // shake off
        enemy.ShakeOff();
        nextState = stateManager.ChaseState;
    }

    public override void ExitState(Enemy enemy)
    {

    }

    public override void UpdateState(Enemy enemy)
    {
        if (!enemy.IsBehaving)
        {
            stateManager.SwitchState(nextState);
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
            nextState = stateManager.SleepState;
        }
    }

    public override void OnDanceTransitionCalled()
    {
        nextState = stateManager.DanceState;
    }
}
