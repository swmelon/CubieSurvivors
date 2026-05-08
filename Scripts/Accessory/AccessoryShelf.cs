using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Local.Scripts.Extensions;
using System.Runtime.CompilerServices;
using UnityEngine.UIElements;

public class AccessoryShelf : MonoBehaviour
{
    [SerializeField]
    private GameAccessoryManager gameAccManager;

    [SerializeField]
    private AccessoryInventoryScreen accInventoryScreen;

    [SerializeField]
    private AccessoryCollectionScreen accCollectionScreen;

    [SerializeField]
    private CharacterManagerSO characterManager;

    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private EventChannelSO enterShelfEC, refreshAccessoryEC, exitShopEC;

    [SerializeField]
    private InputAction press;

    [SerializeField]
    private GameObject exclamationMark;

    private List<(Accessory, List<AccData>)> inventory;
    private AccessorySlot[] slots;
    private int numSlotsPerPage;
    private int currentPage;
    private int startIndex;
    private Vector2 screenPos;

    private AccessoryManager playerAccManager;

    public AccessorySlot[] Slots => slots;
    public List<(Accessory, List<AccData>)> currentAccData => inventory.GetRange(startIndex, Mathf.Min(inventory.Count - startIndex, numSlotsPerPage));

    private void Awake()
    {
        slots = GetComponentsInChildren<AccessorySlot>();
        numSlotsPerPage = slots.Length;
        inventory = gameAccManager.AccessoriesOnShelf;

        refreshAccessoryEC.Subscribe(OnRefreshPlayerAccessory);
        exitShopEC.Subscribe(CheckNotice);
    }

    private void OnDestroy()
    {
        refreshAccessoryEC.Unsubscribe(OnRefreshPlayerAccessory);
        exitShopEC.Unsubscribe(CheckNotice);
    }

    private void OnEnable()
    {
        Show(0);
        playerTransformChannel.Subscribe(SetPlayerAccManager);
        press.Enable();
        press.performed += OnScreenPressed;
        gameAccManager.DataChanged += OnDataChanged;
        CheckNotice();
        enterShelfEC.Subscribe(DisableExMark);
    }

    private void OnDisable()
    {
        playerTransformChannel.Unsubscribe(SetPlayerAccManager);
        press.Disable();
        press.performed -= OnScreenPressed;
        gameAccManager.DataChanged -= OnDataChanged;
        enterShelfEC.Unsubscribe(DisableExMark);
    }

    public bool Show(int page = 0)
    {
        int newStartIndex = page * numSlotsPerPage;

        if (page < 0 || inventory.Count <= newStartIndex)
        {
            return false;
        }

        startIndex = newStartIndex;
        currentPage = page;
        ClearCurrentPage();

        for (int i = 0; i < numSlotsPerPage; i++)
        {
            int currentIndex = startIndex + i;

            if (currentIndex >= inventory.Count)
            {
                return true;

            }

            if (ReferenceEquals(inventory[currentIndex].Item1, null))
            {
                continue;
            }

            slots[i].EquipAccessory(inventory[currentIndex].Item2[0]);
        }

        return true;
    }

    public bool ShowLeft()
    {
        return Show(currentPage - 1);
    }

    public bool ShowRight()
    {
        return Show(currentPage + 1);
    }

    private void ClearCurrentPage()
    {
        for (int i = 0; i < numSlotsPerPage; i++)
        {
            slots[i].UnequipAccessory();
        }
    }

  
    public bool TrySelectSlot(AccessorySlot slot)
    {
        // get slot's index

        for (int i = 0; i < slots.Length; i++)
        {
            if (ReferenceEquals(slots[i], slot))
            {
                if (startIndex + i >= inventory.Count || ReferenceEquals(inventory[startIndex + i].Item1, null))
                {
                    return false;
                }

                List<AccData> accDatas = inventory[startIndex + i].Item2;
                accCollectionScreen.ShowAccessoryCollectionScreen(accDatas);
                return true;
            }
        }

        return false;
    }

    private void OnScreenPressed(InputAction.CallbackContext context)
    {

        // ���⼭ stat ī�带 �����ϰ�
        if (!accInventoryScreen.IsVisible())
        {
            return;
        }

        screenPos = Mouse.current.position.ReadValue();

        if (screenPos == Vector2.zero)
        {
            screenPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (!TryGetSlot(out AccessorySlot slot, screenPos) || !slot.IsFull())
        {
            return;
        }

    }

    private bool TryGetSlot(out AccessorySlot slot, Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit hit;
        slot = null;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMaskCash.AccessorySlot , QueryTriggerInteraction.Collide))
        {
            return hit.transform.TryGetComponent(out slot);
        }

        return false;
    }

    private void SetPlayerAccManager(Transform playerTransform)
    {
        if (ReferenceEquals(playerTransform, null))
        {
            return;
        }

        playerAccManager = playerTransform.GetComponent<AccessoryManager>();
    }

    private void OnDataChanged()
    {
        // refresh shelf
        Show();
    }

    private void DisableExMark()
    {
        if (saveLoadManager.SaveFile.ShelfExclamationMark)
        {
            exclamationMark.SetActive(false);
            saveLoadManager.SaveFile.ShelfExclamationMark = false;
            saveLoadManager.Save();
        }
    }

    private void OnRefreshPlayerAccessory()
    {
        List<AccData> accDatas = playerAccManager.UnequipAll();
        AddAccessoriesToInventory(accDatas);
        gameAccManager.WriteSaveFile();
        Show(currentPage);
    }

    private void AddAccessoriesToInventory(List<AccData> accDatas)
    {
        foreach (var data in accDatas)
        {
            if (ReferenceEquals(data, null))
            {
                continue;
            }

            gameAccManager.Unequip(data, characterManager.GetCurrentCharIndex());
        }
    }

    private bool TryGetEmptyInventoryIndex(out int index)
    {
        index = -1;

        for (int i = 0; i < inventory.Count; i++)
        {
            if (ReferenceEquals(inventory[i], null))
            {
                index = i;
                return true;
            }
        }

        return false;
    }


    private void CheckNotice()
    {
        if (saveLoadManager.SaveFile.ShelfExclamationMark)
        {
            exclamationMark.SetActive(true);
        }
    }
}