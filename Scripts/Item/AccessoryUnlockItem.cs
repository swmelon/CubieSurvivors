using UnityEngine;
using System.Collections.Generic;


[RequireComponent(typeof(AccessorySlot))]
public class AccessoryUnlockItem : CoveredItem<Accessory>
{
    [SerializeField]
    private int slotID = -1;

    [SerializeField]
    private int accID = -1;

    [SerializeField]
    private GameAccessoryManager gameAccManager;

    [SerializeField]
    private OnePureEffectSpawner disappearingEffectSpawner;

    [SerializeField]
    private CardSelectionScreenController cardSelectionScreenController;

    [SerializeField]
    private BooleanEventChannelSO userInputControlChannel;

    [SerializeField]
    private EventChannelSO getNewAccEC;

    private AccessorySlot slot;
    private AccData accData;

    protected override void Awake()
    {
        base.Awake();
        slot = GetComponent<AccessorySlot>();
    }

    private void Start()
    {
        if(!gameAccManager.TryGetAccessoryInAccUnlockItem(slotID, accID, out accData))
        {
            gameObject.SetActive(false);
            return;
        }

        Accessory instance = slot.EquipAccessory(accData);
        SetContent(instance);
        collider.enabled = true;
    }

    public override void SetContent(Accessory instance)
    {
        this.content = instance;
        instance.BeItem();
    }

    public override void Activate(Player player)
    {
        slot.UnequipAccessory();

        // show cards
        AccessoryCardData accCardData = new AccessoryCardData(accData, gameAccManager, AccessoryCardData.InteractionMode.GetNewFromRewardStage);
        userInputControlChannel.Raise(false);
        cardSelectionScreenController.ShowCardSelectionScreen(true, UIText.OK, OnConfirm);
        cardSelectionScreenController.ShowCard(accCardData);

        disappearingEffectSpawner.Spawn().transform.position = transform.position;
        Destroy(gameObject);
    }

    private void OnConfirm()
    {
        userInputControlChannel.Raise(true);
        getNewAccEC?.Raise();
    }
}