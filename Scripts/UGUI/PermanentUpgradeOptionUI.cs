
using Minimalist.Quantity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PermanentUpgradeOptionUI : MonoBehaviour
{
    public PermanentUpgradeOption Option => option;
    
    [SerializeField] 
    private PermanentUpgradeOption option;
    
    [SerializeField]
    private TextMeshProUGUI costText;
    
    [SerializeField]
    private Slider slider;
    
    [SerializeField]
    private Button button;
    
    private Upgradable<int> UCost;
    private PermanentUpgradableStat upgradableStat;
    private QuantityBhv quantityBhv;
    
    private void Awake()
    {
        quantityBhv = GetComponentInChildren<QuantityBhv>();
        button.onClick.AddListener(OnClick);
    }
    
    public void SetCost(Upgradable<int> UCost)
    {
        this.UCost = UCost;
        SetupUI();
    }
    
    public void SetUpgradableStat(PermanentUpgradableStat saveFile)
    {
        this.upgradableStat = saveFile;
    }

    private void SetupUI()
    {
        quantityBhv.FillAmount= UCost.GetCompletionPercentage();
        
        if (UCost.IsUpgradable())
        {
            costText.text = UCost.Value.ToString();
        }
        else
        {
            costText.text = "MAX";
            button.interactable = false;
        }
    }
    
    public void OnClick()
    {
        if (!upgradableStat.IsUpgradable(option))
        {
            FMODAudioManager.instance.UIButtonClickedNegative();
            Debug.Log("Not enough coins");
            return;
        }

        FMODAudioManager.instance.UpgradeStat();
        upgradableStat.Upgrade(option);
        SetupUI();
    }
}
