
using System;
using UnityEngine;

public class IconizedAction : Iconized<Action>
{
    public string Name
    {
        get => name;
        set => name = value;
    } 
    
    private string name, optionText, optionTextNoTranslate;
    private FXCameraController particleIconCamera;

    public IconizedAction(Action content, Sprite icon = null, string optionText = "", string optionTextNoTranslate = "") : base(content, icon)
    {
        this.optionText = optionText;
        this.optionTextNoTranslate = optionTextNoTranslate;
    }
    
    public string GetOptionText()
    {
        return optionText;
    }

    public string GetOptionTextNoTranslate()
    {
        return optionTextNoTranslate;
    }

    public void SetParticleIconCam(FXCameraController particleIconCamera)
    {
        this.particleIconCamera = particleIconCamera;
    }

    public bool TryGetParticleIconCam(out FXCameraController particleIconCamera)
    {
        particleIconCamera = this.particleIconCamera;
        return !ReferenceEquals(this.particleIconCamera, null);
    }
}
