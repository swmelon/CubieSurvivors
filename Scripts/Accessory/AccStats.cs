using System;
using UnityEngine;
using UnityEngine.AI;

public struct AccStats : IEquatable<AccStats>
{
    private const int statBaseOffset = 5;
    private const int maxTotalRank = 4;
    private const int minStatValue = 1;
    private const int maxStatValue = 9;

    public int Attack
    {
        get => attack - statBaseOffset;
        set => attack = value;
    }

    public int Defense
    {
        get => defense - statBaseOffset;
        set => defense = value;
    }

    public int Agility
    {
        get => agility - statBaseOffset;
        set => agility = value;
    }

    public int Luck
    {
        get => luck - statBaseOffset;
        set => luck = value;
    }

    private int attack;
    private int defense;
    private int agility;
    private int luck;

    public AccStats(int attack, int defense, int agility, int luck)
    {
        this.attack = attack;
        this.defense = defense;
        this.agility = agility;
        this.luck = luck;
    }

    public AccStats(AccessoryNotForSale accessory)
    {
        this.attack = statBaseOffset + accessory.Attack;
        this.defense = statBaseOffset + accessory.Defense;
        this.agility = statBaseOffset + accessory.Agility;
        this.luck = statBaseOffset + accessory.Luck;
    }

    public int GetStatID()
    {
        string idStr = attack.ToString()
                    + defense.ToString()
                    + agility.ToString()
                    + luck.ToString();
        return int.Parse(idStr);
    }

    public bool Equals(AccStats other)
    {
        return attack == other.attack &&
               defense == other.defense &&
               agility == other.agility &&
               luck == other.luck;
    }

    public int GetRank()
    {
        return Attack + Defense + Agility + Luck;
    }

    public bool ChangeStats(int deltaAtk, int deltaDef, int deltaAgi, int deltaLuk)
    {
        bool result = CheckStatBound(attack + deltaAtk) &&
                      CheckStatBound(defense + deltaDef) &&
                      CheckStatBound(agility + deltaAgi) &&
                      CheckStatBound(luck + deltaLuk);

        if (!result)
        {
            return false;
        }

        int totalDelta = deltaAtk + deltaDef + deltaAgi + deltaLuk;

        int newRank = GetRank() + totalDelta;

        if (newRank > maxTotalRank || newRank < 0)
        {
            return false;
        }

        attack += deltaAtk;
        defense += deltaDef;
        agility += deltaAgi;
        luck += deltaLuk;

        return true;
    }

    private bool CheckStatBound(int stat)
    {
        if (stat < minStatValue)
        {
            return false;
        }

        if (stat > maxStatValue)
        {
            return false;
        }

        return true;
    }
}