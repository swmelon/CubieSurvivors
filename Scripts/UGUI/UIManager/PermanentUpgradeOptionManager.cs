
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class PermanentUpgradeOptionManager : MonoBehaviour
{
    [SerializeField]
    private PermanentUpgradableStat permanentUpgradableStat;

    [SerializeField]
    private FontManager fontManager;

    private PermanentUpgradeOptionUI[] options;
    private TMP_Text[] texts;

    private void Awake()
    {
        options = GetComponentsInChildren<PermanentUpgradeOptionUI>(); 
        texts = GetComponentsInChildren<TMP_Text>();

        HashSet<PermanentUpgradeOptionUI> optionSet = new HashSet<PermanentUpgradeOptionUI>(options);
        
        if (optionSet.Count != options.Length)
        {
            Debug.LogError("Duplicate PermanentUpgradeOptionUI in PermanentUpgradeOptionManager");
        }

    }

    private void OnEnable()
    {
        SetFont();
    }



    private void SetFont()
    {
        TMP_FontAsset fontAsset = fontManager.GetCurrentUITMPFontAsset();

        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].font = fontAsset;
        }
    }

    private void Start()
    {
        
        for (int i = 0; i < options.Length; i++)
        {
            if (!permanentUpgradableStat.TryGetCost(options[i].Option, out var cost))
            {
                Debug.LogError("Could not get cost for " + options[i].Option);
                continue;
            }
            
            options[i].SetCost(cost);
            options[i].SetUpgradableStat(permanentUpgradableStat);
        }
    }
}
