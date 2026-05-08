using System;
using System.Collections.Generic;

public class DoubleKeyDictionary<TKey1, TKey2, TValue>
{
    private Dictionary<TKey1, Dictionary<TKey2, TValue>> innerDictionary = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
    
    public void Add(TKey1 key1, TKey2 key2, TValue value)
    {
        if (!innerDictionary.ContainsKey(key1))
        {
            innerDictionary[key1] = new Dictionary<TKey2, TValue>();
        }
        
        if (!innerDictionary[key1].ContainsKey(key2))
        {
            innerDictionary[key1][key2] = value;
        }
        else
        {
            throw new ArgumentException("An element with the same key already exists in the dictionary.");
        }
    }

    public bool TryGetValue(TKey1 key1, TKey2 key2, out TValue value)
    {
        value = default(TValue);

        if (innerDictionary.TryGetValue(key1, out var subDictionary))
        {
            return subDictionary.TryGetValue(key2, out value);
        }

        return false;
    }

    public bool ContainsKey(TKey1 key1, TKey2 key2)
    {
        if (innerDictionary.TryGetValue(key1, out var subDictionary))
        {
            return subDictionary.ContainsKey(key2);
        }

        return false;
    }

    public bool Remove(TKey1 key1, TKey2 key2)
    {
        if (innerDictionary.TryGetValue(key1, out var subDictionary) && subDictionary.Remove(key2))
        {
            if (subDictionary.Count == 0)
            {
                // Remove the inner dictionary if it becomes empty
                innerDictionary.Remove(key1);
            }
            return true;
        }

        return false;
    }

    // Optionally, you can add methods for enumeration, clearing, etc., as needed

    // Example of getting all keys for the first key:
    public IEnumerable<TKey2> GetKeysForFirstKey(TKey1 key1)
    {
        if (innerDictionary.TryGetValue(key1, out var subDictionary))
        {
            return subDictionary.Keys;
        }
        return new List<TKey2>();
    }
}