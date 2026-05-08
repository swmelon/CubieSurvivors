using Local.Scripts.Extensions;
using UnityEngine;
using System;

public class TrajectoryRenderer : MonoBehaviour
{
    public Action<(Vector3, Vector3)> RayHit;
    public LineRenderer lineRenderer;
    public Transform startPointObjectTransform;
    public int resolution = 30;
    public float predictionTime = 2f;

    private Vector3 lastInitialVelocity;
    private Vector3[] trajectoryPoints;

    private LayerMask targetLayer;

    private void Awake()
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        trajectoryPoints = new Vector3[resolution];

        targetLayer = LayerMaskCash.Obstacle;
    }

    private void OnEnable()
    {
        if (lineRenderer)
        {
            lineRenderer.enabled = true;
        }
    }

    private void OnDisable()
    {
        if (lineRenderer)
        {
            lineRenderer.enabled = false;
        }
    }

    public void UpdateTrajectory(Vector3 initialVelocity)
    {
        lineRenderer.positionCount = resolution;
        Vector3 currentPosition = startPointObjectTransform.position;
        Vector3 currentVelocity = initialVelocity;

        for (int i = 0; i < resolution; i++)
        {
           
            float simulationTime = i / (float)resolution * predictionTime;
            Vector3 nextPosition = CalculatePositionAtTime(initialVelocity, simulationTime);

            // Perform the raycast to detect objects in the targetLayer
            if (Physics.Raycast(currentPosition, nextPosition - currentPosition, out RaycastHit hit, (nextPosition - currentPosition).magnitude, targetLayer))
            {
                // If hit, resize the trajectory points array to the current number of points
                lineRenderer.positionCount = i + 1;
                trajectoryPoints[i] = hit.point;
                RayHit?.Invoke((hit.point, hit.normal));
                break; // Stop the loop as we've hit an object
            }
            else
            {
                trajectoryPoints[i] = nextPosition;
            }

            // Update the current position for the next iteration
            currentPosition = nextPosition;
            currentVelocity.y += Physics.gravity.y * simulationTime;
            
        }

        lineRenderer.SetPositions(trajectoryPoints);
    }

    public bool TryCalcFallPoint(Vector3 initialVelocity, out Vector3 fallPoint, out Vector3 normal)
    {
        Vector3 currentPos = startPointObjectTransform.position;
        Vector3 currentVelocity = initialVelocity;

        for (int i = 0; i < resolution; i++)
        {
            float simulationTime = i / (float)resolution * predictionTime;
            Vector3 nextPosition = CalculatePositionAtTime(initialVelocity, simulationTime);

            if (Physics.Raycast(currentPos, nextPosition - currentPos, out RaycastHit hit, (nextPosition - currentPos).magnitude, targetLayer))
            {
                fallPoint = hit.point;
                normal = hit.normal;
                return true;
            }

            currentPos = nextPosition;
            currentVelocity.y += Physics.gravity.y * simulationTime;
        }

        fallPoint = Vector3.zero;
        normal = Vector3.zero;
        return false;
    }

    private Vector3 CalculatePositionAtTime(Vector3 initialVelocity, float time)
    {
        Vector3 gravity = Physics.gravity;
        Vector3 position = startPointObjectTransform.position + initialVelocity * time + 0.5f * gravity * time * time;
        return position;
    }
}
