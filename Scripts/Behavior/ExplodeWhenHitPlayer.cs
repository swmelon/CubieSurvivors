using UnityEngine;

public class ExplodeWhenHitPlayer : MonoBehaviour
{
    [SerializeField] 
    [Range(10, 100)]
    private int damage;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out DamagablePlayer damagable))
        {
            damagable.Hit(damage);
            print(name);
        }
    }
}

