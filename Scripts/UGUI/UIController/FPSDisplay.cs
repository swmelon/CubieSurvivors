using UnityEngine;
using TMPro; // If you're using TextMeshPro

public class FPSDisplay : MonoBehaviour
{
    public TMP_Text fpsText; // If using regular UI Text: public Text fpsText;
    public float updateInterval = 0.5f;

    private float deltaTime = 0.0f;
    private float frames = 0;

    void Update()
    {
        deltaTime += Time.unscaledDeltaTime;
        frames++;

        if (deltaTime > updateInterval)
        {
            float fps = frames / deltaTime;
            fpsText.text = string.Format("FPS: {0}", Mathf.RoundToInt(fps));

            deltaTime = 0;
            frames = 0;
        }
    }
}