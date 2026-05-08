using UnityEngine;
using System.Collections.Generic;

public class DamageTextPositioner
{
    public float BaseOffset
    {
        set => baseOffset = new Vector3(0, value, 0);
    }
    
    private const float baseOffsetY = 0.2f;

    private Vector3 baseOffset = new Vector3(0, baseOffsetY, 0);
    private float verticalOffset = 0f;
    private const float offsetIncrement = 0.2f;
    private const float maxOffset = 1.0f;
    
    

    public Vector3 GetNextPosition(Vector3 enemyPosition, Vector3 enemyScale)
    {
        Vector3 spawnPosition = enemyPosition + baseOffset + new Vector3(0, verticalOffset, 0);

        // Increment the static offset
        verticalOffset += offsetIncrement;
        if (verticalOffset >= maxOffset)
        {
            verticalOffset = 0f;
        }

        return spawnPosition;
    }
}