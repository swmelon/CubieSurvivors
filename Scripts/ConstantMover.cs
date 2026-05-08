using UnityEngine;

public class ConstantMover : MonoBehaviour
{
    [Tooltip("Movement speed.")]
    public float speed = 1;

    public Vector3 moveDirection;
    public XYZConstantMover.Axis boundAxis;

    public float boundMin, boundMax;
    private Vector3 normalizedMoveDirection;
    private void Start()
    {
        normalizedMoveDirection = (transform.rotation * moveDirection).normalized;
    }
    private void Update()
    {

        Vector3 pos = transform.position;
        pos += speed * Time.deltaTime * normalizedMoveDirection;
        transform.position = pos;

        switch (boundAxis)
        {
            case XYZConstantMover.Axis.X:
                if (pos.x > boundMax || pos.x < boundMin)
                {
                    bool outPositive = pos.x > boundMax;

                    if (outPositive)
                    {
                        transform.position = pos - (pos.x - boundMin) * normalizedMoveDirection;
                    }
                    else
                    {
                        transform.position = pos - (boundMax - pos.x) * normalizedMoveDirection;
                    }
                }
                break;
            case XYZConstantMover.Axis.Y:
                if (pos.y > boundMax || pos.y < boundMin)
                {
                    bool outPositive = pos.y > boundMax;

                    if (outPositive)
                    {
                        transform.position = pos - (pos.y - boundMin) * normalizedMoveDirection;
                    }
                    else
                    {
                        transform.position = pos - (boundMax - pos.y) * normalizedMoveDirection;
                    }
                }
                break;
            case XYZConstantMover.Axis.Z:
                if (pos.z > boundMax || pos.z < boundMin)
                {
                    bool outPositive = pos.z > boundMax;

                    if (outPositive)
                    {
                        transform.position = pos - (pos.z - boundMin) * normalizedMoveDirection;
                    }
                    else
                    {
                        transform.position = pos - (boundMax - pos.z) * normalizedMoveDirection;
                    }
                }
                break;
        }
    }
}