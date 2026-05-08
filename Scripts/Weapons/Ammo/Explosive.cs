using System.Collections;
using UnityEngine;
using System;


public class Explosive : DamagingPoolable<Explosive>
{
    [Tooltip("prefab의 절대 스케일이 1이 아닐때 사용. Range를 1로 설정하면 크기가 1이 되는 상수")]
    [SerializeField]
    private float scaleConst = 1f;
    public void SetRange (float value)
    {
        if (Mathf.Abs(range - value) < Mathf.Epsilon)
        {
            return;
        }

        range = value;
        transform.localScale = range * scaleConst * Vector3.one;
    }


    [Tooltip("절대 스케일")] 
    [SerializeField] 
    private float initialRange = 1f;

    [SerializeField]
    private SFXTags sfxTag;


    private const int maxColliders = 32;
    private const float releaseDelaySeconds = 1f;

    protected float range;
    private Collider[] colliders;
    private WaitForSeconds releaseDelay = new WaitForSeconds(releaseDelaySeconds);


    private void Awake()
    {
        colliders = new Collider[maxColliders];
        SetRange(initialRange);
    }
    
    public void Explode()
    {
        Damage();
        PlaySound();
        StartCoroutine(ReleaseAfterDelay(Release));
    }

    public void Explode(Action action)
    {
        Damage();
        PlaySound();
        StartCoroutine(ReleaseAfterDelay(action));
    }

    private void Damage()
    {
        int numHits = Physics.OverlapSphereNonAlloc(transform.position, range, colliders, targetLayer,
           QueryTriggerInteraction.Collide);
        Debug.DrawRay(transform.position, Vector3.up * range, Color.blue, 1f);

        for (int i = 0; i < numHits; i++)
        {
            // if i need to check if the collider is null or not?
            if (colliders[i] != null && colliders[i].transform.TryGetComponent(out Damagable damagable))
            {
                OnHitDamagable(colliders[i], damagable);
            }
        }
    }

    private void PlaySound()
    {
        FMODAudioManager.instance.PlayOneShot(sfxTag, transform.position);
    }

    private IEnumerator ReleaseAfterDelay(Action afterDelay)
    {
        yield return releaseDelay;
        afterDelay?.Invoke();
    }

    protected virtual void OnHitDamagable(Collider collider, Damagable damagable)
    {
        Vector3 hitDirection = ComputeHitForce(collider.transform.position);

        damagable.Hit(weapon.ComputeFinalDamage(damage, out bool isCritical),
            hitDirection,  isCritical: isCritical);
    }

    public void SetRangeToInitialValue()
    {
        SetRange(initialRange);
    }

    private void ClearArray()
    {
        for (int i = 0; i < colliders.Length; i++)
        {
            if (ReferenceEquals(colliders[i], null))
            {
                break;
            }

            colliders[i] = null;
        }
    }
}
