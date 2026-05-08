using System;
using System.Collections.Generic;


public interface IAttackPattern : IConditionalBehaviourPattern
{
    public void StartAction(Action onAttackFinished, List<IWeapon> weapons);
    public bool IsAvailable(float healthRatio);
    public float GetAttackDistance();
}
