using System;
using UnityEngine;
using UnityEngine.Events;

public class DynamicMesh : MonoBehaviour
{
    [SerializeField] 
    private Mesh[] meshes;

    private int meshIndex;
    private MeshFilter meshFilter;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    public void LinkTo<T>(Upgradable<T> upgradable)
    {
        upgradable.Upgraded += () => UpgradeMesh();
    }

    public bool UpgradeMesh()
    {
        if (meshIndex < meshes.Length - 1)
        {
            meshFilter.mesh = meshes[meshIndex];
            meshIndex++;
            return true;
        }

        return false;
    }
}
