using UnityEngine;
using System;
using System.Collections;
using StarterAssets;
using Local.Scripts.Extensions;

public class BuffController : MonoBehaviour
{
    private const float damageBuffMultiplier = 1.2f;
    private const float criticalBuffMultiplier = 1.2f;
    private const float buffTier1Threshold = 0.25f;
    private const float buffTier2Threshold = 0.5f;
    private const float buffTier3Threshold = 0.75f;
    private WeaponManager weaponManager;
    private DamageCalculatorSO damageCalculator;
    private Player player;
    private CustomThirdPersonController controller;
    private bool isDamageBuffed = false;
    private bool isCriticalBuffed = false;


    private void Awake()
    {
        if (TryGetComponent(out player))
        {
            damageCalculator = player.damageCalculator;
        }

        TryGetComponent(out controller);
        if (!TryGetComponent(out weaponManager))
        {
            Debug.LogError("WeaponManager is not found.");
        }
    }

    public void BuffROF(float time)
    {
        weaponManager.StartQuickFire(time);
    }

    public void BuffDamage(float time)
    {
        if (isDamageBuffed)
        {
            StopCoroutine(nameof(ResetDamageBonus));
        }

        damageCalculator.SetDamageBonus(damageBuffMultiplier);
       StartCoroutine(ResetDamageBonus(time));
    }

    private IEnumerator ResetDamageBonus(float time)
    {
        yield return new WaitForSeconds(time);
        damageCalculator.ResetDamageBonus();
    }

    public void BuffCriticalProb(float time)
    {
        if (isCriticalBuffed)
        {
            StopCoroutine(nameof(ResetCriticalProbBonus));
        }

        damageCalculator.SetCriticalProbBonus(criticalBuffMultiplier);
        StartCoroutine(ResetCriticalProbBonus(time));
    }

    private IEnumerator ResetCriticalProbBonus(float time)
    {
        yield return new WaitForSeconds(time);
        damageCalculator.ResetCriticalProbBonus();
    }

    public void BuffSpeed(float time)
    {
        controller.BoostMoveSpeed(time);
    }


    public void BuffRandom(float time)
    {
        // call random buff

        float prob = RandomExtenstion.GetRandomProbability();

        if (prob < buffTier1Threshold)
        {
            BuffDamage(time);
            Debug.Log("Buffed Damage");
        }
        else if (prob < buffTier2Threshold)
        {
            BuffCriticalProb(time);
            Debug.Log("Buffed Critical");
        }
        else if (prob < buffTier3Threshold)
        {
            BuffROF(time);
            Debug.Log("Buffed ROF");
        }
        else
        {
            BuffSpeed(time);
            Debug.Log("Buffed Speed");
        }

    }
}