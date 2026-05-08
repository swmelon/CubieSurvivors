using UnityEngine;

public class Stabilizer : MonoBehaviour
{
    public Transform target; // The transform of the mortar barrel
    public float desiredAngle = 45f; // Desired shoot angle in degrees
    public Transform stabilizationPoint; // A point in world space that the mortar aims at to maintain the angle

    void LateUpdate()
    {
        target.rotation = Quaternion.Euler(desiredAngle, target.eulerAngles.y, 0f);
    }

    private void Stabilize()
    {
        if (stabilizationPoint == null)
        {
            // If there's no explicit stabilization point, use a point in the air at the desired angle
            Vector3 worldPoint = target.position + (Quaternion.Euler(-desiredAngle, 0f, 0f) * Vector3.forward);
            stabilizationPoint.position = worldPoint;
        }

        // Calculate the direction from the mortar to the stabilization point
        Vector3 directionToStabilizationPoint = stabilizationPoint.position - target.position;

        // Calculate the rotation needed to look at the stabilization point
        Quaternion lookRotation = Quaternion.LookRotation(directionToStabilizationPoint);

        // Apply the rotation to the mortar barrel while maintaining its current yaw
        target.rotation = Quaternion.Euler(lookRotation.eulerAngles.x, target.eulerAngles.y, target.eulerAngles.z);
    }
}
