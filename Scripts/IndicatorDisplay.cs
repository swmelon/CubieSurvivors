using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class IndicatorDisplay : MonoBehaviour
{
    [SerializeField]
    private float flashDuration = 1.0f;

    [SerializeField]
    private int numFlash = 2;

    [SerializeField]
    private float flashAlpha = 0.5f;

    [SerializeField]
    private Color flashColor = Color.red;

    [SerializeField]
    private WorldDirectionChannelSO worldDirectionChannel;

    [SerializeField]
    private MainCameraChannelSO mainCameraChannel;

    [SerializeField]
    private Sprite[] oneDirectionIndicator;

    [SerializeField]
    private Sprite[] twoDirectionIndicator;

    private Image image;
    private bool flash = false;
    private float time = 0f;
    private int flashCount = 0;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.enabled = false;
    }

    public void SetIndicator(Sprite indicator)
    {
        WorldDirection worldDirection = worldDirectionChannel.WorldDirection;
        
        if(!mainCameraChannel.TryGetVariable(out CameraController cameraController) || cameraController.IsThirdPersonMode())
        {
            return;
        }

        int i = 0;
        bool found = false;

        for (i = 0; i < oneDirectionIndicator.Length; i++)
        {
            if (indicator == oneDirectionIndicator[i])
            {
                i = (i + (int)worldDirection * 2) % 8;
                found = true;
                indicator = oneDirectionIndicator[i];
                break;
            }
        }

        if (!found)
        {
            for (i = 0; i < twoDirectionIndicator.Length; i++)
            {
                if (indicator == twoDirectionIndicator[i])
                {
                    i = (i + (int)worldDirection) % 2;
                    found = true;
                    indicator = twoDirectionIndicator[i];
                    break;
                }
            }
        }

        image.sprite = indicator;
        flash = true;
        time = 0f;
        flashCount = 0;
        image.enabled = true;
    }

    private void Update()
    {
        if (!flash)
        {
            return;
        }


        time += Time.deltaTime;

        if (time > flashDuration)
        {
            time = 0f;
            flashCount++;

            if (flashCount >= numFlash)
            {
                flash = false;
                image.enabled = false;
                return;
            }
        }

        float alpha = Mathf.PingPong(2*time, flashDuration) / flashDuration * flashAlpha;
        image.color = new Color(flashColor.r, flashColor.g, flashColor.b, alpha);
    }
}