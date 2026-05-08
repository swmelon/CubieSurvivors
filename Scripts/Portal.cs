
using Local.Scripts.Extensions;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    private Vector3 offset;

    [SerializeField]
    private EventChannelSO portalActivatedEventChannel;
    
    private bool activated = false;

    private void OnEnable()
    {
        activated = false;
    }
    private void FixedUpdate()
    {
        if (activated)
        {
            return;
        }
        
        RaycastHit hit;
        
        Debug.DrawRay(transform.position + offset, Vector3.up, Color.red);
        if (Physics.Raycast(transform.position + offset, Vector3.up, out hit, 1f, LayerMaskCash.Player))
        {
            portalActivatedEventChannel.Raise();
            activated = true;
        }
    }
}
