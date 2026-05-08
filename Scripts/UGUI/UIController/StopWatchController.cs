using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class StopWatchController : MonoBehaviour
{ 
    public float RecordedTime
    {
        get => time;
    }

    public enum TimeUIMode
    {
        Timer,
        Stopwatch,
        Pause,
    }

    // Subscriber
    [SerializeField]
    private EventChannelSO playerDeadEC, playerReviveEC;

    [SerializeField]
    private EventChannelSO startGameEC, defeatFinalBossEC;

    [SerializeField]
    private EventChannelSO playerFallEC;

    // Invoker
    [SerializeField] private EventChannelSO runOutOfTimerEventChannel;


    // 다른 스크립트에서 시간 값을 참조
    [SerializeField]
    private IntChannelSO timeCountChannel;
    
    private TextMeshProUGUI text;
    private float time = 0;
    private TimeUIMode mode;
    
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        playerDeadEC.Subscribe(PauseStopwatch);
        playerReviveEC.Subscribe(ResumeStopwatch);

        startGameEC.Subscribe(StartStopwatch);
        playerFallEC.Subscribe(ResetStopwatch);
        defeatFinalBossEC.Subscribe(PauseStopwatch);
        
        text.text = "00:00";
        mode = TimeUIMode.Pause;
    }

    private void OnDestroy()
    {
        playerDeadEC.Unsubscribe(PauseStopwatch);
        playerReviveEC.Unsubscribe(ResumeStopwatch);


        startGameEC.Unsubscribe(StartStopwatch);
        playerFallEC.Unsubscribe(ResetStopwatch);
        defeatFinalBossEC.Unsubscribe(PauseStopwatch);
    }

    private void StartTimer(float val)
    {
        time = val;
        mode = TimeUIMode.Timer;
    }

    private void StartStopwatch()
    {
        time = 0;
        mode = TimeUIMode.Stopwatch;
    }

    private void ResetStopwatch()
    {
        SetTime(0);
        mode = TimeUIMode.Pause;
    }

    private void Update()
    {
        switch (mode)
        {
            case TimeUIMode.Timer:
                UpdateTimer();
                break;
            case TimeUIMode.Stopwatch:
                UpdateStopWatch();
                break;
            case TimeUIMode.Pause:
                break;
         }
    }
    
    private void UpdateTimer()
    {
        time -= Time.deltaTime;

        DisplayTime();

        if (time <= 0)
        {
            text.text = "";
            mode = TimeUIMode.Pause;
            runOutOfTimerEventChannel.Raise();
        }
    }

    private void UpdateStopWatch()
    {
        SetTime(time + Time.deltaTime);
        DisplayTime();
    }

    private void DisplayTime()
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);

        switch (mode)
        {
            case TimeUIMode.Timer:
                text.text = string.Format("{0:00}:{1:00}", seconds, milliseconds);
                break;
            case TimeUIMode.Stopwatch:
                text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                break;
        }
    }

    private void PauseStopwatch()
    {
        mode = TimeUIMode.Pause;
    }

    private void ResumeStopwatch()
    {
        mode = TimeUIMode.Stopwatch;
    }

    public string GetTime()
    {
        return text.text;
    }

    public float GetTimeInSeconds()
    {
        return time;
    }

    private void SetTime(float time)
    {
        this.time = time;
        timeCountChannel.Register((int)time);
        DisplayTime();
    }
    
}
