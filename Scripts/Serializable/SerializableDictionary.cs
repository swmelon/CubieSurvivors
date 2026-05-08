using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    public Dictionary<TKey, TValue> ToDictionary()
    {
        Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

        for (int i = 0; i < keys.Count; i++)
        {
            if (dict.ContainsKey(keys[i]))
            {
                Debug.LogError("Duplicated prefab enum type. Check the prefab list.");
            }
            else
            {
                dict[keys[i]] = values[i];
            }
        }

        return dict;
    }

    public void FromDictionary(Dictionary<TKey, TValue> dict)
    {
        keys.Clear();
        values.Clear();

        foreach (var pair in dict)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
}