using UnityEngine;
using UnityEngine.InputSystem;

public class AccessorySlot : MonoBehaviour
{
    [SerializeField]
    public AccessoryType accessoryType;

    [SerializeField]
    private float scaleFactor = 1f;

    [SerializeField]
    private Accessory equippedAccessory;


    private bool equipped = false;
    private AccData equippedAccData;

    public Accessory EquippedAccessory => equippedAccessory;
    public AccData EquippedAccData => equippedAccData;
   
    public bool IsFull()
    {
        return equipped;
    }

    public virtual Accessory EquipAccessory(AccData accData)
    {
        if (equipped)
        {
            Debug.LogError("Already equipped");
            return null;
        }

        Accessory accessory = accData.accessory;
        Accessory accessoryInstance = Instantiate(accessory.gameObject).GetComponent<Accessory>();
        // �̸� ����� ����
        accessoryInstance.gameObject.name = accessory.gameObject.name;

        accessoryInstance.gameObject.SetActive(true);
        Vector3 parentsRotation = transform.rotation.eulerAngles;
        Vector3 accsRotation = accessoryInstance.transform.rotation.eulerAngles;

        accessoryInstance.transform.rotation = Quaternion.Euler(accsRotation.x, parentsRotation.y, accsRotation.z - parentsRotation.z);
        accessoryInstance.transform.position = transform.position;
        accessoryInstance.transform.parent = transform;
        accessoryInstance.transform.localScale *= scaleFactor;

        accessoryInstance.gameObject.layer = gameObject.layer;
        // and also child

        for (int i = 0; i < accessoryInstance.transform.childCount; i++)
        {
            accessoryInstance.transform.GetChild(i).gameObject.layer = gameObject.layer;
        }

        equippedAccessory = accessoryInstance;
        equippedAccData = accData;
        equipped = true;
        return accessoryInstance;
    }

    public void Copy(AccessorySlot otherSlot)
    {
        if (ReferenceEquals(otherSlot.equippedAccessory, null))
        {
            UnequipAccessory();
            return;
        }
        Accessory copy = Instantiate(otherSlot.equippedAccessory.gameObject).GetComponent<Accessory>();
        copy.transform.parent = transform;
        copy.transform.localPosition = otherSlot.equippedAccessory.transform.localPosition;
        copy.transform.localRotation = otherSlot.equippedAccessory.transform.localRotation;
        equippedAccessory = copy;
        equippedAccData = otherSlot.equippedAccData;
    }

    public virtual AccData UnequipAccessory()
    {
        // Remove the accessory from the slot
        AccData accData = equippedAccData;

        if (ReferenceEquals(accData, null))
        {
            return null;
        }

        if (equipped)
        {
            Destroy(equippedAccessory.gameObject);
            equippedAccessory = null;
            equipped = false;
            return accData;
        }

        return null;
    }
}
