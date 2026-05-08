using Local.Scripts.Extensions;
using UnityEngine;

public class DataContainer<T> : ScriptableObject where T : class
{
    [SerializeField]
    private T[] datas;

    public T[] Datas => datas;

    public T GetRandom()
    {
        if (datas.TryPickRandom(out T data))
        {
            return data;
        }

        Debug.LogError("No data found in the container. Container must contain at least one content.");
        return null;
    }
}