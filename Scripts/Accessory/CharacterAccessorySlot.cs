

public class CharacterAccessorySlot : AccessorySlot
{
    public override Accessory EquipAccessory(AccData accData)
    {
        Accessory accInstance = base.EquipAccessory(accData);
        accInstance.OnEquipped();
        return accInstance;
    }

}
