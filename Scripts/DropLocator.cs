using System.Collections.Generic;
using System.Linq;
using Local.Scripts.Extensions;
using UnityEngine;


public class DropLocator
{
    public bool Locate(Vector3[] fixedPoints, float dropSize, int numOfDrops, int stageSize, 
        out List<Vector3> selectedLocations, float radius = 6f)
    {
        Debug.Assert(dropSize >= 1, "dropSize >= 1");
        Debug.Assert(numOfDrops >= 1, "numOfDrops >= 1");
        Debug.Assert(stageSize >= 1, "stageSize >= 1");
        Debug.Assert(fixedPoints.Length >= 1, "fixedPoints.Length >= 1");
        
        stageSize -= (int)(dropSize / 2) + 1;
        selectedLocations = new List<Vector3>();
        
        Vector2[] fixedPoints2D = new Vector2[fixedPoints.Length];

        foreach (var point in fixedPoints)
        {
            Debug.DrawRay(point, Vector3.up * 10, Color.red, 2f);
        }
        
        for (int i = 0; i < fixedPoints.Length; i++)
        {
            fixedPoints2D[i] = new Vector2(fixedPoints[i].x, fixedPoints[i].z);
        }
        
        // calculate vertices, directions, perps 
        Vector2 prevLoc = fixedPoints2D[^1];
        Vector2[] vertices = new Vector2[fixedPoints2D.Length];
        Vector2[] directions = new Vector2[fixedPoints2D.Length];
        Vector2[] perps = new Vector2[fixedPoints2D.Length];
        
        for(int i = 0; i < fixedPoints2D.Length; i++)
        {
            Vector2 loc = fixedPoints2D[i];
            Vector2 dir = loc - prevLoc;
            Vector2 perp = Vector2.Perpendicular(dir).normalized;
            Vector2 vertex = prevLoc + perp * radius;
            
            directions[i] = dir;
            vertices[i] = vertex;
            perps[i] = perp;
            prevLoc = loc;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.DrawRay(new Vector3(vertices[i].x, 1 , vertices[i].y), 
                new Vector3(directions[i].x, 0, directions[i].y), Color.black, 2f);
        }
        
        
        // calculate angles, lengths
        Vector2 prevPerp = perps[^1];
        float[] angles = new float[fixedPoints2D.Length];
        
        for(int i = 0; i < fixedPoints2D.Length; i++)
        {
            Vector2 perp = perps[i];
            float angle = Vector2.Angle(prevPerp, perp);
            angles[i] = angle;
            prevPerp = perp;
        }
        
        if (fixedPoints2D.Length == 1)
        {
            angles[0] = 360f;
            perps[0] = Vector2.right;
        }
        
        float[] lengths = new float[fixedPoints2D.Length * 2];
        
        for (int i = 0; i < fixedPoints2D.Length; i++)
        {
            lengths[2 * i] = directions[i].magnitude;
            lengths[2 * i + 1] = radius * Mathf.Deg2Rad * angles[i];
        }
        
        float sum = 0f;
        
        foreach (var length in lengths)
        {
            sum += length;
        }
        
        // calculate candidate positions
        
        int numCandidates = Mathf.RoundToInt(sum / dropSize);
        Vector2[] candidates = new Vector2[numCandidates];
        
        if (numCandidates < numOfDrops)
        {
            return false;
        }
        
        float lengthLeft = dropSize;
        int candidateIndex = 0;

        for (int i = 0; i < lengths.Length; i++)
        {
            int verticesIndex = i / 2;
            if(lengthLeft < lengths[i])
            {
                if (i % 2 == 0)
                {
                    candidates[candidateIndex] = vertices[verticesIndex] + directions[verticesIndex].normalized * lengthLeft;
                }
                else
                {
                    candidates[candidateIndex] = (Vector2)(Quaternion.Euler(0, 0, -(lengthLeft * 180) / (radius * Mathf.PI))
                                              * perps[verticesIndex] * radius)
                                    + fixedPoints2D[verticesIndex];
                }
                
                candidateIndex++;
                
                if(candidateIndex == numCandidates)
                {
                    break;
                }
                
                lengthLeft += dropSize;
                i--;
                continue;
            }
            
            lengthLeft -= lengths[i];
        }
        
        // check if candidates are in stage

        foreach (var location in candidates)
        {
            if (Mathf.Abs(location.x) > stageSize || Mathf.Abs(location.y) > stageSize)
            {
                continue;
            }
            
            selectedLocations.Add(new Vector3(location.x, 0, location.y));
        }
        
        for (int i = 0; i < selectedLocations.Count; i++)
        {
            Debug.DrawRay(selectedLocations[i], Vector3.up, Color.blue, 2f);
        }
        
        if (selectedLocations.Count < numOfDrops)
        {
            return false;
        }

        selectedLocations = selectedLocations.PickRandom(numOfDrops).ToList();
        return true;
    }
}
