using Local.Scripts.Extensions;
using UnityEngine;

public class RotationRandomizer : MonoBehaviour
{
    private void Start()
    {
        transform.localEulerAngles = new Vector3(0, RandomExtenstion.GetFloatInRange(0, 360f) , 0); 
    }
}