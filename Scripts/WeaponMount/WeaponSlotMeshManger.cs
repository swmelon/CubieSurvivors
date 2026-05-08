using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
public class WeaponSlotMeshManger : MonoBehaviour
{   
    [SerializeField]
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    
    // Start is called before the first frame update
    private MeshRenderer otherMeshRenderer;
    
    private int count;


    
    private void Awake()
    {
        otherMeshRenderer = GetComponent<MeshRenderer>();
        otherMeshRenderer.enabled = false;
        foreach (var renderer in meshRenderers)
        {
            renderer.enabled = false;
        }
        count = 0;
    }

    public void WeaponMounted()
    {
        count++;
        if (count > 0)
        {
            otherMeshRenderer.enabled = true;
            
            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = true;
            }
        }
    }

    public void WeaponUnmounted()
    {
        count--;
        if (count == 0)
        {
            otherMeshRenderer.enabled = false;
            
            foreach (var renderer in meshRenderers)
            {
                renderer.enabled = false;
            }
        }
    }
}
