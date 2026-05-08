using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum UpgradeCallbackType
{
    Activate,
    Upgrade,
    CompleteUpgrade
}

public class Upgradable<T> : IUpgradable 
{
    public Action Upgraded;
    public Action UpgradeCompleted;
    public Action UpgradeReset;

    
    protected readonly List<T> values = new List<T>();
    protected bool buff;
    
    private string name;
    private Sprite icon, symbol;
    private RenderTexture particleIcon;
    private int grade;
    private string optionText;
    private bool exposed, initialExposedValue;
    private Weapon weapon;
    
    public virtual T Value => buff ? values[Mathf.Min(grade + 1, values.Count - 1)] : values[grade]; 
    
    public int GetGrade() => grade;
    public string GetName() => name;
    public Sprite GetUpgradeSymbol() => symbol;
    public float GetCompletionPercentage() => (float)grade / (values.Count - 1);

    public string Name
    {
        get => name;
        set => name = value;
    }

    public Sprite Icon
    {
        get => icon;
        set => icon = value;
    }

    public Weapon Weapon
    {
        get => weapon;
        set => weapon = value;
    }

    protected Upgradable(bool exposed, Sprite icon, Sprite symbol, string name, string optionText)
    {
        this.exposed = exposed;
        this.initialExposedValue = exposed;
        this.name = name;
        this.icon = icon;
        this.symbol = symbol;
        this.optionText = optionText;
    }
    
    public Upgradable(List<T> values, string name = "", string optionText = "None", Sprite icon = null, Sprite symbol = null, bool exposed = true)
    {
        Type type = typeof(T);
        this.values = values;
        this.optionText = optionText;
        this.icon = icon;
        this.symbol = symbol;
        this.exposed = exposed;
        this.name = name;
        this.initialExposedValue = exposed;
        grade = 0;
    }

    public Sprite GetIcon()
    {
        return icon;
    }

    public virtual void Upgrade()
    {
        if (IsUpgradable())
        {
            grade++;
            Upgraded?.Invoke();
            
            if (IsUpgradable() == false)
            {
                UpgradeCompleted?.Invoke();
            }
        }
        else
        {
            Debug.Log("Upgrade() called when unable to upgrade.");
        }
    }
    
    public bool IsExposed()
    {
        return exposed;
    }

    public bool IsUpgradable()
    {
        return grade < values.Count - 1;
    }

    public virtual void GenerateRandomBonusRate() {}
    
    public virtual string GetOptionText()
    {
        return optionText;
    }

    public virtual string GetPercentageText()
    {
        return "";
    }

    public virtual string GetBonusText(out Color color)
    {
        color = Color.white;
        return "";
    }
    
    
    public void Buff()
    {
        buff = true;
    }
    
    public void FinishBuff()
    {
        buff = false;
    }

    public void SetGrade(int newGrade)
    {
        if (newGrade < values.Count)
        {
            grade = newGrade;
        }
        else
        {
            grade = values.Count - 1;            
        }
    }

    /// <summary>
    /// �ٸ� Upgradable�� ���� ���������� ���׷��̵� �ϰ� ���� �� ���.
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="upgradable"></param>
    public void LinkTo<U>(Upgradable<U> upgradable)
    {
        exposed = false;
        initialExposedValue = exposed;
        upgradable.Upgraded += Upgrade;
    }


    /// <summary>
    /// �ٸ� Upgradable�� ���׷��̵� �Ϸ�Ǿ��� �� ����
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="upgradable"></param>
    public void UnlockWhenComplete(IUpgradable upgradable)
    {
        exposed = false;
        initialExposedValue = exposed;
        upgradable.CallBackWhenUpgradeCompleted((u) => exposed = true);
    }

    public void UnlockWhenComplete(IUpgradable[] upgradables)
    {
        exposed = false;
        initialExposedValue = exposed;

        // ��� upgradable�� ���׷��̵尡 �Ϸ�Ǹ�

        foreach (IUpgradable upgradable in upgradables)
        {
            upgradable.CallBackWhenUpgradeCompleted((u) =>
            {
                foreach (IUpgradable other in upgradables)
                {
                    if (other.IsUpgradable())
                    {
                        return;
                    }
                }
                exposed = true;
            });
        }
    }

    /// <summary>
    /// �ٸ� Upgradable�� ���׷��̵� �Ϸ�Ǿ��� �� �ݹ�
    /// </summary>
    /// <param name="action"></param>

    public void CallBackWhenUpgradeCompleted(Action<IUpgradable> action)
    {
        UpgradeCompleted += () => action(this);
    }
    
    public void Reset()
    {
        exposed = initialExposedValue;
        grade = 0;
        UpgradeReset?.Invoke();
        Upgraded?.Invoke();
    }

    public void SetWeapon(Weapon weapon)
    {
        this.weapon = weapon;
    }

    public bool TryGetWeapon(out Weapon weapon)
    {
        weapon = this.weapon;
        return !ReferenceEquals(weapon, null);
    }
}
