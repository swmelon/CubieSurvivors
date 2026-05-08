using System;
using System.Collections.Generic;


public interface IConditionalBehaviourPattern : IBehaviourPattern
{
    public bool IsAvailable();
    public bool IsTargetInRange(float distance);
}
