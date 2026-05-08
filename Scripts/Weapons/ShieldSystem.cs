using System;
using System.Collections;
using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;

public class ShieldSystem : UserWeapon
{
    [SerializeField]
    private List<Explosive> explosives; 
    
    [SerializeField]
    private Shield shield;

    [SerializeField]
    private float _invincibleDuration = 1f;
    [SerializeField]
    private float _shieldRecoveryTime = 5f;
    private float time, invincibleTime;
    
    [SerializeReference]
    private int shieldLevel = 0;
    
    private struct UpgradableStat
    {
        public List<int> MaxShieldLevel, Color;
        public List<float>ExplosiveDamage;
        public List<bool> Explosion;
    }
    
    private Upgradable<int> UMaxShieldLevel;
    private UInt UExplosiveDamage;
    private List<Color> colors;
    private Damagable damagableUser;
    private GameObject dummyShield;

    protected override void Awake()
    {
        base.Awake();
        dummyShield = transform.GetChild(0).gameObject;
        
        onMounted = OnMounted;
    }

    private void OnMounted()
    {
        dummyShield.SetActive(false);
        shield.gameObject.SetActive(true);

        for (int i = 0; i < explosives.Count; i++)
        {
            explosives[i].SetDamage(UExplosiveDamage.Value);
            
            explosives[i].SetTargetLayer(LayerMaskCash.Enemy);
            explosives[i].SetWeapon(this);
        }
    }

    public override void OnUnmounted()
    {
    }

    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        InitializeUpgradables(upgradableStat);
    }

    public override void Damage()
    {
        throw new NotImplementedException();
    }

    private void InitializeUpgradables(UpgradableStat upgradableStat)
    {
        UExplosiveDamage = new UInt(upgradableStat.ExplosiveDamage, symbol: symbolContainer.ExplosionDamage, 
            optionText: CardText.EXPLOSION_DAMAGE, noBonus: true);
        UMaxShieldLevel = new Upgradable<int>(upgradableStat.MaxShieldLevel, symbol: symbolContainer.Plus,
            optionText: CardText.SHIELD_LEVEL_UP);
        colors = new List<Color>(upgradableStat.Color.ToColors());
    }

    private void Update()
    {
        time += Time.deltaTime;
        invincibleTime -= Time.deltaTime;
        
        if (time > _shieldRecoveryTime && shieldLevel < UMaxShieldLevel.Value)
        {
            shieldLevel += 1;
            time = 0;

            if (shieldLevel == 0)
            {
                damagableUser.Invincible();
                shield.gameObject.SetActive(true);
                return;
            }
            
            ChangeShieldColor();
        }
    }

    public override void SetWeaponUser(ITargetHaves weaponUser)
    {
        base.SetWeaponUser(weaponUser);
        
        if (!weaponUser.GetTransform().TryGetComponent(out Damagable damagable))
        {
            Debug.LogError("User must have Damagable component. " 
                           + transform.root.name + "Can't use ShieldSystem.");
        }
        
        damagableUser = damagable;
        damagableUser.OnHit.AddListener((direction) => OnPlayerHit());
        damagableUser.Invincible();
        invincibleTime = _invincibleDuration;
        shieldLevel = 0;
        shield.SetColor(colors[shieldLevel]);
    }

    private void OnPlayerHit()
     { 
         if (invincibleTime > 0)
         {
             return;
         }

         if (shieldLevel < 0)
         {
             time = 0;
             return;
         }

        Explosive explosive = explosives[shieldLevel];
        explosive.gameObject.SetActive(true);
        explosive.Explode(() => explosive.gameObject.SetActive(false));
         shieldLevel -= 1;
         time = 0;
         invincibleTime = _invincibleDuration;

         if (shieldLevel < 0)
         {
            shield.gameObject.SetActive(false);
            damagableUser.Invincible(invincibleTime);
            return; 
         }
         
         ChangeShieldColor();
    }
    
    private void ChangeShieldColor()
    {
        if (shieldLevel < colors.Count && shieldLevel >= 0)
        {
            shield.SetColor(colors[shieldLevel]);
        }
    }

    public override void BeItem()
    {
        dummyShield.SetActive(true);
        shield.gameObject.SetActive(false);
    }
}
