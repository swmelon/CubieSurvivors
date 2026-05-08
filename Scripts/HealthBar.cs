using Minimalist.Utility.SampleScene;
using UnityEngine;

/// <summary>
/// Displays a configurable health bar for any object with a Damageable as a parent
/// </summary>
public class HealthBar : MonoBehaviour
{

    MaterialPropertyBlock matBlock;
    MeshRenderer meshRenderer;
    Damagable damagable;

    [SerializeField]
    private MainCameraChannelSO mainCameraChannel;

    private CameraController mainCameraController;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        matBlock = new MaterialPropertyBlock();
        // get the damageable parent we're attached to
        damagable = GetComponentInParent<Damagable>();
        mainCameraChannel.Subscribe(SetMainCamera);
    }

    private void SetMainCamera(CameraController mainCameraController)
    {
        this.mainCameraController = mainCameraController;
    }

    private void OnDestroy()
    {
        mainCameraChannel.Unsubscribe(SetMainCamera);
    }

    private void OnEnable()
    {
        meshRenderer.enabled = true;
        AlignCamera();
    }

    private void Update()
    { 
        AlignCamera();
        UpdateParams();
    }
    
    private void UpdateParams()
    {
        int health = damagable.Health;

        if (health <= 0)
        {
            meshRenderer.enabled = false;
            return;
        }

        meshRenderer.GetPropertyBlock(matBlock);
        matBlock.SetFloat("_Fill", health / (float)damagable.MaxHealth);
        meshRenderer.SetPropertyBlock(matBlock);
    }

    private void AlignCamera()
    {
        if (!ReferenceEquals(mainCameraController, null))
        {
            var camXform = mainCameraController.transform;
            var forward = transform.position - camXform.position;
            forward.Normalize();
            var up = Vector3.Cross(forward, camXform.right);
            transform.rotation = Quaternion.LookRotation(forward, up);
        }
    }
}