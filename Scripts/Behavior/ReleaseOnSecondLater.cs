
using UnityEngine;

public class ReleaseOnSecondLater : Poolable<ReleaseOnSecondLater>
{
    private const float releaseDelay = 1f;

    private void Awake()
    {
        Invoke("Release", releaseDelay);
    }
}
