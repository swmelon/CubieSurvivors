
using UnityEngine;

public class SendTriggerMessageToParent : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Call the OnTriggerEnter method of the parent script
        transform.parent.SendMessage("OnTriggerEnter", other);
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Call the OnTriggerEnter method of the parent script
        transform.parent.SendMessage("OnTriggerExit", other);
    }
}
