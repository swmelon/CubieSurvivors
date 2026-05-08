using Local.Scripts.Extensions;
using UnityEngine;


public class PropDestroyer : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if ( collision.collider.TryGetComponent(out RemovableProp prop))
        {
            prop.Remove();
        }
    }
}
