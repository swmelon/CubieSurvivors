using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using Local.Scripts.Extensions;
using StarterAssets;
using UnityEngine;


public class Flamethrower : QuickFirableWeapon<UFloat>
{
    [SerializeField]
    private Sprite flameScaleIcon;

    [SerializeField]
    private Transform flameTransform;

    [SerializeField]
    private Transform flameLeft, flameRight;
    
    [SerializeField][Range(0f, 1f)]
    private float hitForceMultiplier = 0.5f;
    
    [SerializeField]
    private RayPoint rayPoint;

    [SerializeField]
    private RayPoint rayPointLeft, rayPointRight;

    [SerializeField]
    private StudioEventEmitter studioEventEmitter;


    private RaycastHit[] hits = new RaycastHit[32];
    
    struct UpgradableStat
    {
        public List<float> Damage;
        public List<float> RateOfFire, FlameScale;
        public List<bool> Unlocked;
    }
    
    private UInt UDamage;
    private UFloat UFlameScale;
    private Upgradable<bool> UUltimateUpgrade;

    private const float overheatScaleMultiplier = 0.5f;
    private int numHits;
    private float damageTimeCount = 0f;
    [SerializeField]
    private float scaleChangeTime = 5f;
    private float scaleChangeTimeCount = 0f;
    private float initialRayLength;
    private float startFlameScale = 1f, targetFlameScale = 1f;
    private bool overHeated = true;
    [SerializeField]
    private float rayLengthOffset = 0.5f;
    private CustomThirdPersonController userController;

    protected override void Awake()
    {
        base.Awake();
        initialRayLength = rayPoint.rayLength;
        onMountedOnPlayer += OnMountedOnPlayer;
        onMounted += OnMounted;
    }

    protected override void SetupUpgradables()
    {
        UpgradableStat upgradableStat = UpgradeExtension.ReadUpgradableStat<UpgradableStat>(this);
        UDamage = new UInt(upgradableStat.Damage, symbol: symbolContainer.Damage,
            optionText: CardText.DAMAGE);
        UFireWaitTime = new UFloat(upgradableStat.RateOfFire, symbol: symbolContainer.RateOfFire,
            optionText: CardText.RATE_OF_FIRE,
           useReciprocal: true);
        UFlameScale = new UFloat(upgradableStat.FlameScale, symbol: symbolContainer.Scale,
            optionText: CardText.FLAME_SCALE);
        UFlameScale.Upgraded += SetFlameScale;

        UUltimateUpgrade = new Upgradable<bool>(upgradableStat.Unlocked, optionText: CardText.THREE_X);

        IUpgradable[] otherUpgradables = {UDamage, UFireWaitTime, UFlameScale};
        UUltimateUpgrade.UnlockWhenComplete(otherUpgradables);
        UUltimateUpgrade.UpgradeCompleted += Unlock;
        UUltimateUpgrade.UpgradeReset += Lock;
    }

    private void Unlock()
    {
        flameLeft.parent.gameObject.SetActive(true);
        flameRight.parent.gameObject.SetActive(true);
    }

    private void Lock()
    {
        flameLeft.parent.gameObject.SetActive(false);
        flameRight.parent.gameObject.SetActive(false);
    }



    private void OnMountedOnPlayer()
    {
        if(!user.GetTransform().TryGetComponent(out userController))
        {
            Debug.LogWarning("User is Player but does not have a CustomThirdPersonController");
        }
    }

    private void OnMounted()
    {
        scaleChangeTimeCount = scaleChangeTime;
    }

    
    public override void Damage()
    {
    }

    private void OnEnable()
    {
        flameTransform.gameObject.SetActive(true);
    }


