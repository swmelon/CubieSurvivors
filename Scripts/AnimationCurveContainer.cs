using UnityEngine;


[CreateAssetMenu(fileName = "AnimationCurveContainer", menuName = "ScriptableObjects/DataContainer/AnimationCurveContainer", order = 1)]
public class AnimationCurveContainer : ScriptableObject
{
    [SerializeField]
    private AnimationCurve heartbeat;

    [SerializeField]
    private AnimationCurve stopSmoothly;

    public AnimationCurve Heartbeat => heartbeat;
    public AnimationCurve StopSmoothly => stopSmoothly;
}