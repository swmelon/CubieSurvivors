using System;
using UnityEngine;

public class Shield : MonoBehaviour, IColorable
{
    private ParticleSystem.MainModule mainModule;
    private Color targetColor; 

    private void Awake()
    {
        mainModule = GetComponent<ParticleSystem>().main;
    }

    public void SetColor(Color color)
    {
        targetColor = color;
        mainModule.startColor = color;
        
        // lerp color
    }
}
