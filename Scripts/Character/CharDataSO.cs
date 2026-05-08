using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "New CharacterAbillity", menuName = "ScriptableObjects/CharacterAbillity", order = SOAssetMenuIndex.Character)]
public class CharDataSO : ScriptableObject
{
    public int CharIndex => charIndex;
    public string CharName => charName;
    public string Description => charDescription;
    public Sprite Icon => charIcon;
    public float Speed => speed;
    public float MaxHealth => maxHealth;
    public float Attack => attack;
    public float Defense => defense;

    [SerializeField]
    private int charIndex;
    
    [SerializeField] 
    private string charName;
    
    [SerializeField]
    private Sprite charIcon;
    
    [SerializeField]
    private GameObject charPrefab;
    
    [SerializeField] 
    private string charDescription;

    [Range(3f, 5f)]
    [SerializeField] 
    private float speed;

    [Range(100f, 200f)]
    [SerializeField] 
    private float maxHealth;
    
    [Range(-5f, 5f)]
    [SerializeField]
    private float attack, defense, luck; 
    
    [SerializeField]
    private DamageCalculatorSO damageCalculator;
    
    public GameObject Instantiate()
    {
        return Instantiate(charPrefab);
    }
}
