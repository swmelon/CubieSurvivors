using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public class FloorItemBoxBuilder : MonoBehaviour
{
    [SerializeField]
    private ItemSpawner itemSpawner;

    [SerializeField]
    private ItemBoxSpawner itemBoxSpawner;

    [SerializeField]
    private float numItemBoxMultiplier = 1.5f;

    [SerializeField]
    private WorldDirectionChannelSO worldDirectionChannel;

    [SerializeField]
    private EventChannelSO finishStageMoveChannel;

    private FloorLEDBuilder floorLEDBuilder;
    private List<ItemBox> activeBoxes, inactiveBoxes;
    private float itemBoxHeight = 0.5f;

    private void Awake()
    {
        floorLEDBuilder = transform.root.GetComponentInChildren<FloorLEDBuilder>();
        activeBoxes = new List<ItemBox>();
        inactiveBoxes = new List<ItemBox>();
    }

    private void OnEnable()
    {
        finishStageMoveChannel.Subscribe(DeparentBoxes);
    }

    private void OnDisable()
    {
        finishStageMoveChannel.Unsubscribe(DeparentBoxes);
    }


    private int GenerateRandomNumItemBox(int stageSize)
    {
        return Mathf.RoundToInt(numItemBoxMultiplier * Mathf.Sqrt(stageSize));
    }

    private ItemBox BoxItem(Item item)
    {
        ItemBox itemBox = itemBoxSpawner.Spawn();
        itemBox.SetItem(item);
        return itemBox;
    }

    public void BuildFloorItemBox()
    {
        // buildLEDFloor first
        List<ItemBox> itemBoxes = new List<ItemBox>();

        int numItemBox = GenerateRandomNumItemBox(floorLEDBuilder.Size);

        for (int i = 0; i < numItemBox; i++)
        {
            itemBoxes.Add(itemBoxSpawner.Spawn());
        }

        for (int i = 0; i < itemBoxes.Count; i++)
        {
            ItemBox itemBox = itemBoxes[i];

            if (floorLEDBuilder.TryGetOnFloorObjectPosition(out Vector3 position))
            {
                position.y += itemBoxHeight;
                itemBox.gameObject.SetActive(false);
                itemBox.transform.SetParent(floorLEDBuilder.transform);
                itemBox.transform.SetPositionAndRotation(position, worldDirectionChannel.RandomRotation());
                inactiveBoxes.Add(itemBox);
            }
            else
            {
                itemBox.ForceKill(ignore: false);
            }
        }
    }
    
    public void ActivateBoxes()
    {
        for (int i = 0; i < inactiveBoxes.Count; i++)
        {
            ItemBox itemBox = inactiveBoxes[i];
            itemBox.gameObject.SetActive(true);
            activeBoxes.Add(itemBox);
        }

        inactiveBoxes.Clear();
    }

    public void DeparentBoxes()
    {
        for (int i = 0; i < activeBoxes.Count; i++)
        {
            ItemBox itemBox = activeBoxes[i];
            
            if (itemBox.gameObject != null)
            {
                itemBox.transform.SetParent(null);
            }
        }

        activeBoxes.Clear();
    }
}