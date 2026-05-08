using UnityEngine;

public class TemporalPlatform : MonoBehaviour
{
    public static Vector3 Position = new Vector3(0, 200f, 0);

    private void Awake()
    {
        Position = transform.position;
    }
}