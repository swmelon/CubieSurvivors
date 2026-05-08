using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownObject : MonoBehaviour
{
    private float speed = 0f;
    private Vector3 direction = Vector3.zero;
    private LayerMask layerMask;
    private int damage = 0,  damageCount = 1;
    
    // max length of list is about 10, so i will use List instead of HashSet.
    private List<Damagable> hitEnemies = new List<Damagable>();

    public void SetSpeedAndDirection(float speed, Vector3 direction)
    {
        this.speed = speed;
        this.direction = direction;
        layerMask = LayerMask.GetMask("Enemy");
    }

    private void Update()
    {
        transform.position += speed * Time.deltaTime * direction;
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 0.5f, layerMask)
            && hit.collider.transform.root.TryGetComponent(out Damagable damagable)
            && !hitEnemies.Contains(damagable))
        {
            damagable.Hit(damage);
            hitEnemies.Add(damagable);
            damageCount--;
            if (damageCount == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
