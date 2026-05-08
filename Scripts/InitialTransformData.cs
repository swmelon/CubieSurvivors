using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[CreateAssetMenu(fileName = "InitialTransformData", menuName = "ScriptableObjects/InitialTransformData", order = SOAssetMenuIndex.Enemy)]
public class InitialTransformData : ScriptableObject
{ 
    [SerializeField]
    private SerializableDictionary<string, TransformData> serializedInitialTransforms = new SerializableDictionary<string, TransformData>();
    
    private Dictionary<string, TransformData> initialTransforms = new Dictionary<string, TransformData>();
    
    private void OnEnable()
    {
        initialTransforms = serializedInitialTransforms.ToDictionary();
    }
    
    public bool TryGetValue(string key, out TransformData data)
    {
        return initialTransforms.TryGetValue(key, out data);
    }

    public void AddData(string key, TransformData data)
    {
        initialTransforms.Add(key, data);
        serializedInitialTransforms.FromDictionary(initialTransforms);
    }

    public void ResetTransform(Transform targetRoot)
    {
        Transform[] allTransforms = targetRoot.GetComponentsInChildren<Transform>(true);

        foreach (Transform t in allTransforms)
        {
            if (initialTransforms.TryGetValue(t.name, out TransformData data))
            {
                t.localPosition = data.position;
                t.localRotation = data.rotation;
                t.localScale = data.scale;
            }
        }
    }

    public void CaptureInitialState(Transform targetRoot)
    {
        initialTransforms.Clear();

        Transform[] allTransforms = targetRoot.GetComponentsInChildren<Transform>(true);
        Transform[] childTransforms = allTransforms.Skip(1).ToArray();

        foreach (Transform t in childTransforms)
        {
            TransformData data = new TransformData
            {
                position = t.localPosition,
                rotation = t.localRotation,
                scale = t.localScale
            };
            
            AddData(t.name, data);
        }
    }
}

[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}