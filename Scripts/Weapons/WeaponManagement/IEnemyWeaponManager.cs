
using System.Collections.Generic;

public interface IEnemyWeaponManager : IWeaponManager
{
    public bool TryGetAttackPatternByDistanceToTarget(float distanceToTarget, out IAttackPattern attackPattern);
    
    public List<IWeapon> GetWeaponsContainAttackPattern(IConditionalBehaviourPattern attackPattern);
}