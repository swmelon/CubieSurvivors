using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


[RequireComponent(typeof(DamagablePlayer))]
[RequireComponent(typeof(PartnerInput))]
public class Partner : Player, IItemizable
{
    private PartnerInput input;
    private Player master;

    private void Awake()
    {
        input = GetComponent<PartnerInput>();
    }

    protected override void Start()
    {
        damagable = GetComponent<DamagablePlayer>();
        damagable.OnDead.AddListener(() => OnDead());
        controller = GetComponent<CustomThirdPersonController>();
    }
    
    public void SetMaster(Player player)
    {
        input.SetMaster(player.transform);
        enemyManager = player.EnemyManager;
        enemyManager.AddPlayer(transform);
        master = player;
    }

    public void ResetMaster(Player player)
    {
        input.SetMaster(player.transform);
        master = player;
    }
    
    public void MasterDie()
    {
        if (partner != null)
        {
            partner.MasterDie();
        }
        enemyManager.RemovePlayer(transform);
        Destroy(gameObject);
    }

    protected override void OnDead()
    {
        master.PartnerDie(partner);
        
        if (partner != null)
        {
            partner.ResetMaster(master);
        }
        
        enemyManager.RemovePlayer(transform);
        Destroy(gameObject);
    }
    
    public void BeItem()
    {
        gameObject.SetActive(false);
    }
}
