using Local.Scripts.Extensions;
using System.Runtime.CompilerServices;
using UnityEngine;

public class IcecreamWeapon : WeaponDisposable
{
    private struct UpgradableInfo
    {
        // Define Damage, RateOfFire, or whatever you want.
    }

    [SerializeField]
    private int damage = 20;

    [SerializeField]
    private float launchForce = 1f;

    [SerializeField]
    private float rotationSpeed = 720f;

    [SerializeField]
    private BooleanChannelSO freefallChannel;

    [SerializeField]
    private EventChannelSO startStageMoveEC;

    [SerializeField]
    private OnePureEffectSpawner effectSpawner;

    [SerializeField]
    private GameObject trailEffect;

    private Vector3 initialLocalPos;
    private Quaternion initialLocalRot;
    private Vector3 moveDirection;
    private bool launched = false;
    private bool landed = false;
    private Vector3 cumlatedGravity = Vector3.zero;
    private Transform icecreamMeshObject;
    private IAttackPattern attackPattern;

    protected override void Awake()
    {
        base.Awake();
        icecreamMeshObject = transform.GetChild(0);
        attackPattern = GetComponent<IAttackPattern>();
        trailEffect.SetActive(false);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        startStageMoveEC.Subscribe(OnStartStageMove);
    }

    private void OnDisable()
    {
        launched = false;
        landed = false;
        cumlatedGravity = Vector3.zero;
        icecreamMeshObject.localRotation = Quaternion.identity;
        startStageMoveEC.Unsubscribe(OnStartStageMove);
        trailEffect.SetActive(false);
    }

    public override void Damage()
    {
        // This method will be call by AttackPattern.

        if (Released)
        {
            return;
        }

        if (launched)
        {
            return;
        }

        Transform myTrans = transform;

        weaponManager.Unmount(this, deactivate: false);

        trailEffect.SetActive(true);

        moveDirection = myTrans.up * launchForce;
        launched = true;
    }



    protected override void SetupUpgradables()
    {
        // If you want to implement a upgradable weapon,
        // First : Define UpgradableStat and call ReadUpgradableInfo() to read a json file.
        // The json file name must be same with a weapon class name.
        // Second : Define Upgradables and initialize using returned UpgradableStat.
    }

    public override void OnUnmounted()
    {
    }



    private void Update()
    {
        if (!launched || landed)
        {
            return;
        }

        UpdateMoveDirection();

        icecreamMeshObject.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime);
    }

    protected override void FixedUpdate()
    {
        if (!launched || landed)
        {
            return;
        }

        Physics.Raycast(icecreamMeshObject.position, moveDirection, out RaycastHit hit, 0.3f, LayerMaskCash.Ground);

        if (hit.collider != null)
        {
            landed = true;
            trailEffect.SetActive(false);
            FMODAudioManager.instance.PlayOneShot(SFXTags.Splat);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (!Released && launched && !landed && other.CompareTag("Player") && other.transform.TryGetComponent(out Damagable damagable))
        {
            damagable.Hit(damage);
            Explode();
            CheckActivation();
            Release();
        }
    }

    private void UpdateMoveDirection()
    {
        // apply gravity

        if (!freefallChannel.Value)
        {
            moveDirection += Physics.gravity * Time.deltaTime;

            if (cumlatedGravity != Vector3.zero)
            {
                moveDirection += cumlatedGravity;
                cumlatedGravity = Vector3.zero;

                // Terminal velocity를 전역변수로
                if (moveDirection.y < -30f)
                {
                    moveDirection.y = -30f;
                }
            }
        }
        else
        {
            cumlatedGravity += Physics.gravity * Time.deltaTime;
        }

        transform.position += moveDirection * Time.deltaTime;
    }

    private void OnStartStageMove()
    {
        if (landed && !Released)
        {
            CheckActivation();
            Release();
        }
    }

    private void Explode()
    {
        effectSpawner.Spawn(transform.position, Quaternion.identity);
    }

    private void CheckActivation()
    {
        if (attackPattern.IsActivated())
        {
            attackPattern.StopAction();
        }

        weaponManager.Unmount(this, deactivate: false);
    }
}