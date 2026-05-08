
using System;
using System.Collections;
using UnityEngine;

public class OnHitBlinker : MonoBehaviour
{
    public float lerpTime = 0.1f; 
    public Color hitColor = Color.white;
    
    private MeshRenderer meshRenderer;
    
    [SerializeReference]
    private Color originalColor;
    
    private bool started = false;

    private MaterialPropertyBlock materialPropertyBlock;

    [SerializeField]
    private Enemy enemy;
    
    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
    }

    private void Start()
    {
        transform.root.GetComponent<Damagable>().OnHit.AddListener((direction) => Blink());

        // sharedMaterial로 참조해야 instance가 생기지 않음
        originalColor = meshRenderer.sharedMaterial.color;
        started = true;
    }

    private void Blink()
    {
        StartCoroutine(LerpColor());
    }

    private IEnumerator LerpColor()
    {
        float elapsedTime = 0;

        while (elapsedTime < lerpTime)
        {
            materialPropertyBlock.SetColor("_BaseColor", Color.Lerp(originalColor, hitColor, elapsedTime / lerpTime));
            meshRenderer.SetPropertyBlock(materialPropertyBlock);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Once done, reset to original color or move on to any other logic
        materialPropertyBlock.SetColor("_BaseColor", originalColor);
        meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }
}
