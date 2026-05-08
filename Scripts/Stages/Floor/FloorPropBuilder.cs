using Local.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FloorPropBuilder : MonoBehaviour
{
    private FloorLEDBuilder floorLEDBuilder;
    private float propDensityConst = 2f;
    private float texturePropDensityConst = 3f;
    private float onLiquidPropDensityConst = 0.2f;
    private List<GameObject> props = new List<GameObject>();

    [SerializeField]
    private EventChannelSO enterEventStageEC;

    private void Awake()
    {
        floorLEDBuilder = transform.parent.GetComponentInChildren<FloorLEDBuilder>();
        enterEventStageEC.Subscribe(ClearProps);
    }

    private void OnDestroy()
    {
        enterEventStageEC.Unsubscribe(ClearProps);
    }

    /// <summary>
    /// This should be called first before calling BuildTextureProps or BuildPropsOnLiquid
    /// Because this will clear all the props.
    /// </summary>
    /// <param name="props"></param>
    /// <param name="floorSize"></param>
    public void BuildProps(ThemeData themeData, StageBlueprint blueprint)
    {
        int floorSize = blueprint.Size;
        float floorRatio = floorLEDBuilder.FloorRatio;
        float density = themeData.PropDensity;
        int numProps = (int)(Mathf.Pow(floorSize, 2) * density* propDensityConst * floorRatio);

        for (int i = 0; i < numProps; i++)
        {
            if (floorLEDBuilder.TryGetOnFloorObjectPosition(out Vector3 position))
            {
                GameObject prop = Instantiate(themeData.GetRandomProp(), position, Quaternion.identity);
                prop.transform.SetParent(transform);
                props.Add(prop);
            }
            else
            {
                break;
            }
        }

        // add on liquid props
    }

    public void BuildTextureProps(ThemeData themeData, StageBlueprint blueprint)
    {
        int floorSize = blueprint.Size;
        float floorRatio = floorLEDBuilder.FloorRatio;
        float density = themeData.TexturePropDensity;
        int numProps = (int)(Mathf.Pow(floorSize, 2) * density * texturePropDensityConst * floorRatio);

        for (int i = 0; i < numProps; i++)
        {
            if (floorLEDBuilder.TryGetOnFloorTexturePosition(out Vector3 position, out bool onEdge))
            {
                GameObject propPrefab = themeData.GetRandomTextureProp();
                GameObject propInstance = Instantiate(propPrefab, position, Quaternion.identity);

                if (onEdge && propInstance.TryGetComponent(out FloorTextureProp textureProp))
                { 
                    textureProp.SetOnEdge();
                }
                
                propInstance.transform.SetParent(transform);
                props.Add(propInstance);
            }
            else
            {
                break;
            }
        }

    }

    public void BuildPropsOnLiquid(ThemeData themeData, StageBlueprint blueprint)
    {
        int floorSize = blueprint.Size;
        float liquidRatio = 1 - floorLEDBuilder.FloorRatio;
        float density = themeData.OnLiquidPropDensity;
        int numProps = (int)(Mathf.Pow(floorSize, 2) * density * onLiquidPropDensityConst * liquidRatio);

        for (int i = 0; i < numProps; i++)
        {
            if (floorLEDBuilder.TryGetOnLiquidObjectPosition(out Vector3 position))
            {
                GameObject prop = Instantiate(themeData.GetRandomOnLiquidProp(), position, Quaternion.identity);
                prop.transform.SetParent(transform);
                props.Add(prop);
            }
            else
            {
                break;
            }
        }
    }

    public void ClearProps()
    {
        foreach (GameObject prop in props)
        {
            Destroy(prop);
        }
        props.Clear();
    }
}