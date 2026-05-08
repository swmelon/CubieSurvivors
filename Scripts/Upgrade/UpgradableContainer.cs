using System.Collections.Generic;
using System.Reflection;
using Local.Scripts.Extensions;
using UnityEngine;

public abstract class UpgradableContainer : MonoBehaviour, IUpgradableContainer
{

    /// <summary>
    /// deceprated. 
    /// </summary>
    public List<IUpgradable> Upgradables
    {
        get
        {
            List<IUpgradable> exposableUpgradables = new List<IUpgradable>();
            
            for (int i = 0; i < upgradables.Count; i++)
            {
                IUpgradable upgradable = upgradables[i];

                if (upgradable.IsExposed() && upgradable.IsUpgradable())
                {
                    exposableUpgradables.Add(upgradable);
                }
            }

            if (exposableUpgradables.Count == 0)
            {
                NoUpgradables(exposableUpgradables);
            }
                return exposableUpgradables;
        }
    }
    
    protected List<IUpgradable> upgradables = new List<IUpgradable>();

    protected abstract void SetupUpgradables();
    
    protected virtual void Awake()
    {
        SetupUpgradables();
        RegisterUpgradables();
    }
    
    protected void RegisterUpgradables()
    {
        // 초기화 해줘야 실수로 두 번 호출되어도 문제 없음. 물론 두 번 호출되는 상황도 고쳐야지
        upgradables.Clear();

        FieldInfo[] fieldInfos;
        fieldInfos = GetType().GetFields(BindingFlags.NonPublic |
                                         BindingFlags.Instance);

        foreach (var item in fieldInfos)
        {
            if (typeof(IUpgradable).IsAssignableFrom(item.FieldType))
            {
                upgradables.Add((IUpgradable)item.GetValue(this));
            }
        }
    }

    protected virtual void NoUpgradables(List<IUpgradable> upgradables)
    {

    }

    public bool TryGetRandomAvailableUpgradable(out IUpgradable selectedUpgradable)
    {
        upgradables.FisherShuffle();

        for (int i = 0; i < upgradables.Count; i++)
        {
            IUpgradable upgradable = upgradables[i];

            if (upgradable.IsExposed() && upgradable.IsUpgradable())
            {
                selectedUpgradable = upgradable;
                return true;
            }
        }

        selectedUpgradable = null;
        return false;
    }

    public int GetAvailableUpgradablesNonAlloc(IUpgradable[] availableUpgradables)
    {
        int count = 0;
        int length = upgradables.Count;

        for (int i = 0; i < upgradables.Count; i++)
        {
            IUpgradable upgradable = upgradables[i];

            if (upgradable.IsExposed() && upgradable.IsUpgradable())
            {
                if (count >= length)
                {
                    return count;
                }

                availableUpgradables[count] = upgradable;
            }
        }

        return count;
    }

    public int GetAvailableUpgradablesRandomOrder(IUpgradable[] availableUpgradables)
    {
        upgradables.FisherShuffle();
        return GetAvailableUpgradablesNonAlloc(availableUpgradables);
    }

    public bool UpgradeRandom()
    {
        bool result = TryGetRandomAvailableUpgradable(out IUpgradable upgradable);

        if (result)
        {
            upgradable.Upgrade();
            return true;
        }

        return false;
    }

    public bool UpgradeRandom(int times)
    {
        for (int i = 0; i < times; i++)
        {
            if (!UpgradeRandom())
            {
                return false;
            }
        }

        return true;
    }
    
    public int GetNumOfUpgradableTimes()
    {
        int grade = 0;
        upgradables.ForEach(upgradable => grade += upgradable.GetGrade());
        return grade;
    }
    
    public void ResetUpgrade()
    {
        Upgradables.ForEach(upgradable => upgradable.Reset());
    }

    private void CheckIfDuplicated(List<IUpgradable> upgradables)
    {
        for (int i = 0; i < upgradables.Count; i++)
        {
            for (int j = i + 1; j < upgradables.Count; j++)
            {
                if (upgradables[i] == upgradables[j])                {
                    Debug.LogError("Duplicated Upgradable: " + upgradables[i].GetType());
                }
            }
        }
    }
}
