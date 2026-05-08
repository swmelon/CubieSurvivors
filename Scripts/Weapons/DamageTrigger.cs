using UnityEngine;


public class DamageTrigger : MonoBehaviour
{
    [SerializeField] 
    [Range(10, 200)]
    protected int damage;
    
    [SerializeField] 
    protected string targetTag;
    
    public int Damage
    {
        set => damage = value;
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag) && other.TryGetComponent<Damagable>(out Damagable damagable))
        {
            damagable.Hit(damage);
        }
    }
}
