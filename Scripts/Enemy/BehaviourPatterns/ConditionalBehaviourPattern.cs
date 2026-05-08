using UnityEngine;

public abstract class ConditionalBehaviourPattern : BehaviourPattern, IConditionalBehaviourPattern
{
    public abstract bool IsAvailable();
    public virtual bool IsTargetInRange(float distance) => true;
}
