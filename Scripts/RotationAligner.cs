using System.Collections;
using UnityEngine;

public class RotationAligner : MonoBehaviour
{
    private Quaternion initialRotation;
    // Use this for initialization
    private void Awake()
    {
        initialRotation = transform.localRotation;
    }

    private void OnDisable()
    {
        transform.localRotation = initialRotation;
    }
}