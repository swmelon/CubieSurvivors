using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UITimer : MonoBehaviour
{
    private RectTransform rectTransform;

    [SerializeField]
    private float maxWidth = 500f;

    [SerializeField]
    private float minWidth = 300f;

    [SerializeField]
    private float recoveringTime = 0.6f;

    [SerializeField]
    private float glowingPeriod = 2f;

    [SerializeField]
    private float startGlowingTime = 4f;

    [SerializeField]
    private AnimationCurve glowingIntensityCurveRed, glowingIntensityCurveGreen;

    [SerializeField]
    private FloatEventChannelSO setTimerEC;

    [SerializeField]
    private EventChannelSO timerEndEC;

    [SerializeField]
    private EventChannelSO getInitialWeaponEC;

    [SerializeField]
    private EventChannelSO playerDeadEC, playerReviveEC, playerFallEC, restartGameEC;

    [SerializeField]
    private EventChannelSO defeatBossEC;
    


    private enum TimerState { Running, Stopped, Recovering}
    private TimerState state;
    private TimerState prevState;
    private float timeElapsed, setTime;
    private Image image;
    private bool readyToResume = false;
    private Color initialImageColor;
    
    private float widthOnStartToRecover;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        state = TimerState.Recovering;
        initialImageColor = image.color;
    }

    private void OnEnable()
    {
        setTimerEC.Subscribe(SetTimer);
        playerDeadEC.Subscribe(Pause);
        playerFallEC.Subscribe(Stop);
        playerReviveEC.Subscribe(Resume);
        defeatBossEC.Subscribe(OnDefeatBoss);
    }

    private void OnDisable()
    {
        setTimerEC.Unsubscribe(SetTimer);
        playerDeadEC.Unsubscribe(Pause);
        playerFallEC.Unsubscribe(Stop);
        playerReviveEC.Unsubscribe(Resume);
        defeatBossEC.Unsubscribe(OnDefeatBoss);
    }

    public void SetTimer(float seconds)
    {
        timeElapsed = 0;
        setTime = seconds;

        if (readyToResume)
        {
            prevState = TimerState.Running;
            return;
        }

        state = TimerState.Running;
    }

    private void Update()
    {
        switch (state)
        {
            case TimerState.Stopped:
                return;
            case TimerState.Running:
                timeElapsed += Time.deltaTime;
                float ratio = timeElapsed / setTime;
                rectTransform.sizeDelta = new Vector2(Mathf.Lerp(maxWidth, minWidth, ratio), rectTransform.sizeDelta.y);

                if (timeElapsed >= setTime)
                {
                    timeElapsed = 0f;
                    state = TimerState.Recovering;
                    widthOnStartToRecover = minWidth;
                    timerEndEC.Raise();
                    return;
                }



                float timeRemaining = setTime - timeElapsed;

                if (timeElapsed < 1f)
                {
                    float val = glowingIntensityCurveGreen.Evaluate(timeElapsed);

                    Color color =  image.color;
                    color.g = val;

                    image.color = color;
                }

                if (timeRemaining < startGlowingTime)
                {
                    float val = ((startGlowingTime - timeRemaining) % glowingPeriod) / glowingPeriod;
                    val = glowingIntensityCurveRed.Evaluate(val);

                    Color color =  image.color;
                    color.r = val;

                    image.color = color;
                }


                break;
            case TimerState.Recovering:
                timeElapsed += Time.deltaTime;
                float ratio2 = timeElapsed / recoveringTime;
                rectTransform.sizeDelta = new Vector2(Mathf.Lerp(widthOnStartToRecover, maxWidth, ratio2), rectTransform.sizeDelta.y);

                if (timeElapsed >= recoveringTime)
                {
                    timeElapsed = 0f;
                    state = TimerState.Stopped;
                }
                break;
        }
    }

    private void Pause()
    {
        readyToResume = true;
        prevState = state;

        if (state != TimerState.Recovering)
        {
            state = TimerState.Stopped;
        }
    }

    private void Resume()
    {
        readyToResume = false;
        state = prevState;
    }

    private void Stop()
    {
        image.color = initialImageColor;
        timeElapsed = 0f;
        widthOnStartToRecover = rectTransform.sizeDelta.x;
        state = TimerState.Recovering;
    }

    private void OnDefeatBoss()
    {
        SetTimer(5f);
    }
}