using UnityEngine;
using System.Collections.Generic;

public class EnemyBox : Drops
{
    [SerializeField]
    private EnemySpawner enemySpawner;
    
    [SerializeField]
    private OnePureEffectSpawner boxCrashEffectSpawner;

    [SerializeField]
    private List<EventChannelSO> releaseEventChannel;
    
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        transform.rotation = Quaternion.identity;

        for (int i = 0; i < releaseEventChannel.Count; i++)
        {
            releaseEventChannel[i].Subscribe(CrashWithoutEnemy);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < releaseEventChannel.Count; i++)
        {
            releaseEventChannel[i].Unsubscribe(CrashWithoutEnemy);
        }
    }

    public override void Drop(Vector3 position)
    {
        position.y = 15f;
        transform.position = position;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (Released)
        {
            return;
        }
        
        Transform crashEffectTransform = (boxCrashEffectSpawner.Spawn()).transform;

        if (enemySpawner.TrySpawnCurrentPowerEnemy(out Enemy enemy, transform.position))
        {
        }
   
        crashEffectTransform.SetPositionAndRotation(transform.position, transform.rotation);
        crashEffectTransform.localScale = transform.localScale;
        Release();
    }

    private void CrashWithoutEnemy()
    {
        if (Released)
        {
            return;
        }

        Transform crashEffectTransform = (boxCrashEffectSpawner.Spawn()).transform;
        crashEffectTransform.SetPositionAndRotation(transform.position, transform.rotation);
        crashEffectTransform.localScale = transform.localScale;
        Release();
    }
}
