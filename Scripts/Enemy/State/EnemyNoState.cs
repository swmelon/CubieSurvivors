
public class EnemyNoState : EnemyState
{
    public EnemyNoState(EnemyStateManager stateManager) : base(stateManager)
    {
    }
    
    public override void EnterState(Enemy enemy) {}
    
    public override void ExitState(Enemy enemy) {}
    
    public override void UpdateState(Enemy enemy) 
    {
        enemy.ManuallyAdjustHeightWhenFalling();
    }

    public override void OnSleepTransitionCalled() {}
    
    public override void OnKnockBackTransitionCalled() {}

    public override void OnDanceTransitionCalled() { }
}
