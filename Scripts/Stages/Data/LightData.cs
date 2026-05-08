using UnityEngine;


[CreateAssetMenu(fileName = "Ambient", menuName = "ScriptableObjects/Stage/AmbientData", order = 1)]
public class AmbientData : ScriptableObject
{
    public bool staticLight;
    public bool staticFog;
    public bool useBackground;

    public Color startFogColor;
    public Color endFogColor;
    public Color startLightColor;
    public Color endLightColor;
    public float startLightIntensity;
    public float endLightIntensity;
    public Color pillarColor;
}
