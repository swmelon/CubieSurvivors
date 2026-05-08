using UnityEngine;

public class XYZConstantMover : MonoBehaviour
{
    public enum Axis
    {
        X,
        Y,
        Z
    }

    public Axis moveAxis;
    public bool movePositive;

    [Range(0f, 10f)]
    public float speed;


    public float boundMin, boundMax;

    private void Update()
    {
        Vector3 pos = transform.position;
        switch (moveAxis)
        {
            case Axis.X:
                pos.x += movePositive ? speed * Time.deltaTime : -speed * Time.deltaTime;

                if (pos.x > boundMax || pos.x < boundMin)
                {
                    pos.x = pos.x > boundMax ? boundMin : boundMax;
                }
                break;
            case Axis.Y:
                pos.y += movePositive ? speed * Time.deltaTime : -speed * Time.deltaTime;

                if (pos.y > boundMax || pos.y < boundMin)
                {
                    pos.y = pos.y > boundMax ? boundMin : boundMax;
                }
                break;

            case Axis.Z:
                pos.z += movePositive ? speed * Time.deltaTime : -speed * Time.deltaTime;

                if (pos.z > boundMax || pos.z < boundMin)
                {
                    pos.z = pos.z > boundMax ? boundMin : boundMax;
                }
                break;
        }

        transform.position = pos;
    }
}