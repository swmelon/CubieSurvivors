using UnityEngine;
using UnityEngine.UI;

public class FrameCapture : MonoBehaviour
{
    public BooleanEventChannelSO GamePausedEC;
    public Canvas snapshotCanvas;
    public Image snapshotImage;
    private RenderTexture renderTexture;
    private Camera renderCamera;
    private Camera mainCamera;

    private void Awake()
    {
        renderCamera = GetComponent<Camera>();
        mainCamera = transform.parent.GetComponent<Camera>();
        GamePausedEC.Subscribe(OnGamePaused);
        renderCamera.enabled = false;
        snapshotCanvas.enabled = false;
    }

    private void OnDestroy()
    {
        GamePausedEC.Unsubscribe(OnGamePaused);
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    private void OnGamePaused(bool paused)
    {
        if (paused)
        {
            CaptureFrame();
        }
        else
        {
            ResetBeforePlay();
        }
    }

    private void CaptureFrame()
    {
        renderCamera.backgroundColor = mainCamera.backgroundColor;
        renderCamera.enabled = true;
        snapshotCanvas.enabled = true;
        mainCamera.enabled = false;

        if (renderTexture == null || renderTexture.width != Screen.width || renderTexture.height != Screen.height)
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }
            renderTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            renderTexture.Create();
        }

        renderCamera.targetTexture = renderTexture;
        snapshotImage.material.mainTexture = renderTexture;
        snapshotImage.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
        renderCamera.Render();
        renderCamera.targetTexture = null;
        renderCamera.enabled = false;
    }

    private void ResetBeforePlay()
    {
        mainCamera.enabled = true;
        renderCamera.enabled = false;
        snapshotCanvas.enabled = false;
    }
}
