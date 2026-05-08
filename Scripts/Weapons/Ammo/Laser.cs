using System;
using UnityEngine;

public class Laser : Poolable<Laser>
{
    [SerializeField]
    private float speed = 10.0f;

    private void Update()
    {
        transform.position += Time.deltaTime * speed * transform.forward;
    }
}
