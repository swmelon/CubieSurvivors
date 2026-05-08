using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[RequireComponent(typeof(HealthBarManager))]
[RequireComponent(typeof(ExpBarManager))]
public class PlayersUIManager : MonoBehaviour
{
    [SerializeField] 
    private float heightOffset;
    
    private HealthBarManager healthBarManager;
    private ExpBarManager expBarManager;
    private StackBarManager stackBarManager;

    private void Awake()
    {
        healthBarManager = GetComponent<HealthBarManager>();
        expBarManager = GetComponent<ExpBarManager>();

        TryGetComponent(out stackBarManager);
    }
    
    private void Start()
    {
        healthBarManager.HeightOffset = heightOffset;
        expBarManager.SetupEXPBar(heightOffset, healthBarManager.Height);
        stackBarManager?.SetupEXPBar(heightOffset, healthBarManager.Height);
    }
}
