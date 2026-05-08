using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "ESDLayout", menuName = "ScriptableObjects/ESDLayout", order = 1)]
public class ESDLayout : ScriptableObject
{
    public List<WorldDirection> directions = new List<WorldDirection>();
    public bool moveESD;
    public Sprite indicator;

    public bool CycleMode()
    {
        return moveESD;
    }

    public bool TryGetIndicator(out Sprite sprite)
    {
        sprite = indicator;
        return sprite != null;
    }
}