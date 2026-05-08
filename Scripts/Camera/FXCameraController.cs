
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;


[RequireComponent(typeof(Camera))]
public class FXCameraController : MonoBehaviour
{
    [SerializeField] private FXCameraChannelSO fxCameraChannel;
    [SerializeField] private GameObject fx;
    [SerializeField] private bool multipleFx = false;
    [SerializeField] private GameObject[] fxs;
    [SerializeField] private bool manualLooping = false;
    private Camera fxCamera;
    private WaitForSeconds wait = new WaitForSeconds(1);
    private Task loopFx;
    private float time = 0;
    private bool fxRendering = false;
    private RenderTexture renderTexture;
    public RenderTexture RenderTexture => renderTexture;
    private void Awake()
    {
        fxCamera = GetComponent<Camera>();
        renderTexture = fxCamera.targetTexture;

        if (!ReferenceEquals(fxCameraChannel, null))
        {
            fxCameraChannel.Register(this);
        }
    }
    
    public void TurnOnFx()
    {
        fx?.SetActive(true);
        fxCamera.enabled = true;
        fxRendering = true;

        if (multipleFx)
        {
            for (int i = 0; i < fxs.Length; i++)
            {
                fxs[i]?.SetActive(true);
            }
        }

        // 게임이 일시정지일때, 작동시킬 수 있는 방법 아마 await을 사용하면 될 것 같다
    }

    public void TurnOffFx()
    {
        fx?.SetActive(false);
        fxCamera.enabled = false;
        fxRendering = false;

        if (multipleFx)
        {
            for (int i = 0; i < fxs.Length; i++)
            {
                fxs[i]?.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (!fxRendering || !manualLooping)
        {
            return;
        }

        time += Time.unscaledDeltaTime;

        if (time >= 1)
        {
            time = 0;
            fx.gameObject.SetActive(false);
            fx.gameObject.SetActive(true);

            if (multipleFx)
            {
                for (int i = 0; i < fxs.Length; i++)
                {
                    fxs[i].SetActive(false);
                    fxs[i].SetActive(true);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (!ReferenceEquals(fxCameraChannel, null))
        {
            fxCameraChannel.Unregister(this);
        }
    }
}
