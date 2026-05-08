using System;
using System.Collections;
using Local.Scripts.Extensions;
using Minimalist.Quantity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Damagable : MonoBehaviour
{
    [SerializeField]
    private AudioClip hitSound;
    
    [SerializeField]
    protected int maxHealth = 100;
    
    [SerializeField]
    private int health;
    
    [SerializeField]
    private bool debugMode = false;
    
    [SerializeField]
    private DamageTextSpawner damageTextSpawner, criticalDamageTextSpawner;
    
    [SerializeReference]
    protected bool dead = false;
    
    [SerializeReference]
    protected bool invincible = false;
    
    private bool started;
    private Coroutine invincibleCoroutine;

    public virtual int Health
    {
        get => health; 
        set
        {
            health = value;
            float healthRatio = (float)Health / MaxHealth;

            OnHealthChange?.Invoke(healthRatio);
        }
    }

    public virtual int MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            if (Health > MaxHealth)
            {
                Health = MaxHealth;
            }
        }
    }

    public bool IsDead => dead;


    public UnityEvent OnDead;
    public UnityEvent<float> OnHealthChange;
    public UnityEvent<Vector3> OnHit;
    public UnityEvent OnHeal; 
    
    private DamageTextPositioner textPositioner = new DamageTextPositioner();


    protected virtual void OnDisable()
    {
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
            invincible = false;
        }
    }

    protected virtual void Start()
    {
        Revive();
        textPositioner.BaseOffset = transform.localScale.x;
        started = true;
    }
    
    public virtual void Hit(int damage, Vector3 hitForce = default, bool isCritical = false,
        bool ignoreInvincible = false, Transform hitman = null)
    {
        if (dead)
        {
            return; 
        }

        if (damage < 0)
        {
            Debug.LogError("Damage cannot be negative.");
            return;
        } 
        
        // When you want to debug specific enemy,
        // you can set debugMode to true and make a checkpoint in the if statement below.
        if (debugMode)
        {
            
        }
        
        if (!Hitable())
        {
            return;
        }

        damage = RandomExtenstion.RandomizeDamage(damage);
        SpawnDamageText(damage, isCritical);
        
        OnHit?.Invoke(hitForce);

        if (invincible && !ignoreInvincible)
        {
            return;
        }

        CheckIfDead(damage, ignoreInvincible);
    }

    protected bool Hitable()
    {
        //  Not damaged until Start() is called.
        return !dead && started;
    }

    protected void CheckIfDead(int damage, bool ignoreInvincible)
    {
        if (Health - damage <= 0)
        {
            if (ignoreInvincible)
            {
                Health = 1;
                return;
            }

            Health = 0;
            dead = true;
            OnDead?.Invoke();
        }
        else
        {
            Health -= damage;
        }
    }

    protected void SpawnDamageText(int damage, bool isCritical)
    {
        DamageText damageText = isCritical ? criticalDamageTextSpawner.Spawn() : damageTextSpawner.Spawn();

        Vector3 spawnPosition = textPositioner.GetNextPosition(transform.position, transform.localScale);
        damageText.transform.position = spawnPosition;
        damageText.SetText(damage);
    }

    public virtual void HitRate(float rate, bool ignoreInvincible = false)
    {
        Hit((int)(MaxHealth * rate), ignoreInvincible: ignoreInvincible);
    }

    public virtual void Heal(int healthBoost)
    {
        Health = Mathf.Clamp(Health + healthBoost, 0, MaxHealth);
        OnHeal?.Invoke();
    }

    public void HealRate(float rate)
    {
        Heal((int)(MaxHealth * rate));
    }
    
    public virtual void Revive()
    {
        dead = false;
        invincible = false;
        Health = MaxHealth;
    }
    
    public void Invincible(float time)
    {
        invincible = true;

        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
        }

        invincibleCoroutine = StartCoroutine(OffInvincibleModeAfter(new WaitForSeconds(time)));
    }

    public void Invincible(WaitForSeconds delay)
    {
        invincible = true;

        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
        }

        invincibleCoroutine = StartCoroutine(OffInvincibleModeAfter(delay));
    }
    
    public void Invincible()
    {
        invincible = true;

        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
        }
    }

    public void OffInvincible()
    {
        invincible = false;
    }

    private IEnumerator OffInvincibleModeAfter(WaitForSeconds delay)
    {
        yield return delay;
        invincible = false;
    }

    public void Kill()
    {
        Health = 0;
        dead = true;
        OnDead?.Invoke();
    }

    // forcekill ��, ���� �����ӿ� �������� ����Ǿ� EnemyManager���� Remove�� �� �� �����ϴ� ���� ���� ����.
    public void ForeceSetDead()
    {
        dead = true;
    }
}
