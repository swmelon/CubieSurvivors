
using Local.Scripts.Extensions;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CATeleport : CharacterAbillity
{
    [SerializeField] 
    private VFXController teleportableZoneFXPrefab;
    
    [SerializeField]
    private GameObject teleportEffectPrefab;

    [SerializeField]
    private SFXTags teleportSound;

    private VFXController teleportableZoneFX;
    private GameObject teleportEffect;

    private Collider[] colliders = new Collider[32];
    
    protected override void Awake()
    {
        base.Awake();
        teleportableZoneFX = Instantiate(teleportableZoneFXPrefab);
        teleportEffect = Instantiate(teleportEffectPrefab);
        teleportEffect.gameObject.SetActive(false);
    }
    
    public override void Perform()
    {
        teleportEffect.SetActive(false);
        teleportEffect.transform.position = transform.position;
        teleportEffect.SetActive(true);
        FMODAudioManager.instance.PlayOneShot(teleportSound, transform.position);
        controller.Teleport();
        teleportableZoneFX.TurnOffAndOn();
    }

    private void Explode(Vector3 position)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(position, 1f,  colliders, LayerMaskCash.OnlyEnemy, QueryTriggerInteraction.Ignore);      
        
        for(int i = 0; i < hitCount; i++)
        {
            if (colliders[i] != null && colliders[i].transform.TryGetComponent(out Damagable damagable))
            {
            }
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        teleportableZoneFX.gameObject.SetActive(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (teleportableZoneFX != null)
        {
            teleportableZoneFX.gameObject.SetActive(false);
        }
    }

    protected override void EnableAbillity()
    {
        base.EnableAbillity();
        teleportableZoneFX.gameObject.SetActive(true);
    }

    protected override void DisableAbillity()
    {
        base.DisableAbillity();
        teleportableZoneFX.gameObject.SetActive(false);
    }
}
