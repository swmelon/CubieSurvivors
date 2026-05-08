
using StarterAssets;
using UnityEngine;

public abstract class CharacterAbillity : MonoBehaviour
{
    [SerializeField]
    private EventChannelSO enterEventStageChannel;

    [SerializeField]
    private EventChannelSO exitEventStageChannel;

    [SerializeField]
    protected bool useCAStack;

    [SerializeField]
    private bool useRageStack;

    protected DamagablePlayer damagable;
    protected CustomThirdPersonController controller;
    
    
    private bool isLocked = false;

    protected Animator animator;
    protected Player player;
    protected CAStackController abilitystackController;
    protected RageStackController rageStackController;
    
    // 고유 능력치
    private float magicalBonus, physicalBonus, criticalBonus, attackSpeedBonus, moveSpeedBonus;

    public abstract void Perform();

    public void PerformIfNotLocked()
    {
        if (isLocked)
        {
            return;
        }

        Perform();
        StackUp();
    }

    protected virtual void Awake()
    {
        damagable = GetComponent<DamagablePlayer>();
        controller = GetComponent<CustomThirdPersonController>();
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        useCAStack = TryGetComponent<CAStackController>(out abilitystackController);
        useRageStack = TryGetComponent<RageStackController>(out rageStackController);
        ReadProfile();
    }

    protected virtual void OnEnable()
    {
        enterEventStageChannel.Subscribe(DisableAbillity);
        exitEventStageChannel.Subscribe(EnableAbillity);
    }

    protected virtual void OnDisable()
    {
        enterEventStageChannel.Unsubscribe(DisableAbillity);
        exitEventStageChannel.Unsubscribe(EnableAbillity);
    }

    protected virtual void EnableAbillity()
    {
        isLocked = false;
    }

    protected virtual void DisableAbillity()
    {
        isLocked = true;
    }

    private void ReadProfile()
    {
    }

    protected void StackUp()
    {
        if (useCAStack)
        {
            abilitystackController.StackUp();
        }
    }
}
