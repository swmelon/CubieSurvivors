using System;
using UnityEngine;

public interface IUpgradable : IIconized
{
    public void Upgrade();
    public bool IsUpgradable();
    public bool IsExposed();
    public string Name { get; set; }
    public Sprite Icon { get; set; }

    public Weapon Weapon { get; set; }
    public string GetOptionText();
    public string GetPercentageText();
    public Sprite GetUpgradeSymbol();
    public string GetBonusText(out Color color);
    public int GetGrade();
    public void Reset();
    public void Buff();
    public void FinishBuff();

    public bool TryGetWeapon(out Weapon weapon);

    public void CallBackWhenUpgradeCompleted(Action<IUpgradable> action);

    
    /// <summary>
    /// This method will be call when UpgradeCard is set.
    /// </summary>
    public void GenerateRandomBonusRate();
}
