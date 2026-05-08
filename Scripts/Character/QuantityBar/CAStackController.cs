using Minimalist.Quantity;
using UnityEngine;
using System;
using Minimalist.Bar;
using Unity.VisualScripting;

public class CAStackController : OptionalQuantityUIController
{
    public event Action StackFull;
    public event Action StackReleased;

    [SerializeField]
    [Range(2, 5)]
    private int maxStackCount = 3;

    private int stackCount;

    public int StackCount
    {
        get => stackCount;
        set
        {
            stackCount = value;

            if (stackCount >= maxStackCount)
            {
                StackFull?.Invoke();
            }
            
            quantityBhv.FillAmount = (float)stackCount/maxStackCount;
        }
    }

    private void Start()
    {
        quantityBhv.MaximumAmount = maxStackCount;
        StackCount = 0;
    }

    public void StackUp()
    {
        if (StackCount >= maxStackCount)
        {
            StackReleased?.Invoke();
            StackCount = -1;
        }

        StackCount++;
    }
}