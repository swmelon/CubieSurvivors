using UnityEngine;
using UnityEngine.Rendering.Universal;


[CreateAssetMenu(fileName = "GraphicsQualityManager", menuName = "ScriptableObjects/Graphics/Quality Manager")]
public class GraphicsQualityManager : ScriptableObject
{
    [SerializeField]
    private UniversalRenderPipelineAsset lowQualitySettings;
    [SerializeField]
    private UniversalRenderPipelineAsset mediumQualitySettings;
    [SerializeField]
    private UniversalRenderPipelineAsset highQualitySettings;
    [SerializeField]
    private UniversalRenderPipelineAsset ultraQualitySettings;

    public void SetGraphicsQuality(GraphicQuality quality)
    {
        switch (quality)
        {
            case GraphicQuality.Low:
                QualitySettings.renderPipeline = lowQualitySettings;
                break;
            case GraphicQuality.Medium:
                QualitySettings.renderPipeline = mediumQualitySettings;
                break;
            case GraphicQuality.High:
                QualitySettings.renderPipeline = highQualitySettings;
                break;
            case GraphicQuality.Ultra:
                QualitySettings.renderPipeline = ultraQualitySettings;
                break;
        }

    }
}
