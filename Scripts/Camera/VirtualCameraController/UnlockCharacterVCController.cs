using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class UnlockCharacterVCController : MonoBehaviour
{
    [SerializeField]
    private PlayerChannelSO playerChannel;

    [SerializeField]
    private AnimationCurve orbitingCurve;

    private CinemachineVirtualCamera virtualCamera;

    public float startAngle = 60f; // Starting angle
    public float endAngle = 240f; // Ending angle
    public float duration = 4f; // Time to rotate in seconds
    public float orthoSize = 5f; // Orthographic size

    private CinemachineOrbitalTransposer transposer;
    private float elapsedTime = 0f;
    private Player player;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        transposer = virtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();
    }

    private void OnEnable()
    {
        playerChannel.Subscribe(SetPlayer);
        virtualCamera.Follow = player.transform;
        virtualCamera.LookAt = player.transform;
        transposer.m_XAxis.Value = startAngle;
        virtualCamera.m_Lens.OrthographicSize = orthoSize;
        elapsedTime = 0f;
    }

    private void OnDisable()
    {
        playerChannel.Unsubscribe(SetPlayer);

    }

    private void Update()
    {
        if (transposer == null) return;

        // Increment the elapsed time
        elapsedTime += Time.deltaTime;

        if (elapsedTime <= duration)
        {
            // Calculate the current angle based on interpolation
            float newAngle = Mathf.Lerp(startAngle, endAngle, orbitingCurve.Evaluate(elapsedTime / duration));
            transposer.m_XAxis.Value = newAngle; // Set the current angle
        }
    }

    private void SetPlayer(Player currentPlayer)
    {
        player = currentPlayer;
    }
}