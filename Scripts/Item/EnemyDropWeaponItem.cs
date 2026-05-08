using UnityEngine;
using UnityEngine.Serialization;

public class EnemyDropWeaponItem : CoveredItem<Weapon>
{
    [SerializeField]
    private WeaponEventChannelSO playerGetBossDropWeaponEventChannel;

    [SerializeField]
    private WeaponEventChannelSO returnWeaponChannel;

    [SerializeField]
    private EventChannelSO playerFallEC;

    [SerializeField]
    private EventChannelSO enterEventStage;

    [SerializeField]
    private FloorGeoDataChannel floorHeightChannel;

    protected override void OnEnable()
    {
        base.OnEnable();
        playerFallEC.Subscribe(ReturnWeapon);
        enterEventStage.Subscribe(ReturnWeapon);
    }

    protected override void  OnDisable()
    {
        base.OnDisable();
        playerFallEC.Unsubscribe(ReturnWeapon);
        enterEventStage.Unsubscribe(ReturnWeapon);
    }

    public override void Activate(Player player)
    {
        playerGetBossDropWeaponEventChannel.Raise(content);
        content.transform.parent = null;
        content = null;
        playerFallEC.Unsubscribe(ReturnWeapon);
        base.Activate(player);
    }    

    private void ReturnWeapon()
    {
        content.transform.parent = null;
        returnWeaponChannel.Raise(content);
        content = null;
        Release();
    }
}
