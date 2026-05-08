using UnityEngine;
using UnityEngine.UIElements;

public class VisualElementAnimator : MonoBehaviour
{
    
    [SerializeField]
    private AnimationCurveContainer animCurveContainer;

    // curve type 선택할 수 있게 나중에 변경
    private bool animate = false;
    private float time = 0f;
    private float period = 0.3f;
    private AnimationCurve scaleCurve;
    private VisualElement visualElement;

    protected virtual void Awake()
    {
        scaleCurve = animCurveContainer.Heartbeat;
    }

    public void SetVisualElement(VisualElement visualElement)
    {
        this.visualElement = visualElement;
    }

    public void StartAnimation()
    {
        animate = true;
        time = 0f;
    }

    private void Update()
    {
        if (animate)
        {
            time += Time.deltaTime;
            if (time > period)
            {
                animate = false;
                time = period;
            }

            float scale = scaleCurve.Evaluate(time / period);
            visualElement.transform.scale= new Vector3(scale, scale, 1f);
        }
    }
}