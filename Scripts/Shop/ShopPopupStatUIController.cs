using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;


public enum ItemStatType
{
    Attack,
    Defense,
    Speed,
    Luck
}

public class ShopPopupStatUIController : MonoBehaviour
{
    [SerializeField]
    private ItemStatType statType;

    [SerializeField]
    private TextMeshProUGUI stats;

    public bool SetStat(Accessory accessory)
    {
        switch (statType)
        {
        }

        return true;
    }


    private void SetText(int stat)
    {
        if (stat >= 0)
        {
            stats.text = "+" + (stat * 10).ToString();
        }
        else
        {
            stats.text = (stat * 10).ToString();
        }
    }
}