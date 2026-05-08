using UnityEngine;
using Local.Scripts.Extensions;
using System.Collections.Generic;


public class FloorLiquidBuilder : MonoBehaviour
{
    [SerializeField]
    private FloorLiquidContainerSO liquidContainer;

    [SerializeField]
    private SerializableDictionary<WaterType, OnLiquidBehaviour> onLiquidBehaviours;

    [SerializeField]
    private OnLiquidBehaviourChannel onLiquidBehaviourChannel;

    [SerializeField]
    private OnePureEffectSpawner boilingLavaSpawner, boilingAcidSpawner;

    [SerializeField]
    private FloorGeoDataChannel floorGeoDataChannel;

    private Renderer liquidRenderer;
    private FloorLEDBuilder floorLEDBuilder;
    private List<PureEffect> boilingEffects = new List<PureEffect>();

    private Dictionary<WaterType, OnLiquidBehaviour> onLiquidBehavioursDict;
    private BoxCollider boxCollider;

    private float liquidHeightAdjustment = 0.01f;
    private bool isLiquid = false;

    public bool IsLiquid => isLiquid;

    private void Awake()
    {
        liquidRenderer = GetComponent<Renderer>();
        floorLEDBuilder = transform.parent.GetComponentInChildren<FloorLEDBuilder>();
        DeactivateLiquid();
        boxCollider = GetComponent<BoxCollider>();
        onLiquidBehavioursDict = onLiquidBehaviours.ToDictionary();
    }

    private void OnEnable()
    {
        floorGeoDataChannel.RegisterLiquidBuilder(this);
    }

    private void OnDisable()
    {
        floorGeoDataChannel.UnregisterLiquidBuilder(this);
    }

    public void BuildAndTransformFloor(WaterType liquidType, int stageSize, Vector3 anchorPos)
    {
        ActivateLiquid();

        if (liquidContainer.TryGetLiquidMaterial(liquidType, out Material material))
        {
            liquidRenderer.material = material;
        }

        transform.position = anchorPos + Vector3.up * liquidHeightAdjustment;
        transform.localScale = ((float)(stageSize - 1) / 5) * Vector3.one;

        switch (liquidType)
        {
            case WaterType.Water:
                boxCollider.isTrigger = true;
                isLiquid = true;
                ResetEffects();
                break;
            case WaterType.AdvencedWater:
                boxCollider.isTrigger = true;
                isLiquid = true;
                ResetEffects();
                break;
            case WaterType.Lava:
                boxCollider.isTrigger = true;
                OnLava();
                break;
            case WaterType.Acid:
                boxCollider.isTrigger = true;
                OnAcid();
                isLiquid = true;
                break;
            case WaterType.Mud:
                boxCollider.isTrigger = true;
                isLiquid = true;
                break;
            case WaterType.Frozen:
                boxCollider.isTrigger = false;
                isLiquid = false;
                break;
            case WaterType.BolingSugar:
                boxCollider.isTrigger = true;
                OnBoilingSugar();
                break;

        }

        OnLiquidBehaviour lb;

        if (!onLiquidBehavioursDict.ContainsKey(liquidType))
        {
            lb = onLiquidBehavioursDict[WaterType.Water];
        }
        else
        {
            lb = onLiquidBehavioursDict[liquidType];

        }

        onLiquidBehaviourChannel.Register(lb);
    }

    public void DeactivateLiquid()
    {
        gameObject.SetActive(false);
    }

    public void ActivateLiquid()
    {
        gameObject.SetActive(true);
    }

    private void OnLava()
    {
      FillUpWithEffect(boilingLavaSpawner);
    }

    private void OnAcid()
    {
      FillUpWithEffect(boilingAcidSpawner);
    }

    private void OnBoilingSugar()
    {
        FillUpWithEffect(boilingLavaSpawner);
    }

    private void ResetEffects()
    {
        for (int i = 0; i < boilingEffects.Count; i++)
        {
            boilingEffects[i].Release();
        }

        boilingEffects.Clear();
    }

    private void FillUpWithEffect(OnePureEffectSpawner spawner)
    {
        ResetEffects();
        
        while (floorLEDBuilder.TryGetOnLiquidObjectPosition(out Vector3 position))
        {
            PureEffect effect = spawner.Spawn();
            position.y = transform.position.y;
            effect.transform.parent = transform;
            effect.transform.position = position;
            boilingEffects.Add(effect);
        }
    }

}