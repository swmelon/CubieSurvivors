using System;
using System.Collections.Generic;


public interface IBehaviourPattern
{
    public void StartAction(Action onAttackFinished);
    public void SetAnimationController(EnemyAnimationController controller);
    public void SetUser(Enemy enemy);
    public void StopAction();
    public bool IsActivated();
}
