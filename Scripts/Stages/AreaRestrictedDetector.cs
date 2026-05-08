using System.Collections;
using UnityEngine;

namespace Assets.Local.Scripts.Stages
{
    public class AreaRestrictedDetector : MonoBehaviour
    {

        private string tagName = "Area-restricted";

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(tagName) && other.TryGetComponent(out Damagable damagable))
            {
                damagable.Kill();
            }
        }
    }
}