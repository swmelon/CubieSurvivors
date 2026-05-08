using Minimalist.Quantity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class DamagablePlayer : DamagableAlly
{
    public event Action FinishVampireMode;

    [SerializeField]
    private FloatChannelSO playerHealthChannel;

    [SerializeField]
    private QuantityBhvChannel healthQuantityChannel;

    [SerializeField]
    private GameObject healingEffect;

    [SerializeField]
    private float selfHealTimeout = 1f;

    [SerializeField]
    private EventChannelSO playerHitEventChannel;

    [SerializeField]
    private PermanentUpgradableStat permanentUpgradableStat;

    public AnimationCurve vignetteEffectCurve;

    [SerializeField]
    private FloatEventChannelSO vignetteValueChangedChannel;

    [SerializeField]
    private EventChannelSO enterEventStageChannel;

    [SerializeField]
    private EventChannelSO defeatFinalBossEC;

    [SerializeField]
    private EventChannelSO takeHealthPackEC;

    [SerializeField]
    private DamageCalculatorSO damageCalculator;

    [SerializeField]
    private SFXTags heartbeatSFXTag;

    private QuantityBhv healthQuantityBhv;
    private const int selfHealAmount = 10;
    private const int vampireMaxInjuryCount = 4;
    private const float vampireInjuryRate = 0.05f;
    private const float drainFactorBase = 0.3f;
    private float selfInjureTimeout = 1f;
    private float selfHealTime, selfInjureTime;
    private bool vampireMode = false;
    private int selfInjureCount = 0;
    private int numberOfDrain = 0;
    private float drainFactor, minimumDrainFactor = 0.035f;

    private float healthPackHealRate = 0.5f;

    public override int Health
    {
        get => Mathf.RoundToInt(healthQuantityBhv.FillAmount * healthQuantityBhv.MaximumAmount);
        set
        {
            float healthRatio = (float)value / MaxHealth;
            healthQuantityBhv.FillAmount = healthRatio;
            OnHealthChange?.Invoke(healthRatio);
            playerHealthChannel.Register(healthRatio);
        }
    }

    public override int MaxHealth
    {
        get => Mathf.RoundToInt(healthQuantityBhv.MaximumAmount);
        set
        {
            healthQuantityBhv.MaximumAmount = value;
            maxHealth = value;

            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }
    }

    private void OnEnable()
    {
        CharacterUpgradableStat.MaxHealthChanged += SetMaxHealth;

        enterEventStageChannel.Subscribe(StopVampireMode);
        healthQuantityChannel.Subscribe(SetHealthQuantityBhv);
        defeatFinalBossEC.Subscribe(Invincible);
        takeHealthPackEC.Subscribe(OnTakeHealthPack);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        CharacterUpgradableStat.MaxHealthChanged -= SetMaxHealth;
        enterEventStageChannel.Unsubscribe(StopVampireMode);
        healthQuantityChannel.Unsubscribe(SetHealthQuantityBhv);
        defeatFinalBossEC.Unsubscribe(Invincible);
        takeHealthPackEC.Unsubscribe(OnTakeHealthPack);
    }

    protected override void Start()
    {
        // to apply additional health
        SetMaxHealth(MaxHealth);
        Health = MaxHealth;
        selfInjureTime = selfHealTimeout;

        base.Start();
        OnHit.AddListener((dir) => playerHitEventChannel.Raise());

    }

    private void SetMaxHealth(int newMaxHealth)
    {
        MaxHealth = newMaxHealth + permanentUpgradableStat.UAdditionalHealth.Value;
        Health = MaxHealth;
    }

    private void SetHealPeriod(float newHealPeriod)
    {
        selfHealTimeout = newHealPeriod;
    }
    
    public override void Hit(int damage, Vector3 hitForce = default,  bool isCritical = false, bool ignoreInvincible = false,
        Transform hitman = null)
    {
        FMODAudioManager.instance.PlayerHit();
        // apply defense
        damage = damageCalculator.TakeDamage(damage);
        base.Hit(damage, hitForce, isCritical, ignoreInvincible);
    }

    public override void Heal(int healthBoost)
    {
        healingEffect.SetActive(true);
        Invoke("OffHealingEffect", 1f);
        base.Heal(healthBoost);
    }

    private void Update()
    {
        if (dead)
        {
            return;
        }

        if (vampireMode)
        {
            UpdateVampireStatus();
            return;
        }
        selfHealTime += Time.deltaTime;

        if (selfHealTime > selfHealTimeout)
        {
            selfHealTime = 0;
            base.Heal(selfHealAmount);
        }
    }

    public void OffHealingEffect()
    {
        healingEffect.SetActive(false);
    }

    public void OnVampireMode()
    {
        FMODAudioManager.instance.PlayOneShot(heartbeatSFXTag, transform.position);
        if (vampireMode)
        {
            StopVampireMode();
        }

        damageCalculator.CallbackDamage(OnDamageVampireMode);
        vampireMode = true;
        numberOfDrain = 0;
    }

    private void OnDamageVampireMode(int damage)
    {
        // drain health

        // ���� : �ʹݿ��� ������ ������, �Ĺݿ��� �ʹ� ������.
        // ���� ���� ���δ� ȿ���� ����. (ü���� ���� ���ƾ��ҵ�)
        // ������ ���� ȸ������ ������.
        Heal((int)(damage * GetDrainFactor()));
    }

    private void UpdateVampireStatus()
    {
        selfInjureTime += Time.deltaTime;
        vignetteValueChangedChannel.Raise(vignetteEffectCurve.Evaluate(selfInjureTime / selfInjureTimeout));

        if (selfInjureTime > selfInjureTimeout)
        {
            if (selfInjureCount >= vampireMaxInjuryCount)
            {
                StopVampireMode();
                return;
            }
            
            selfInjureCount++;
            selfInjureTime = 0;
            base.Hit((int)(vampireInjuryRate * MaxHealth), ignoreInvincible: true);
            FMODAudioManager.instance.PlayOneShot(heartbeatSFXTag, transform.position);
        }
    }

    private void StopVampireMode()
    {
        vampireMode = false;
        selfInjureCount = 0;
        selfHealTime = 0;
        vignetteValueChangedChannel.Raise(0f);
        damageCalculator.RemoveCallback(OnDamageVampireMode);
        FinishVampireMode?.Invoke();
    }

    private void SetHealthQuantityBhv(QuantityBhv healthQuantityBhv)
    {
        this.healthQuantityBhv = healthQuantityBhv;
    }

    public void ForceToDieForDebug()
    {
        Health = 0;
        Hit(MaxHealth);
    }

    private float GetDrainFactor()
    {
        numberOfDrain++;
        return Math.Max(drainFactorBase * (float)(1 - Math.Log10(numberOfDrain)), minimumDrainFactor);
    }

    private void OnTakeHealthPack()
    {
        HealRate(healthPackHealRate);
    }
}


