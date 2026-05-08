using Minimalist.Bar;
using Minimalist.Quantity;
using UnityEngine;

public class CharacterQuantityManager : MonoBehaviour
{
    [SerializeField]
    private QuantityBhv health, exp, abilityStack, rageStack;

    [SerializeField]
    private QuantityBhvChannel healthChannel, expChannel, abilityStackChannel, rageStackChannel;

    [SerializeField]
    private BarBhv healthBar, expBar, abilityStackBar, rageStackBar;

    private void OnEnable()
    {
        healthChannel.HasListener += EnableHealthBar;
        expChannel.HasListener += EnableExpBar;
        abilityStackChannel.HasListener += EnableAbilityBar;
        rageStackChannel.HasListener += EnableRageBar;

        healthChannel.NoListener += DisableHealthBar;
        expChannel.NoListener += DisableExpBar;
        abilityStackChannel.NoListener += DisableAbilityBar;
        rageStackChannel.NoListener += DisableRageBar;

        healthChannel.Register(health);
        expChannel.Register(exp);
        abilityStackChannel.Register(abilityStack);
        rageStackChannel.Register(rageStack);
    }

    private void OnDisable()
    {
        healthChannel.HasListener -= EnableHealthBar;
        expChannel.HasListener -= EnableExpBar;
        abilityStackChannel.HasListener -= EnableAbilityBar;
        rageStackChannel.HasListener -= EnableRageBar;

        healthChannel.NoListener -= DisableHealthBar;
        expChannel.NoListener -= DisableExpBar;
        abilityStackChannel.NoListener -= DisableAbilityBar;
        rageStackChannel.NoListener -= DisableRageBar;

        healthChannel.Unregister(health);
        expChannel.Unregister(exp);
        abilityStackChannel.Unregister(abilityStack);
        rageStackChannel.Unregister(rageStack);
    }

    private void EnableHealthBar()
    {
        healthBar.gameObject.SetActive(true);
    }

    private void EnableExpBar()
    {
        expBar.gameObject.SetActive(true);
    }

    private void EnableAbilityBar()
    {
        abilityStackBar.gameObject.SetActive(true);
    }

    private void EnableRageBar()
    {
        rageStackBar.gameObject.SetActive(true);
    }

    private void DisableHealthBar()
    {
        healthBar.gameObject.SetActive(false);
    }

    private void DisableExpBar()
    {
        expBar.gameObject.SetActive(false);
    }

    private void DisableAbilityBar()
    {
        abilityStackBar.gameObject.SetActive(false);
    }

    private void DisableRageBar()
    {
        rageStackBar.gameObject.SetActive(false);
    }
}
