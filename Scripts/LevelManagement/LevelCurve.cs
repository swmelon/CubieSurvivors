using UnityEngine;

public class LevelCurve
{
    public float GetPoint() => currentPoint;
    public float GetInitialPoint() => initialPoint;

    private readonly float exponent;
    private float currentPoint;
    private float initialPoint;

    public LevelCurve(float initialPoint, float exponent)
    {
        this.initialPoint = initialPoint;
        this.currentPoint = initialPoint;

        if (exponent <= 1)
        {
            Debug.LogError("Exponent must be greater than 1");
        }
        
        this.exponent = exponent;
    }
    
    public void UpdatePoint()
    {
        currentPoint *= exponent;
    }
    
    public void Reset(float initialPoint)
    {
        currentPoint = initialPoint;
    }
}
