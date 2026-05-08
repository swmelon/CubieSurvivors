using Local.Scripts.Extensions;
using UnityEngine;

public class ScaleRandomizer : MonoBehaviour
{
    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);
    

    private void Start()
    {
        transform.localScale = transform.localScale * RandomExtenstion.GetFloatInRange(scaleRange.x, scaleRange.y); 
    }
}