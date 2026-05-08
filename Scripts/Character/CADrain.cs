
using Local.Scripts.Extensions;
using System.Collections;
using UnityEngine;

public class CADrain : CharacterAbillity
{    
    [SerializeField] 
    private GameObject auraVFXPrefab, bloodVFXPrefab;
    
    private GameObject auraVFX, bloodVFX;

    private bool isVampireMode = false;
    
    protected override void Awake()
    {
        base.Awake();
        auraVFX = Instantiate(auraVFXPrefab, transform);
        bloodVFX = Instantiate(bloodVFXPrefab);
        
        auraVFX.SetActive(false);
        bloodVFX.SetActive(false);
      
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        damagable.FinishVampireMode += OnFinishVampireMode;

        // 자유로운 CA 교체를 위해 수정할 필요가 있음
        damagable.OnHit.AddListener((var) => EnableBloodVFX());
        isVampireMode = false;
        abilitystackController.StackReleased+= OnStackReleased;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        damagable.FinishVampireMode -= OnFinishVampireMode;
        abilitystackController.StackReleased -= OnStackReleased;
    }

    public override void Perform()
    {
        controller.Dash();
    }

    private void OnFinishVampireMode()
    {
        isVampireMode = false;
        auraVFX.SetActive(false);
        useCAStack = true;
    }

    private void OnStackReleased()
    {
        damagable.OnVampireMode();
        auraVFX.SetActive(true);
        isVampireMode = true;
        useCAStack = false;
    }

    private void EnableBloodVFX()
    {
        bloodVFX.SetActive(false);
        bloodVFX.SetActive(true);
        bloodVFX.transform.position = transform.position;
    }


}
