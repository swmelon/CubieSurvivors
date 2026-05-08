using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "FloorLiquidContainer", menuName = "ScriptableObjects/FloorLiquidContainer", order = 1)]
public class FloorLiquidContainerSO : ScriptableObject
{
    [SerializeField]
    private SerializableDictionary<WaterType, Material> liquidMaterials;

    [SerializeField]
    private SerializableDictionary<WaterType, Material> solidMaterials;

    private Dictionary<WaterType, Material> materials;

    private void OnEnable()
    {
        materials = new Dictionary<WaterType, Material>();

        foreach (var pair in liquidMaterials.ToDictionary())
        {
            materials[pair.Key] = pair.Value;
        }
        
        foreach (var pair in solidMaterials.ToDictionary())
        {
            materials[pair.Key] = pair.Value;
        }
    }

    public bool TryGetLiquidMaterial(WaterType liquidType, out Material material)
    {
        return materials.TryGetValue(liquidType, out material);
    }
}