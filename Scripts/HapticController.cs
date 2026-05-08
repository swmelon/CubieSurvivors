using UnityEngine;
using CandyCoded.HapticFeedback;

[CreateAssetMenu(fileName = "HapticController", menuName = "ScriptableObjects/HapticController")]
public class HapticController : ScriptableObject
{
    private bool hapticFeedbackEnabled = true;

    public void ActivateHapticFeedback(bool val)
    {
        hapticFeedbackEnabled = val;
    }

    public void LightFeedback()
    {
        if (hapticFeedbackEnabled)
        {
            HapticFeedback.LightFeedback();
        }        
    }

    public void MediumFeedback()
    {
        if (hapticFeedbackEnabled)
        {
            HapticFeedback.MediumFeedback();
        }        
    }
    
    public void HeavyFeedback()
    {
        if (hapticFeedbackEnabled)
        {
            HapticFeedback.HeavyFeedback();
        }        
    }
}