
using System.Collections.Generic;
using System.Linq;
using Local.Scripts.Extensions;
using UnityEngine;

public class ItemSpinner : MonoBehaviour
{
    private List<Item> items = new List<Item>();
    
    public void Setup(List<Item> items)
    {
        float angleIncrement = 360f / items.Count;
        float angle = 0f;

        float radius = items.Count * 0.2f;

        items.FisherShuffle();
        
        foreach (Item item in items)
        {
            item.transform.rotation = Quaternion.Euler(0f, angle, 0f);
            item.transform.position = transform.position + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
            item.transform.SetParent(transform);
            item.OnActivated = OnItemSelected;
            angle += angleIncrement;
        }
        
        this.items = items;
    }
    
    private void OnItemSelected()
    {
        int notReleasedCount = 0;
        
        foreach (var item in items)
        {
            if (item.Released)
            {
                notReleasedCount++;
                continue;
            }
            
            item.Release();
        }

        if (notReleasedCount != 1)
        {
            Debug.LogError("There should be only one item selected from the spinner.");
        }
        
        Destroy(gameObject);
    }
    
    private void Update()
    {
        transform.Rotate(Vector3.up, 30f * Time.deltaTime);
    }
}
