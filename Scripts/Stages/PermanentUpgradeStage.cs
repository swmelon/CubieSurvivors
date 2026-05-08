
using Local.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Serialization;

public class PermanentUpgradeStage : EventStage
{
    [SerializeField]
    private PermanentUpgradableStat permanentUpgradableStat;
  
    [SerializeField]
    private int minimumAppearanceInterval = 10;

    [SerializeField] 
    private int definiteAppearanceInterval = 10;
    
    private int turnOverCount;
    
}
