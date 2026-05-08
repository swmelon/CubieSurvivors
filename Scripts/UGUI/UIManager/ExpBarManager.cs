using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


public class ExpBarManager : MonoBehaviour
{
    [SerializeField]
    private CanvasChannelSO worldCanvasChannel;
    
    [FormerlySerializedAs("mainCameraChannel")] [SerializeField]
    private MainCameraChannelSO mainMainCameraChannel;
    
    [SerializeField]
    private GameObject expBarPrefab;

    private RectTransform expBarRectTransform;
    private Vector3 expBarOffset;
    private Camera mainCamera;
    private Canvas worldCanvas;
    private GameObject expBar;
    private Slider expBarSlider;
    
    
    private void OnEnable()
    {
        worldCanvasChannel.Subscribe(SetWorldCanvas);
        mainMainCameraChannel.Subscribe(SetMainCamera);
        
        if (HasExpBar())
        {
            expBar.SetActive(true);
        }
    }
    
    private void OnDisable()
    {
        worldCanvasChannel.Unsubscribe(SetWorldCanvas);
        mainMainCameraChannel.Unsubscribe(SetMainCamera);
        
        if (HasExpBar())
        {
            expBar.SetActive(false);
        }
    }
    
    private bool HasExpBar() => expBar != null;
    
    private void SetWorldCanvas(Canvas canvas)
    {
        worldCanvas = canvas;
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

    public void SetupEXPBar(float heightOffset, float healthBarHeight)
    {
        expBar = Instantiate(expBarPrefab, transform.position, Quaternion.identity);
        expBar.transform.SetParent(worldCanvas.transform);
        expBarRectTransform = expBar.GetComponent<RectTransform>();
        expBarRectTransform.rotation = mainCamera.transform.rotation;
        expBarOffset = new Vector3(0, heightOffset - 0.24f * (healthBarHeight + expBarRectTransform.rect.height), 0);

        expBarSlider = expBar.GetComponent<Slider>();
        GetComponent<ExpManager>().SetExpBar(expBarSlider);

        expBarSlider.value = 0f;
    }

    private void LateUpdate()
    {
        if (expBarRectTransform != null)
        {
            expBarRectTransform.position = transform.position + expBarOffset;
            expBarRectTransform.rotation = mainCamera.transform.rotation;
        }
    }
}
