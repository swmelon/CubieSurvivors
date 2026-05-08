using System.Security.Cryptography;
using UnityEngine;

public class QuickFireItem : Item
{
    [SerializeField] 
    private float buffTime;
    public override void Activate(Player player)
    {
        player.WeaponManager.StartQuickFire(buffTime);
        base.Activate(player);
    }
}
