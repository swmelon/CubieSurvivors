using UnityEngine;

public class HitPlayer : MonoBehaviour
{
    [SerializeField] 
    [Range(10, 100)]
    private int damage;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.gameObject.TryGetComponent(out DamagableAlly damagable))
        {
            damagable.Hit(damage);
            print(name);
        }
    }
}

