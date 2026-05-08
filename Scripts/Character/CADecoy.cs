using StarterAssets;
using Unity.VisualScripting;
using UnityEngine;


public class CADecoy : CharacterAbillity
{
    [SerializeField]
    private GameObject decoyPrefab;

    [SerializeField]
    private EventChannelSO enterBossStageChannel;

    [SerializeField]
    private GameObject appearanceEffectPrefab;

    private AccessoryManager accessoryManager;
    private GameObject decoy, fakeMaster;
    private PartnerInput parterInput;
    private PartnerMovementController decoyMovementController;
    private Damagable decoyDamagable;
    private GameObject appearanceEffect;
    private bool isRageFull = false;
    private BuffController buffController;

    private const float decoyPositionOffset = 1.3f;
    [SerializeField]
    private float _buffDuration = 2f;
    [SerializeField]
    private float _buffActiveDuration = 3f;
    private float buffDuration;
    private float buffDurationTimer = 0f;
    

    protected override  void Awake()
    {
        base.Awake();
        accessoryManager = GetComponent<AccessoryManager>();
        buffController = GetComponent<BuffController>();
        buffDuration = _buffDuration;
        

        decoy = Instantiate(decoyPrefab, transform.position, Quaternion.identity);
        decoy.SetActive(false);
        parterInput = decoy.GetComponent<PartnerInput>();
        parterInput.SetMaster(transform);
        decoyDamagable = decoy.GetComponent<Damagable>();
        decoyMovementController = decoy.GetComponent<PartnerMovementController>();
        appearanceEffect = Instantiate(appearanceEffectPrefab, transform.position, Quaternion.identity);
        appearanceEffect.SetActive(false);

        controller.SyncState(decoyMovementController);
        decoyDamagable.OnDead.AddListener(OnDecoyDead);


    }

    private void Start()
    {
        accessoryManager.Copy(decoy.GetComponent<AccessoryManager>());
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        rageStackController.RageFull += OnRageFull;
        rageStackController.RageReleased += OnRageRelease;
        enterBossStageChannel.Subscribe(decoyDamagable.Kill);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        rageStackController.RageFull -= OnRageFull;
        rageStackController.RageReleased -= OnRageRelease;
        enterBossStageChannel.Unsubscribe(decoyDamagable.Kill);
    }

    
    public override void Perform()
    {
        controller.Jump();

        if (!isRageFull || decoy.activeSelf)
        {
            return;
        }

        Vector3 playerDirection = controller.GetDirection();
        Vector3 decoyPosition = transform.position - decoyPositionOffset * playerDirection;

        decoyDamagable.Revive();

        decoyMovementController.SetPosition(decoyPosition);
        appearanceEffect.transform.position = decoyPosition;
        appearanceEffect.SetActive(false);
        appearanceEffect.SetActive(true);
        decoy.SetActive(true);
        player.EnemyManager.SubstitutePlayerWith(decoy.transform, transform);
        buffDurationTimer = 0f;
        buffController.BuffRandom(_buffActiveDuration);

        rageStackController.ReleaseRage();
    }

    protected override void DisableAbillity()
    {
        base.DisableAbillity();
        decoyDamagable.Kill();
    }

    private void OnRageFull()
    {
        isRageFull = true;
    }

    private void OnRageRelease() 
    { 
        isRageFull = false;
     }

    private void OnDecoyDead()
    {  
        decoy.SetActive(false);
        player.EnemyManager.RemoveSubstitute(player.transform);
        player.DeathManager.OnAllyDead(decoy.transform);
        rageStackController.Unlock();
    }

    private void Update()
    {
        if (!decoy.activeSelf)
        {
            return;
        }
        
        
        
        buffDurationTimer += Time.deltaTime;

        if (buffDurationTimer >= buffDuration)
        {
            buffDurationTimer = 0f;
            buffController.BuffRandom(_buffActiveDuration);
        }

    }
}
