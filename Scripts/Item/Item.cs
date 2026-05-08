using System;
using UnityEngine;


[RequireComponent(typeof(BoxCollider))]
public abstract class Item : Poolable<Item>
{
    public Action OnActivated;
    public bool UseProbability => useProbability;
    public virtual float Probability => probability;

    [Tooltip("If true, the item will only be spawned by probability, not manually.")]
    [SerializeField]
    private bool useProbability = false;

    [SerializeField]
    private FloorGeoDataChannel geoDataChannel;

    [SerializeField]
    private EventChannelSO startStageMoveEC;

    [SerializeField]
    private SFXTags SFXTags;

    [SerializeField][Range(0f, 1f)]
    protected float probability = 0f;

    [SerializeField]
    protected float spinSpeed = 50f;

    private float halfItemSize = 0.5f;

    protected override void OnEnable()
    {
        base.OnEnable();
        OnActivated = null;
        transform.rotation = Quaternion.identity;
        startStageMoveEC.Subscribe(OnStageMove);
    }

    protected virtual void OnDisable()
    {
        startStageMoveEC.Unsubscribe(OnStageMove);
    }

    public virtual void Activate(Player player)
    {
        Release();
        FMODAudioManager.instance.PlayOneShot(SFXTags);
        OnActivated?.Invoke();
    }
    
    protected virtual void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    protected virtual void OnStageMove()
    {
            transform.position = new Vector3(transform.position.x, 
            geoDataChannel.GetHeightOf(transform.position) + halfItemSize, transform.position.z);
    }
}
