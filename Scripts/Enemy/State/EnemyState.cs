
public abstract class EnemyState
{
    protected EnemyStateManager stateManager;
    
    protected EnemyState(EnemyStateManager stateManager)
    {
        this.stateManager = stateManager;
    }

    public abstract void EnterState(Enemy enemy);
    public abstract void ExitState(Enemy enemy);
    public abstract void UpdateState(Enemy enemy);
    public abstract void OnKnockBackTransitionCalled();
    public abstract void OnSleepTransitionCalled();
    public abstract void OnDanceTransitionCalled();
}
