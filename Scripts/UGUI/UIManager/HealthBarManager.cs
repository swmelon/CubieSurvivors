using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;


[RequireComponent(typeof(Damagable))]
public class HealthBarManager : MonoBehaviour
{
    public float HeightOffset
    {
        set
        {
            heightOffset = value;
            offset = new Vector3(0, heightOffset, 0);
        }
    }

    public float Height
    {
        get
        {
            if (ReferenceEquals(rectTransform, null))
            {
                return healthBarPrefab.GetComponent<RectTransform>().rect.height;
            }
            
            return rectTransform.rect.height;
        }
    }

    
    [FormerlySerializedAs("mainMainCameraChannel")] [SerializeField]
    private MainCameraChannelSO mainCameraChannel;
    
    [SerializeField]
    private CanvasChannelSO worldCanvasChannel;
    
    [SerializeField]
    private GameObject healthBarPrefab;

    [SerializeField]
    protected float heightOffset;

    private Damagable damagable;
    private GameObject healthBar;

    private Camera mainCamera;
    private Canvas worldCanvas;
    private RectTransform rectTransform;
    private Slider slider;
    private Vector3 offset;
    
    private void Start()
    {
        damagable = GetComponent<Damagable>();
        SetupHealthBar();
    }

    private void OnEnable()
    {
        if (HasHealthBar())
        {
            healthBar.SetActive(true);
        }
        
        mainCameraChannel.Subscribe(SetMainCamera);
        worldCanvasChannel.Subscribe(SetWorldCanvas);
        
    }
    
    private void OnDisable()
    {
        if (HasHealthBar())
        {
            healthBar.SetActive(false);
        }
        
        mainCameraChannel.Unsubscribe(SetMainCamera);
        worldCanvasChannel.Unsubscribe(SetWorldCanvas);
    }
    private bool HasHealthBar() => healthBar != null;
    
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

    private void OnDestroy()
    {
        Destroy(healthBar);
    }

    private void SetupHealthBar()
    {
        healthBar = Instantiate(healthBarPrefab, transform.position, Quaternion.identity);
        healthBar.transform.SetParent(worldCanvas.transform);
        
        rectTransform = healthBar.GetComponent<RectTransform>();
        rectTransform.rotation = mainCamera.transform.rotation;
        offset = new Vector3(0, heightOffset, 0);

        
        slider = healthBar.GetComponent<Slider>();
        damagable.OnHealthChange.AddListener((value) => { slider.value = value; });
        slider.value = 1f;
    }
    
    private void LateUpdate()
    {
        rectTransform.position = transform.position + offset;
        rectTransform.rotation = mainCamera.transform.rotation;
    }
}
