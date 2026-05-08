using System.Collections;
using TMPro;
using UnityEngine;

public class DamageText : Poolable<DamageText>
{
    private TextMeshProUGUI textMeshPro;
    private RectTransform rectTransform;
    private Color originalColor;
    private Color fadedColor;
    private readonly float fadeTime = 0.9f;
    
    private void Awake()
    {
        textMeshPro = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        
        originalColor = textMeshPro.color;
        fadedColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
    }
    
    public void SetText(int damage)
    {
        textMeshPro.text = damage.ToString();
        StartCoroutine(FadeText());
    }
    
    public void SetRotation(Camera targetingCamera)
    {
        rectTransform.rotation = targetingCamera.transform.rotation;
    }
    
    private IEnumerator FadeText()
    {
        for (float t = 0.01f; t < fadeTime; t += Time.deltaTime)
        {
            textMeshPro.color = Color.Lerp(originalColor, new Color(originalColor.r, originalColor.g, originalColor.b, 0), Mathf.Min(1, t / fadeTime));
            yield return null;
        }
        
        Release();
    }
}
 