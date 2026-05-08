using UnityEngine;

public class AccessoryNotForSale : Accessory
{
    [SerializeField]
    [Range(-5, 5)]
    private int attack, defense, agility, luck;

    public int Attack => attack;
    public int Defense => defense;
    public int Agility => agility;
    public int Luck => luck;
}