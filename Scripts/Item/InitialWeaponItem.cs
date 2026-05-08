using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;


public class InitialWeaponItem : CoveredItem<Weapon>
{
    [SerializeField]
    private EventChannelSO getInitialWeaponEventChannel;
    
    [SerializeField] 
    private WeaponEventChannelSO returnWeaponChannel;

    [SerializeField]
    private EventChannelSO playerFallEC;

    private bool isActivated = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        isActivated = false;
        playerFallEC.Subscribe(ReturnWeaponOnPlayerFall);
    }

    public override void Activate(Player player)
    {
        isActivated = true;
        getInitialWeaponEventChannel.Raise();
        player.WeaponManager.Mount(content);
        content = null;
        base.Activate(player);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        playerFallEC.Unsubscribe(ReturnWeaponOnPlayerFall);

        if (isActivated)
        {
            return;
        }

        if (!ReferenceEquals(content, null))
        {
            returnWeaponChannel.Raise(content);
            content = null;
        }
    }
    private void ReturnWeaponOnPlayerFall()
    {
        content.transform.parent = null;
        returnWeaponChannel.Raise(content);
        content = null;
        Release();
    }

    protected override void OnStageMove()
    {
        // Do nothing
    }
}
