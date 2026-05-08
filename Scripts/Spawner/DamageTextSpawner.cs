using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "DamageTextSpawner", menuName = "ScriptableObjects/Spawner/DamageTextSpawner",
    order = SOAssetMenuIndex.Spawner)]
public class DamageTextSpawner : Spawner<DamageText>, IDependentInitialization
{
    [FormerlySerializedAs("worldCanvas")] [SerializeField]
    private CanvasChannelSO worldCanvasChannel;
    
    [FormerlySerializedAs("mainMainCamera")] [FormerlySerializedAs("mainCamera")] [SerializeField]
    private MainCameraChannelSO mainCameraChannel;
    
    private Camera mainCamera;
    private Canvas worldCanvas;
    
    public new void Initialize()
    {
        mainCameraChannel.Subscribe(SetMainCamera);
        worldCanvasChannel.Subscribe(SetWorldCanvas);
        base.Initialize();
    }
    
    private void SetMainCamera(CameraController cameraController)
    {
        if (ReferenceEquals(cameraController, null))
        {
            mainCamera = null;
            return;
        }
        
        mainCamera = cameraController.Camera;
    }
    
    private void SetWorldCanvas(Canvas canvas)
    {
        worldCanvas = canvas;
    }
    
    public override DamageText Spawn()
    {
        DamageText damageText = base.Spawn();
        damageText.transform.SetParent(worldCanvas.transform);
        damageText.SetRotation(mainCamera);
        return damageText;
    }
}
