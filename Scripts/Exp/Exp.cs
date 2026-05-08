using System;
using UnityEngine;

public class Exp : Poolable<Exp>
{
    [SerializeField]
    private int exp = 500;

    [SerializeField]
    private SFXTags SFXTags;

    [SerializeField]
    private EventChannelSO startStageMoveEC;

    [SerializeField]
    private FloorGeoDataChannel geoDataChannel;

    private void OnTriggerEnter(Collider other)
    {
        if (!Released && other.TryGetComponent(out ExpManager expManager))
        {
            expManager.GetExp(exp);

            FMODAudioManager.instance.PlayOneShot(SFXTags);
            Release();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        startStageMoveEC.Subscribe(OnStageMove);
    }

    private void OnDisable()
    {
        startStageMoveEC.Unsubscribe(OnStageMove);
    }

    protected virtual void OnStageMove()
    {
        transform.position = new Vector3(transform.position.x, 
                       geoDataChannel.GetHeightOf(transform.position) + 0.5f, transform.position.z);
    }
}
