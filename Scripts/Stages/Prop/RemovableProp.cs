using System;
using UnityEngine;

public class RemovableProp : MonoBehaviour
{
    public event Action OnRemove;

    [SerializeField] 
    private float animTime = 1;

    [SerializeField]
    private OnePureEffectSpawner effectSpawner;

    private bool remove = false;
    private float time = 0;
    private float veolcity = 0;

    private void OnEnable()
    {
        remove = false;
        time = 0;
        veolcity = 0;
    }
    public bool Remove()
    {
        if (remove)
        {
            return false;
        }
        remove = true;
        effectSpawner.Spawn().transform.position = transform.position;
        OnRemove?.Invoke();
        return true;
    }

    private void Update()
    {
        if (remove)
        {
            time += Time.deltaTime;

            if (time >= animTime)
            {
                Destroy(gameObject);
            }

            veolcity += Physics.gravity.y * Time.deltaTime;
            transform.position += Vector3.up * veolcity * Time.deltaTime + UnityEngine.Random.insideUnitSphere * 0.05f;

            // add random noise to position (shaking effect)
        }
    }
}