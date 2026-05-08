using System;
using System.Collections;
using Local.Scripts.Extensions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class Napalm : DamagingPoolable<Napalm>
{
    private const float minScaleY = 0.2f;

    [SerializeField]
    private float spreadSpeed;
    
    private Vector3 direction;
    private bool isCollided = false;
    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    protected override void OnEnable()
    {
        base.OnEnable();
        direction = Vector3.forward;
        float randomYRotation = RandomExtenstion.GetFloatInRange(0f, 360f);
        direction = Quaternion.Euler(0, randomYRotation, 0) * direction;
        transform.localScale = Vector3.one;
        isCollided = false;
        rb.useGravity = true;
    }

    private void Update()
    {
        if (transform.localScale.y <= minScaleY)
        {
            return;
        }
        if (isCollided)
        {
            transform.localScale += spreadSpeed * Time.deltaTime * new Vector3(1, -1, 1);
            
        }
        else
        {
            transform.position += Time.deltaTime * direction;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollided && other.CompareTag("Structure"))
        {
            isCollided = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        }
    }
}
