using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/Enemy/EnemyData", order = SOAssetMenuIndex.Enemy)]
public class EnemyData : PrefabDataSO<Enemy>, IColorable
{
    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }
    public float Weight
    {
        get => weight;
        set => weight = value;
    }
    public int MaxHealth
    {
        get => (int)(maxHealth * healtFactor);
        set => maxHealth = value;
    }
    public float Scale
    {
        get => scale;
        set => scale = value;
    }

    public Color Color
    {
        get => color;
        set => color = value;
    }

    public float Power
    {
        get => power * healtFactor;
    }
        
    public float AttackToChaseDelay => attackToChaseDelay;
    public Material RollerMaterial => rollerMaterial;
    
    [SerializeField]
    private float moveSpeed;

    [SerializeField] 
    private float weight;
    
    [SerializeField]
    private int maxHealth;
    
    [SerializeField]
    private float scale;
    
    [SerializeField]
    private float attackToChaseDelay = 0f;
    
    [SerializeField]
    private Material rollerMaterial;

    [SerializeField]
    private Color color;

    [SerializeField]
    private float power = 0f;

    public float healtFactor = 1f;

    public void InitializePower()
    {
        if (!Mathf.Approximately(power, 0f))
        {
            return;
        }

        power = maxHealth * moveSpeed;
    }

    public void SetColor(Color color)
    {
        rollerMaterial.color = color;
    }

    private void OnEnable()
    {
        if (color == new Color())
        {
            color = rollerMaterial.color;
        }
    }
}
