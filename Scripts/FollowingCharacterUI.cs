using UnityEngine;

public class FollowingCharacterUI : MonoBehaviour
{
    [SerializeField]
    private MainCameraChannelSO mainCameraChannel;

    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    [SerializeField]
    private RectTransform[] rectTransforms;

    [SerializeField]
    private float heightOffset, depth;

    
    [SerializeField]
    private int[] Yoffsets;

    private Transform player;
    private CameraController mainCameraController;

    private void OnEnable()
    {
        playerTransformChannel.Subscribe(SetPlayer);
        mainCameraChannel.Subscribe(SetMainCamera);
    }
    
    private void OnDisable()
    {
        playerTransformChannel.Subscribe(SetPlayer);
        mainCameraChannel.Subscribe(SetMainCamera);
    }

    private void SetPlayer(Transform player)
    {
        this.player = player;
    }

    private void SetMainCamera(CameraController mainCameraController)
    {
        this.mainCameraController = mainCameraController;
    }

    public void UpdateUIPos()
    {
        if (ReferenceEquals(player, null) || ReferenceEquals(mainCameraController, null))
        {
            return;
        }

        Vector3 playerPos = player.position;
        playerPos.y += heightOffset;
        Vector3 direction = (playerPos - mainCameraController.transform.position).normalized;
        Vector3 newPos = mainCameraController.transform.position + direction * depth;
        transform.position = newPos;
    }
}
