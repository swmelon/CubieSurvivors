using UnityEngine;
using UnityEngine.Events;

public class PositionTracker : MonoBehaviour
{
    public UnityEvent onPositionChanged;

    private Vector3 lastPosition;

    void Awake()
    {
        lastPosition = transform.localPosition;
    }

    void Update()
    {
        if (transform.localPosition != lastPosition)
        {
            lastPosition = transform.localPosition;
        }
    }
}
