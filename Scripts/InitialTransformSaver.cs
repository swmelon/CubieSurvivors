using UnityEngine;

public class InitialTransformSaver : MonoBehaviour
{
    private Quaternion initialRotation;
    private Vector3 initialPosition;

    private void Awake()
    {
        initialRotation = transform.localRotation;
        initialPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        transform.localRotation = initialRotation;
        transform.localPosition = initialPosition;
    }
}