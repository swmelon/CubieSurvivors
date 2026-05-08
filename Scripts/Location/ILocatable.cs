using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILocatable
{
    public bool SelectLocation(Dictionary<Vector3, Vector3[]> locations, out List<Vector3> selected);
}
