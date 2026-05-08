using UnityEngine;

public interface ICharacterController
{
    public Vector3 GetDirection();
    public float GetSpeed();
    public float GetMaxSpeed();

    public Component component { get; }
}
