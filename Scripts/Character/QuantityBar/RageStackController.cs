using System.Collections;
using UnityEngine;
using System;


public class RageStackController : OptionalQuantityUIController
{
    public event Action RageFull;
    public event Action RageReleased;

    private Player player;
    private Damagable damagable;
    private float stackingTick = 1f;
    private float stackingTimer = 0f;
    private bool rageFull = false;
    private bool locked = false;


    private void Awake()
    {
        player = GetComponent<Player>();
        damagable = GetComponent<Damagable>();

        damagable.OnHit.AddListener( (var) => FillRage(0.1f));
    }

    private void Update()
    {
        if (!player.enabled)
        {
            return;
        }

        bool hasTarget = player.TryGetTarget(out Transform target);

        if (hasTarget)
        {
            stackingTimer += Time.deltaTime;

            if (stackingTimer >= stackingTick)
            {
                stackingTimer = 0f;
                FillRage(0.1f);
            }

            if (!rageFull && quantityBhv.FillAmount > 0.99f)
            {
                RageFull?.Invoke();
                rageFull = true;
            }
        }
    }

    public void ReleaseRage()
    { 
        if (!rageFull)
        {
            return;
        }

        rageFull = false;
        RageReleased?.Invoke();
        quantityBhv.FillAmount = 0f;
        locked = true;
    }

    public void Unlock()
    {
        locked = false;
    }

    private void FillRage(float amount)
    {
        if (locked)
        {
            return;
        }
        quantityBhv.FillAmount = Mathf.Clamp(quantityBhv.FillAmount + amount, 0f, 1f);
    }
}
