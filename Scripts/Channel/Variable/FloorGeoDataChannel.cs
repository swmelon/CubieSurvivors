using Local.Scripts.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorGeoDataChannel", menuName = "ScriptableObjects/Channels/FloorGeoDataChannel", order = SOAssetMenuIndex.Channel)]
public class FloorGeoDataChannel : ScriptableObject
{
    private FloorLEDBuilder floorBuilder;
    private FloorLiquidBuilder liquidBuilder;
    private float nonLiquidHeightOffset = 0.49f;
    public void RegisterLEDBuilder(FloorLEDBuilder floorBuilder)
    {
        this.floorBuilder = floorBuilder;
    }
    
    public void UnregisterLEDBuilder(FloorLEDBuilder floorBuilder)
    {
        if (this.floorBuilder == floorBuilder)
        {
            this.floorBuilder = null;
        }
    }

    public void RegisterLiquidBuilder(FloorLiquidBuilder liquidBuilder)
    {
        this.liquidBuilder = liquidBuilder;
    }

    public void UnregisterLiquidBuilder(FloorLiquidBuilder liquidBuilder)
    {
        if (this.liquidBuilder == liquidBuilder)
        {
            this.liquidBuilder = null;
        }
    }

    public float GetHeightOf(Vector3 location)
    {
        float height = floorBuilder.GetHeightOf(location);

        // 높이가 -1이면 무조건 액체라고 가정
        // 이 게임에서 높이를 다양하게 만들 필요가 없다.
        if (!liquidBuilder.IsLiquid && height == -1)
        {
            
            height += nonLiquidHeightOffset;
        }

        return height;
    }

    public bool TryGetHeightOf(Vector3 location, out float height)
    {
        height = GetHeightOf(location);
        return height != int.MinValue;
    }

    public bool OnStage(Vector3 location, int truncate= 1)
    {
        return floorBuilder.IsOnFloor(location, truncate);
    }

    public bool TryGetPopOutItemPosition(Vector3 center, float radius, float itemSize, int numItems, 
        LayerMask layerToCheck, out Vector3[] positions)
    {
        LocateItemAround(new Vector3[] { center }, itemSize, numItems,
            out List<Vector3> locations, radius: radius);

        // check overlap

        float halfItemSize = itemSize * 0.5f;
        List<Vector3> validLocations = new List<Vector3>();

        for (int i = 0; i < locations.Count; i++)
        {
            Vector3 location = locations[i];
            location.y = GetHeightOf(locations[i]) + halfItemSize;

            if (Physics.OverlapSphere(location, halfItemSize, layerToCheck).Length == 0)
            {
                validLocations.Add(location);
            }
        }

        if (validLocations.TryPickRandom(numItems, out positions))
        {
            return true;
        }

        return false;
    }

    public bool LocateItemAround(Vector3[] fixedPoints, float itemSize, int numOfDrops,
        out List<Vector3> selectedLocations, float radius = 6f)
    {
        Debug.Assert(itemSize >= 1, "dropSize >= 1");
        Debug.Assert(numOfDrops >= 1, "numOfDrops >= 1");
        Debug.Assert(fixedPoints.Length >= 1, "fixedPoints.Length >= 1");

        int stageSize = floorBuilder.Size;
        stageSize -= (int)(itemSize / 2) + 1;
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

        for (int i = 0; i < fixedPoints2D.Length; i++)
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
            Debug.DrawRay(new Vector3(vertices[i].x, 1, vertices[i].y),
                new Vector3(directions[i].x, 0, directions[i].y), Color.black, 2f);
        }


        // calculate angles, lengths
        Vector2 prevPerp = perps[^1];
        float[] angles = new float[fixedPoints2D.Length];

        for (int i = 0; i < fixedPoints2D.Length; i++)
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

        float sum = lengths.Sum();

        // calculate candidate positions

        int numCandidates = Mathf.RoundToInt(sum / itemSize);
        Vector2[] candidates = new Vector2[numCandidates];

        if (numCandidates < numOfDrops)
        {
            return false;
        }

        float lengthLeft = itemSize;
        int candidateIndex = 0;

        for (int i = 0; i < lengths.Length; i++)
        {
            int verticesIndex = i / 2;
            if (lengthLeft < lengths[i])
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

                if (candidateIndex == numCandidates)
                {
                    break;
                }

                lengthLeft += itemSize;
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