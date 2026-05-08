
using System;

public interface ISpawner
{
    public bool TryGetLifeSpan(out float lifeSpan);
    
    public bool TryGetReleaseEventListener(out Action<IPoolable> listener);
    public void SubscribeSpawnEvent(Action<IPoolable> action);
    public void UnsubscribeSpawnEvent(Action<IPoolable> action);
}
