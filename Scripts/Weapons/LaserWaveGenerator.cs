using System;
using Local.Scripts.Extensions;
using UnityEngine;


public class LaserWaveGenerator : MonoBehaviour
{
    [SerializeField]
    private AnimationCurve waveWidthCurve, radiusCurve;

    [SerializeField] 
    private float waveMaxWidth = 1f, waveMaxRadius = 1f, waveInterval = 1f;
    
    [SerializeField]
    private CircleRayWithLineStrip circleRay;

    [SerializeField] 
    private bool startOnAwake = false;
    
    private float time;
    private bool stop, activated;

    private void Awake()
    {
        time = 0f;
        if (startOnAwake)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
        
        circleRay.SetTargetLayer(LayerMaskCash.PlayerAndEnemy);
    }

    private void Update()
    {
        if (!activated)
        {
            return;
        }
        
        var laserWidth = waveWidthCurve.Evaluate(time/waveInterval) * waveMaxWidth;
        var radius = radiusCurve.Evaluate(time/waveInterval) * waveMaxRadius;
        
        circleRay.LineWidth = laserWidth;
        circleRay.Radius = radius;
        
        time += Time.deltaTime;
        
        if (time > waveInterval)
        {
            time = 0f;
            
            if (stop)
            {
                Stop();
            }
        }
    }
    
    public void Deactivate()
    {
        stop = true;
    }
    
    public void Activate()
    {
        stop = false;
        activated = true;
    }

    public void Stop()
    {
        time = 0f;
        stop = true;
        activated = false;
        circleRay.LineWidth = 0f;
        circleRay.Radius = 0f;
    }
}
