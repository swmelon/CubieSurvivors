using UnityEngine;
using System.Collections.Generic;
using Local.Scripts.Extensions;


[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Renderer))]
public class ItemBox : StaticEnemy, ILocatable
{
    [SerializeField]
    private List<EventChannelSO> manualForceKillEC;

    [SerializeField]
    private ItemSpawner itemSpawner;
    public bool HasItem => !ReferenceEquals(item, null);
    
    private Item item = null; 
    private int shakeHash;
    private Animator animator;

    public override bool UseGravity
    {
        set => rb.useGravity = value;
    }

    protected override void Awake()
    {
        shakeHash = Animator.StringToHash("Shake");
        animator = GetComponent<Animator>();
        
        if (item != null)
        {
            SetItem(item);
        }
        
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    protected override void OnSpawned()
    {
        base.OnSpawned();
        ResetItem();
    }

    // ILocatable
    public bool SelectLocation(Dictionary<Vector3, Vector3[]> locations, out List<Vector3> selected)
    {
        List<Vector3> pickedLocations = new List<Vector3>();
        
        foreach (var oneSide in locations.Values)
        {
            foreach (var loc in oneSide)
            {
                if (loc != Vector3.zero)
                {
                    pickedLocations.Add(loc);
                }
            }
        }

        if (pickedLocations.TryPickRandom(out Vector3 pickedLocation))
        {
            selected = new List<Vector3> {pickedLocation};
            return true;
        }
        else
        {
            selected = default;
            return false;
        }
    }
    public void SetItem(Item newItem)
    {
        if (!ReferenceEquals(item, null))
        {
            Debug.LogError("ItemBox already has item.");
        }

        item = newItem;
        newItem.gameObject.SetActive(false);
        
        if (newItem.GetType() == typeof(PartnerItem))
        {   
            animator.SetBool(shakeHash, true);
        }
        else
        {
            animator.SetBool(shakeHash, false);
        }
    }

    protected override void OnDead()
    {
        // Static Enemy로 취급
        managerChannel.Unsubscribe(this, target);
        deathManager.OnItemBoxCrashed(this);

        DetachItem();
        base.Release();
    }

    public void ResetItem()
    {
        if (HasItem)
        {
            if (!item.Released)
            {
                item.Release();
            }
            item = null;
        }
    }
    
    
    public Item DetachItem()
    {
        Item toReturn = item;

        if (!HasItem || item.Released)
        {
            item = itemSpawner.SpawnAdaptiveRandomItem();
        }
        
        item.gameObject.SetActive(true);

        Transform itemTransform = item.transform;
        
        itemTransform.parent = null;
        itemTransform.SetPositionAndRotation(transform.position + 0.5f * Vector3.up, Quaternion.identity);

        item = null;

        return toReturn;
    }

    public bool ReleaseItem()
    {
        if (!HasItem || item.Released)
        {
            item = null;
            return false;
        }

        item.gameObject.SetActive(true);
        item.transform.parent = null;
        item.Release();
        item = null;

        return true;
    }

    public override void ForceKill(bool spawnExp = false, bool ignore = false)
    {
        if (!ignore)
        {
            ReleaseItem();
            base.ForceKill();
        }
    }
}
