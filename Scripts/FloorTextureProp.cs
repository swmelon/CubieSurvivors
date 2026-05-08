using UnityEngine;


[RequireComponent(typeof(ScaleRandomizer))]
public class FloorTextureProp : Prop
{
    [SerializeField]
    private Vector3 scaleOnEdge;

    private ScaleRandomizer scaleRandomizer;
    public void SetOnEdge()
    {
        scaleRandomizer = GetComponent<ScaleRandomizer>();
        scaleRandomizer.enabled = false;
        transform.localScale = scaleOnEdge;
    }
}