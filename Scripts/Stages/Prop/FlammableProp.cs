using UnityEngine;
using System.Collections;


[RequireComponent(typeof(DamagableNoText))]
[RequireComponent(typeof(DamageOverTimeTrigger))]
public class FlammableProp : MonoBehaviour
{
    [SerializeField]
    private bool shrinkYScaleOnBurn = false;

    [SerializeField]
    private bool recoverAfterBurn = true;

    [SerializeField]
    private float burnTime = 10f;

    [SerializeField]
    private GameObject flame;  // Assumes this is a child GameObject for visual flame effects


    private static float minYScale = 0.01f;
    private DamageOverTimeTrigger damageOverTimeTrigger;
    private DamagableNoText damagable;
    private Vector3 originalScale;

    private enum FlammableState
    {
        Unlit,
        Lit,
        Burnt
    }

    private FlammableState state = FlammableState.Unlit;

    private void Awake()
    {
        state = FlammableState.Unlit;
        flame.SetActive(false);  // Ensure the flame effect is disabled initially
        damageOverTimeTrigger = GetComponent<DamageOverTimeTrigger>();
        damagable = GetComponent<DamagableNoText>();
    }

    public void StartBurn()
    {
        if (state == FlammableState.Unlit)  // Only start burning if not already lit or burnt
        {
            state = FlammableState.Lit;
            flame.SetActive(true);  // Activate the flame effect
            StartCoroutine(Burn());  // Start the burn coroutine
            damageOverTimeTrigger.StartDamage();
        }
    }

    private IEnumerator Burn()
    {
        gameObject.tag = "Burning";
        // If shrinkYScaleOnBurn is true, gradually shrink the Y scale over the burnTime
        if (shrinkYScaleOnBurn)
        {
            float elapsedTime = 0;
            originalScale = transform.localScale;
            while (elapsedTime < burnTime)
            {
                elapsedTime += Time.deltaTime;
                float newYScale = Mathf.Lerp(originalScale.y, minYScale, elapsedTime / burnTime);
                transform.localScale = new Vector3(originalScale.x, newYScale, originalScale.z);
                yield return null;
            }
            transform.localScale = new Vector3(originalScale.x, minYScale, originalScale.z);
        }
        else
        {
            yield return new WaitForSeconds(burnTime);
        }

        state = FlammableState.Burnt;
        flame.SetActive(false);  // Disable the flame effect after the object is burnt
        damageOverTimeTrigger.StopDamage();

        gameObject.tag = "Untagged";

        if (recoverAfterBurn)
        {
            StartCoroutine(Recover());
        }
    }

    private IEnumerator Recover()
    {
        if (shrinkYScaleOnBurn && recoverAfterBurn)
        {
            float elapsedTime = 0;
            while (elapsedTime < burnTime)
            {
                elapsedTime += Time.deltaTime;
                float newYScale = Mathf.Lerp(minYScale, originalScale.y, elapsedTime / burnTime);
                transform.localScale = new Vector3(originalScale.x, newYScale, originalScale.z);
                yield return null;
            }
            transform.localScale = originalScale;
        }
      
        state = FlammableState.Unlit;
        damagable.Revive();
    }

}