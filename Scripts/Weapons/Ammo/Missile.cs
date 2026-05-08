
using UnityEngine;

public class Missile : Bullet<Missile>
{
    [SerializeField]
    private GameObject nozzle;

    [SerializeField]
    private OnePureEffectSpawner fallPointMarkerSpawner;

    [SerializeField]
    private FloorGeoDataChannel floorGeoDataChannel;

    private bool isActive = false;
    private PureEffect mark;

    protected override void OnEnable()
    {
        base.OnEnable();
        SetActive(false);
    }

    protected override void Update()
    {
        if (isActive) base.Update();
    }
    protected override void FixedUpdate()
    {
        if (isActive) base.FixedUpdate();
    }
    public void SetActive(bool val)
    {
        isActive = val;
        nozzle.SetActive(val);
    }

    private void OnTransformParentChanged()
    {
        print(transform.lossyScale);
    }

    public void SetPosAndMarkFallPoint(Vector3 position)
    {
        transform.position = position;
        position.y = floorGeoDataChannel.GetHeightOf(position);

        mark = fallPointMarkerSpawner.Spawn();
        mark.transform.SetPositionAndRotation(position, Quaternion.identity);
    }

    public override void Release()
    {
        if (!ReferenceEquals(mark, null))
        {
            mark.Release();
            mark = null;
        }

        base.Release();
    }
}
