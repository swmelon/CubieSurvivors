using UnityEngine;

[System.Serializable]
public class ChildIndexer
{
    [SerializeField]
    private string indexPath;

    public string IndexPath
    {
        get => indexPath;
        set => indexPath = value;
    }

    private Transform FindTargetTransform(Transform root)
    {
        var indices = indexPath.Split('-');
        Transform current = root;

        foreach (var indexString in indices)
        {
            if (int.TryParse(indexString, out int index))
            {
                if (current.childCount > index)
                    current = current.GetChild(index);
                else
                    return null; // Index out of range
            }
            else
            {
                return null; // Invalid index
            }
        }

        return current;
    }

    public void AddChild(GameObject root, GameObject child)
    {
        Transform parentTransform = FindTargetTransform(root.transform);
        if (parentTransform != null && child != null)
        {
            child.transform.SetParent(parentTransform, false);
        }
    }

    public void RemoveChild(GameObject root, GameObject child)
    {
        Transform parentTransform = FindTargetTransform(root.transform);
        if (parentTransform != null && child != null && child.transform.parent == parentTransform)
        {
            child.transform.SetParent(null);
        }
    }
}