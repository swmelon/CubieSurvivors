
using UnityEngine;


[RequireComponent(typeof(Canvas))]
public class WorldCanvasController : MonoBehaviour
{
    [SerializeField]
    private CanvasChannelSO worldCanvasChannel;
    
    private Canvas canvas;
    
    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        worldCanvasChannel.Register(canvas);
    }
}
