using UnityEngine;

public class ShelfAccessorySlot : AccessorySlot
{
    private static readonly Vector3 glassessOffset = new Vector3(0, 0.7f, 0);

    public override Accessory EquipAccessory(AccData accData)
    {
        Accessory accInstance = base.EquipAccessory(accData);

        accInstance.OnShelf();

        if (accInstance.AccessoryType == AccessoryType.Glasses)
        {
            accInstance.transform.Rotate(0, 180f, 0);
            accInstance.transform.localPosition +=  glassessOffset;
        }

        return accInstance;
    }

}