    private void Update()
    {
        scaleChangeTimeCount += Time.deltaTime;

        if (scaleChangeTimeCount < scaleChangeTime)
        {
            Vector3 flameScale = Mathf.Lerp(startFlameScale, targetFlameScale, scaleChangeTime) * UFlameScale.Value * Vector3.one;
            
            flameTransform.localScale = flameScale;

            float layLength = (initialRayLength + rayLengthOffset) * flameTransform.localScale.x;

            rayPoint.rayLength = layLength;

            if (UUltimateUpgrade.Value)
            {
                flameLeft.localScale = flameScale;
                flameRight.localScale = flameScale;
                rayPointLeft.rayLength = layLength;
                rayPointRight.rayLength = layLength;
            }
        }
        else
        {
            scaleChangeTimeCount = 0f;
            startFlameScale = flameTransform.localScale.x;
            overHeated = !overHeated;

            EventInstance eventInstance = studioEventEmitter.EventInstance;

            targetFlameScale = UFlameScale.Value * (overHeated ? overheatScaleMultiplier : 1f);

            if (overHeated)
            {
                eventInstance.setVolume(0.5f);
                eventInstance.setPitch(targetFlameScale);
                
            }
            else
            {
                eventInstance.setVolume(1f);
                eventInstance.setPitch(targetFlameScale);
            }

        }


        if (!mountedOnEnemy && !ReferenceEquals(target, null))
        {
            float force = flameTransform.lossyScale.x; 

            if (UUltimateUpgrade.Value)
            {
                force *= 2;
            }

            if (force > 0)
            {
                userController.AddExtraForce(-transform.forward, force);
            }
        }
    }

    protected override void FixedUpdate()
    {
        damageTimeCount += Time.fixedDeltaTime;
        
        if (!user.TryGetTarget(out target))
        {
            flameTransform.gameObject.SetActive(false);
            flameLeft.gameObject.SetActive(false);
            flameRight.gameObject.SetActive(false);
            return;
        }

        flameTransform.gameObject.SetActive(true);
        flameLeft.gameObject.SetActive(true);
        flameRight.gameObject.SetActive(true);



        if (damageTimeCount < UFireWaitTime.Value)
        {
            return;
        }

        damageTimeCount = 0f;

        if (mountedOnEnemy)
        {
            CastDamageEnemy(rayPoint);

            if (UUltimateUpgrade.Value)
            {
                CastDamageEnemy(rayPointLeft);
                CastDamageEnemy(rayPointRight);
            }
            return;
        }

        CastDamagePlayer(rayPoint);

        if (UUltimateUpgrade.Value)
        {
            CastDamagePlayer(rayPointLeft);
            CastDamagePlayer(rayPointRight);
        }
    }

    private void CastDamageEnemy(RayPoint rayPoint)
    {
        numHits = Physics.SphereCastNonAlloc(rayPoint.Origin, 0.2f * user.GetTransform().localScale.x, rayPoint.transform.forward, hits,
             rayPoint.rayLength * transform.lossyScale.x, LayerMaskCash.PlayerAndEnemy);
        Debug.DrawRay(flameTransform.position, flameTransform.forward * 6f, Color.red, 1f);
        Transform userTransform = user.GetTransform();

        for (int i = 0; i < numHits; i++)
        {
            if (hits[i].transform.TryGetComponent(out Damagable damagable) && !ReferenceEquals(hits[i].transform, userTransform))
            {
                damagable.Hit(UDamage.Value);
            }
        }
    }

    private void CastDamagePlayer(RayPoint rayPoint)
    {
        numHits = Physics.BoxCastNonAlloc(rayPoint.Origin, 0.7f * UFlameScale.Value * Vector3.one , rayPoint.transform.forward,
                       hits, Quaternion.identity, rayPoint.rayLength, LayerMaskCash.Enemy);

        for (int i = 0; i < numHits; i++)
        {
            if (hits[i].transform.TryGetComponent(out Damagable damagable))
            {
                damagable.Hit(ComputeFinalDamage(UDamage.Value, out bool isCritical), hitForce: ComputeHitForce(hits[i].point),
                    isCritical: isCritical, hitman: transform.root);
            }
        }
    }

    private void SetFlameScale()
    {
        flameTransform.localScale = Vector3.one * UFlameScale.Value;
        rayPoint.rayLength  = initialRayLength * UFlameScale.Value;
    }
  
    private Vector3 ComputeHitForce(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        return direction * hitForceMultiplier;
    }
    


    [SerializeField]
    private float _itemForwardOffset = 0.2f;
    [SerializeField]
    private float _itemUpOffset = 0.1f;

    public override void BeItem()
    {
        transform.localPosition -= _itemForwardOffset * Vector3.forward + _itemUpOffset * Vector3.up;
    }

    public override void OnUnmounted()
    {
        flameTransform.gameObject.SetActive(false);
    }
}

