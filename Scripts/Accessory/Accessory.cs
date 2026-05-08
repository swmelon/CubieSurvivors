using UnityEngine;

public class Accessory : MonoBehaviour, IIconized, IItemizable
{
 
    public int Price => price;


    public string accessoryName;

    [SerializeField]
    private AccessoryType accessoryType;

    [SerializeField]
    private int accessoryID = -1;

    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private Vector3 shelfOffset;

    [SerializeField]
    private AccessoryRank rank;

    [SerializeField]
    private int price = 0;

    [SerializeField]
    private Sprite icon;

    [SerializeField]
    private Vector3 scaleOnEquipped = Vector3.one;

    public AccessoryType AccessoryType => accessoryType;
    public AccessoryRank Rank => rank;
    public int ID => accessoryID;
    public Sprite GetIcon() => icon;
    public AccStats Stats { get; set; }

    public void BeItem()
    {
        transform.position -= 0.05f * transform.lossyScale.x * Vector3.up;
    }

    public void SetID(int id)
    {
        accessoryID = id;
    }

    public void OnEquipped()
    {
        transform.localScale = scaleOnEquipped;
        transform.localPosition = offset;
    }

    public void OnUnequipped()
    {
        transform.localScale = Vector3.one;
    }

    public void OnShelf()
    {
        transform.localPosition = shelfOffset;
    }
}