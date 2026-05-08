using UnityEngine;

public class TransparentBlock : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock propertyBlock;

    [SerializeField]
    private float targetAlpha = 0.5f;
    [SerializeField]
    private float changeTime = 0.5f;

    private float timeElapsed = 0f;
    private Color initialColor;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(propertyBlock);

        initialColor = meshRenderer.sharedMaterial.color;
        initialColor.a = 0f;
        propertyBlock.SetColor("_BaseColor", initialColor);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnEnable()
    {
        timeElapsed = 0f;
        initialColor.a = 0f;
        UpdateColor();
    }

    private void Update()
    {
        if (timeElapsed < changeTime)
        {
            initialColor.a = Mathf.Lerp(0f, targetAlpha, timeElapsed / changeTime);
            UpdateColor();
            timeElapsed += Time.deltaTime;
        }
        else if (initialColor.a != targetAlpha)
        {
            initialColor.a = targetAlpha;
            UpdateColor();
        }
    }

    private void UpdateColor()
    {
        propertyBlock.SetColor("_BaseColor", initialColor);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}
