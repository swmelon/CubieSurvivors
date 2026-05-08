using UnityEngine;
using System.Collections.Generic;

public class UGUIEasyHeartbeat : MonoBehaviour, IEasyListener, IEasyPitchListener
{
    [SerializeField]
    private List<RectTransform> beatListeners;

    [SerializeField]
    private FloatChannelSO playerHealthChannel;

    [SerializeField]
    private AnimationCurveContainer animCurveContainer;

    private AnimationCurve scaleCurve;

    private float periodReference;
    private float period;
    private float time = 0f;
    private bool needUpdatePeriod = true;
    private bool playerHealthLow = false;
    private bool stop = false;
    private float lowHealthThres = 0.3f;

    private void Awake()
    {
        scaleCurve = animCurveContainer.Heartbeat;
    }

    public void OnBeat(EasyEvent currentAudioEvent)
    {
        if (needUpdatePeriod)
        {
            periodReference = FMODAudioManager.instance.BeatLength;
            needUpdatePeriod = false;
            stop = false;
        }

        playerHealthLow = playerHealthChannel.Value <= lowHealthThres;

        if (!playerHealthLow)
        {
            period = periodReference * 2f;

            if (!currentAudioEvent.StrongBeat())
            {
                return;
            }
        }
        else
        {
            period = periodReference;
        }
     
        time = 0f;
    }

    public void OnPitchChanged()
    {
        needUpdatePeriod = true;
        stop = true;
    }

    private void Update()
    {
        if (stop)
        {
            return;
        }

        if (time < periodReference)
        {
            time += Time.unscaledDeltaTime;
            float scale = scaleCurve.Evaluate(time / periodReference);
            foreach (RectTransform beatListener in beatListeners)
            {
                beatListener.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }

    public void AddListener(RectTransform listener)
    {
        beatListeners.Add(listener);
    }

    public void RemoveListener(RectTransform listener)
    {
        listener.localScale = Vector3.one;
        beatListeners.Remove(listener);
    }
}