
using System;
using UnityEngine;

public class VFXController : MonoBehaviour
{
    private ParticleSystem[] particleSystems;
    
    private void Awake()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }
    
    public void TurnOffAndOn()
    {
        foreach (var particleSystem in particleSystems)
        {
            particleSystem.Stop();
            particleSystem.Clear();
            particleSystem.Play();
        }
    }


    private void OnEnable()
    {
        foreach (var particleSystem in particleSystems)
        {
            particleSystem.Play();
        }
    }

    private void OnDisable()
    {
        foreach (var particleSystem in particleSystems)
        {
            particleSystem.Stop();
        }
    }
}
