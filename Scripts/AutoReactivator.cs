using UnityEngine;

public class AutoReactivator : MonoBehaviour
{
    [SerializeField]
    private GameObject target;

    [SerializeField]
    private float reactivationTime = 1f;

    private void OnEnable()
    {
        target.SetActive(true);
    }

    private void Update()
    {
        reactivationTime -= Time.deltaTime;

        if (reactivationTime <= 0)
        {
            target.SetActive(false);
            target.SetActive(true);
            reactivationTime = 1f;
        }        
    }
}
