using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cinemachine;
using Local.Scripts.Extensions;
using StarterAssets;
using UnityEngine;
using UnityEngine.Serialization;


//[RequireComponent(typeof(GravityDelegator))]
//[RequireComponent(typeof(WeaponManager))]
public class Player : MonoBehaviour, ITargetHaves, IItemizable
{
    public DamageCalculatorSO damageCalculator;
    public WeaponManager WeaponManager => weaponManager;
    public DamagablePlayer Damagable => damagable;
    public GravityDelegator Gravity => controller;

    public DeathManager DeathManager;

    public float MoveSpeed => characterController.GetMaxSpeed();
    
    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    [SerializeField] 
    private PlayerChannelSO currentPlayerChannel;

    [SerializeField]
    private EventChannelSO playerDeadEC, playerReviveEC;

    [SerializeField]
    private EventChannelSO enterEventStageEC, exitEventStageEC;

    [SerializeField]
    private EventChannelSO killAllEnemiesEC;

    [SerializeField]
    private GameWeaponManagerSO gameWeaponManager;

    [SerializeField]
    private ItemPackerSO itemPacker;

    [SerializeField]
    private InitialTransformData transformData;

    protected EnemyManager enemyManager;
    protected Partner partner;
    protected CustomThirdPersonController controller;
    protected WeaponManager weaponManager;
    private CharacterAbillity characterAbillity;
    private ICharacterController characterController;
    private float reviveHeight = 25f;

    public EnemyManager EnemyManager => enemyManager;
    
    protected DamagablePlayer damagable;
    private CharacterController unityCharacterController;
    private Transform body;
    private EnemyDropWeaponItem[] unmountedWeaponItems;
    private float slopeLimitDuringEventStage = 65f;
    private float slopeLimitNormal = 55f;

    private void Awake()
    {
        controller = GetComponent<CustomThirdPersonController>();
        weaponManager = GetComponent<WeaponManager>();
        enemyManager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager>();
        damageCalculator.Initialize();
        characterController = GetComponent<ICharacterController>();
        characterAbillity = GetComponent<CharacterAbillity>();
        unityCharacterController = GetComponent<CharacterController>();
        body = transform.GetChild(0);
    }

    private void OnEnable()
    {
        damagable = GetComponent<DamagablePlayer>();
        playerTransformChannel.Register(transform);
        currentPlayerChannel.Register(this);
        enterEventStageEC.Subscribe(OnEnteredEventStage);
        exitEventStageEC.Subscribe(OnExitEventStage);
        enemyManager.AddPlayer(transform);
    }
    
    private void OnDisable()
    {
        playerTransformChannel.Unregister(transform);
        currentPlayerChannel.Unregister(this);
        enterEventStageEC.Unsubscribe(OnEnteredEventStage);
        exitEventStageEC.Unsubscribe(OnExitEventStage);
        enemyManager.RemovePlayer(transform);
        transformData.ResetTransform(transform);
    }
    
    protected virtual void Start()
    {
        damagable.OnDead.AddListener(OnDead);
    }
    
    public bool TryGetTarget(out Transform target)
    {
        if (enemyManager.GetNearestEnemy(transform, out Enemy enemy))
        {
            target = enemy.transform;
            return true;
        }

        target = null;
        return false;
    }

    public bool TryGetTarget(out Transform target, out float distance)
    {
        if (enemyManager.GetNearestEnemy(transform, out Enemy enemy, out distance))
        {
            target = enemy.transform;
            return true;
        }

        target = null;
        return false;
    }
    
    public int GetTargets(int maxTargets, int maxRange, out Transform[] targets)
    {
        return enemyManager.GetEnemiesFromBucketAscendingDistance(transform, maxTargets, maxRange, out targets);
    }

    public void BeMaster(Partner partner)
    {
        if (this.partner == null)
        {
            partner.SetMaster(this);
            this.partner = partner;
            controller.SyncState(partner.gameObject.GetComponent<GravityDelegator>());
        }
        else
        {
            this.partner.BeMaster(partner);
        }
    }

    public void PartnerDie(Partner newPartner)
    {
        partner = newPartner;
    }
    
    public Transform GetTransform() => transform;

    protected virtual void OnDead()
    {
        if (partner != null)
        {
            partner.MasterDie();
        }

        enemyManager.RemovePlayer(transform);

        

        unmountedWeaponItems = itemPacker.WarpUp<Weapon, EnemyDropWeaponItem>(weaponManager.UnmountAll(), transform.position, LayerMaskCash.Item);
        controller.IgnoreInput();
        playerDeadEC.Raise();

        // �ٽ� ��Ȱ�� ��� ��� ���⸦ �÷��̾������� ������

    }

    private void UnmountAllAndReturn()
    {
        foreach (var weapon in weaponManager.UnmountAll())
        {
            gameWeaponManager.ReturnWeaponInstance(weapon);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled)
        {
            return;
        }

        if (!damagable.IsDead && other.TryGetComponent(out Item item))
        {
            item.Activate(this);
        }
    }

    public void EnableEternalLifeForDebug()
    {
        damagable.OnDead.RemoveListener(OnDead);
        damagable.OnDead.AddListener(damagable.Revive);
    }

    public void Revive()
    {
        damagable.Revive();
        enemyManager.AddPlayer(transform);
        controller.ListenInput();
        playerReviveEC.Raise();

        for (int i = 0; i < unmountedWeaponItems.Length; i++)
        {
            unmountedWeaponItems[i].TrackPlayer();
        }

        killAllEnemiesEC.Raise();
    }

    public async void ExplodeAndRevive()
    {
        DeathManager.SpawnExplosionEffect(transform);
        body.gameObject.SetActive(false);
        await Task.Delay(1000);
        body.gameObject.SetActive(true);
        Vector3 position = transform.position;
        position.y = reviveHeight;
        controller.MoveOnlyCharacterTo(position);
        Revive();
    }

    public void BeItem()
    {
        controller.enabled = false;
        this.enabled = false;

        if (!ReferenceEquals(characterAbillity, null))
        {
            characterAbillity.enabled = false;
        }
        transform.localScale = 0.5f * Vector3.one;
    }

    public void Deitemize()
    {
        controller.enabled = true;
        this.enabled = true;

        if (!ReferenceEquals(characterAbillity, null))
        {
            characterAbillity.enabled = true;
        }
        transform.localScale = Vector3.one;
    }

    private void OnEnteredEventStage()
    {
        UnmountAllAndReturn();
        damagable.Invincible();
        unityCharacterController.slopeLimit = slopeLimitDuringEventStage;
    }

    private void OnExitEventStage()
    {
        damagable.OffInvincible();
        unityCharacterController.slopeLimit = slopeLimitNormal;
    }
}
