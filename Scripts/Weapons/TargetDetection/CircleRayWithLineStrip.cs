using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using VolumetricLines;

public class CircleRayWithLineStrip : CircleRay
{
    [SerializeField] 
    private VolumetricLineStripBehavior volLineStrip;

    private float lineWidth = 1f;

    public float LineWidth
    {
        set
        {
            lineWidth = value;
        }
    }

    protected override void UpdateVertices(ref Vector3[] updatedVertices)
    {
        base.UpdateVertices(ref updatedVertices);
        volLineStrip.UpdateLineVertices(updatedVertices);
        volLineStrip.LineWidth = lineWidth;
    }
}
