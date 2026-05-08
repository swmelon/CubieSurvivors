using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    private Vector3 pointA, pointB;
    private float duration, time;

    private void OnEnable()
    {
        time = duration;
    }

    private void Update()
    {
        if (time < duration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(pointA, pointB, time / duration);
        }
    }

    public void Move(Vector3 pointA, Vector3 pointB, float duration)
    {
        this.pointA = pointA;
        this.pointB = pointB;
        this.duration = duration;
        time = 0;
    }

    public void Move(Vector3 pointB, float duration)
    {
        this.pointA = transform.position;
        this.pointB = pointB;
        this.duration = duration;
        time = 0;
    }
}