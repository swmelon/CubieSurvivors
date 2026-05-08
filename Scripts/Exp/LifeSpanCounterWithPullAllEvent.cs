using UnityEngine;
using System.Collections.Generic;

public class LifeSpanCounterWithPullAllEvent : PoolableLifeSpanCounterWithEvent
{
    [SerializeField]
    private EventChannelSO pullAllEC;

    [SerializeField]
    private IntChannelSO countChannel;

    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    [SerializeField]
    private float pullSpeed = 10f;

    private List<IPoolable> pullingExps = new List<IPoolable>();
    private List<int> indexToRemove = new List<int>();
    private Transform player;
    private bool pulling;

    protected override void Awake()
    {
        pullAllEC.Subscribe(PullAllExp);
        base.Awake();
    }

    protected override void OnDestroy()
    {
        pullAllEC.Unsubscribe(PullAllExp);
        base.OnDestroy();
    }

    private void PullAllExp()
    {
        playerTransformChannel.TryGetVariable(out player);
        pullingExps.Clear();

        foreach (var kvp in poolables)
        {
            if (!kvp.Key.Released)
            {
                pullingExps.Add(kvp.Key);
            }
        }

        pulling = true;
    }

    protected override void Update()
    {
        base.Update();

        countChannel.Register(Count);

        if (!pulling)
        {
            return;
        }

        if (pullingExps.Count == 0 || player == null)
        {
            pulling = false;
            return;
        }

        indexToRemove.Clear();
        float speed = Time.deltaTime * pullSpeed;

        for (int i = 0; i < pullingExps.Count; i++)
        {
            IPoolable exp = pullingExps[i];

            if (exp.Released || exp.gameObject == null)
            {
                indexToRemove.Add(i);
                continue;
            }

            Transform expTransform = exp.transform;
            expTransform.position = Vector3.MoveTowards(expTransform.position, player.position, speed);
        }

        int count = 0;

        for (int i = 0; i < indexToRemove.Count; i++)
        {
            pullingExps.RemoveAt(indexToRemove[i] - count);
            count += 1;
        }
    }
}