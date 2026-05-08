using Local.Scripts.Extensions;
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class ZoneTrigger : MonoBehaviour
{
    [SerializeField]
    [FormerlySerializedAs("triggerExitManually")]
    private bool exitManually = false;

    public event Action<ZoneTrigger> PlayerEnter, PlayerExit;
    private BoxCollider triggerArea;
    private Player player;
    private static bool entering = false;
    private static int enteringWaitCount = 2;
    private int count = 0;
    Collider[] collidersInTrigger;

    public bool isPlayerInZone => !ReferenceEquals(player, null);
    
    protected virtual void Awake()
    {
        triggerArea = GetComponent<BoxCollider>();
        collidersInTrigger = new Collider[4];
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player enteringPlayer) && enteringPlayer.enabled)
        {
            player = enteringPlayer;
            PlayerEnter?.Invoke(this);
            entering = true;
            count = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Player exitingPlayer))
        {
            if (!ReferenceEquals(player, exitingPlayer))
            {
                return;
            }

            player = null;
            PlayerExit?.Invoke(this);
        }
    }

    public void ExitZoneManually()
    {
        player = null;
        PlayerExit?.Invoke(this);
    }

    private void FixedUpdate()
    {
        if (ReferenceEquals(player, null) || exitManually)
        {
            return;
        }

        int num = Physics.OverlapBoxNonAlloc(triggerArea.bounds.center, triggerArea.bounds.extents,
            collidersInTrigger, triggerArea.transform.rotation, LayerMaskCash.OnlyPlayer);

        for (int i = 0; i < num; i++)
        {
            if (collidersInTrigger[i].TryGetComponent(out Player playerInZone) && playerInZone.enabled
                && ReferenceEquals(playerInZone, player))
            {
                if (entering && count < enteringWaitCount)
                {
                    count++;
                }
                else
                {
                    entering = false;
                }

                return;
            }
        }

        player = null;

        if (entering)
        {
            return;
        }

        PlayerExit?.Invoke(this);
    }

    private void OnDisable()
    {
        if (!ReferenceEquals(player, null))
        {
            PlayerExit?.Invoke(this);
            player = null;
        }
    }
}