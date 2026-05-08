using UnityEngine;


[CreateAssetMenu(fileName = "SymbolIconContainer", menuName = "ScriptableObjects/SymbolIconContainer", order = 1)]
public class SymbolIconContainer : ScriptableObject
{
    [SerializeField]
    private Sprite damage, range, rateOfFire, explosionDamage, 
        explosionRadius, numOfProjectiles, reloadSpeed, scale, health, moveSpeed, magnet, sword;
    [SerializeField]
    private Sprite totalDamage;

    public Sprite Damage => damage;
    public Sprite TotalDamage => totalDamage;
    public Sprite Range => range;
    public Sprite RateOfFire => rateOfFire;
    public Sprite ExplosionDamage => explosionDamage;
    public Sprite ExplosionRadius => explosionRadius;
    public Sprite Plus => numOfProjectiles;
    public Sprite Refresh => reloadSpeed;
    public Sprite Scale => scale;
    public Sprite Health => health;
    public Sprite MoveSpeed => moveSpeed;
    public Sprite Magenet => magnet;
    public Sprite Sword => sword;


}
