using UnityEngine;


public interface ITargetHaves
{
    public bool TryGetTarget(out Transform target);
    public bool TryGetTarget(out Transform target, out float distance);
    
    public int GetTargets(int maxTargets, int maxRange, out Transform[] targets);
    public Transform GetTransform();
}
