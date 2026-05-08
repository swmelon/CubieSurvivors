using System.Collections.Generic;
using Local.Scripts.Extensions;
using UnityEngine;

public class LocatableESD : EnemySpawnDevice<LocatableESD>, ILocatable
{
    [SerializeField][Range(1f, 3f)]
    private int size;

    public int Size
    {
        get => size;
    }
    
    
    public bool SelectLocation(Dictionary<Vector3, Vector3[]> locations, out List<Vector3> selected)
    {
        List<List<Vector3>> pieces = new List<List<Vector3>>();

        foreach (var array in locations.Values)
        {
            List<Vector3> space = new List<Vector3>();

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != Vector3.zero)
                {
                    space.Add(array[i]);
                }
                else
                {
                    space = new List<Vector3>();
                }

                if (space.Count == size)
                {
                    pieces.Add(space);
                    space = new List<Vector3>();
                }
            }
        }

        if (pieces.Count == 0)
        {
            selected = null;
            return false;
        }

        selected = pieces.PickRandom();
        return true;
    }
}

