
using TMPro;
using UnityEngine;

public class NumberTrackingUI: MonoBehaviour
{
    [SerializeField]
    private RectTransform iconRectTransform;

    [SerializeField]
    private TextMeshProUGUI textMeshProUGUI;

    [SerializeField]
    private AnimationCurveContainer animCurveContainer;

    private bool animate = false;
    private float time = 0f;
    private float period = 0.3f;
    private AnimationCurve scaleCurve;
    protected virtual void Awake()
    {
        scaleCurve = animCurveContainer.Heartbeat;
    }

    protected void SetNumber(int number)
    {
        textMeshProUGUI.text = number.ToString();
        animate = true;
        time = 0f;
    }

    private void Update()
    {
        if (animate)
        {
            time += Time.unscaledDeltaTime;
            if (time > period)
            {
                animate = false;
                time = period;
            }

            float scale = scaleCurve.Evaluate(time / period);
            iconRectTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
