
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Local.Scripts.Extensions;
using UnityEngine.Serialization;
using FMODUnity;


[CreateAssetMenu(fileName = "ThemeData", menuName = "ScriptableObjects/Stage/ThemeData", order = SOAssetMenuIndex.Stage)]
public class ThemeData : ScriptableObject, IDependentInitialization
{
    public bool nonCombatTheme;
    public StageData MainStageData;
    public StageAdapterData StageAdapterData;
    public FloorBlockSet FloorBlockSet;  

    public WaterType liquidType;
    public AmbientData ambientData;

    [SerializeField]
    private AmbientDataChannelSO ambientDataChannel;

    public EventReference bgm;
    public EventReference[] playlist;


    [Range(0, 5)]
    public int randomHeightMapPadding = 4;

    [Range(0.3f, 0.7f)]
    public float randomHeightMapThreshold = 0.5f;

    [Range(0f, 1f)]
    public float localHeightMapProbability = 0f;
    public List<FloorGridDataSO> localHeightMaps;
    public List<FloorGridDataSO> bossLocalHeightMaps;


    [Range(0f, 1f)]
    public float trapDensity = 0.2f;

    public List<TrapType> trapTypes;


    [Range(0f, 1f)]
    public float PropDensity = 0.2f;

    [FormerlySerializedAs("Propss")]
    public SerializableDictionary<GameObject, int> props;

    private List<GameObject> propList;
    

    [Range(0f, 1f)]
    public float TexturePropDensity = 0.2f;
    public List<GameObject> TextureProps;

    [Range(0f, 1f)]
    public float OnLiquidPropDensity = 0.2f;
    public List<GameObject> OnliquidProps;

    // ingredients
    private Dictionary<GameObject, float> propsDict = new Dictionary<GameObject, float>();

    public void Initialize()
    {
        InitializePropList();
        RaiseAmbientData();
    }

    public void RaiseAmbientData()
    {
        ambientDataChannel.Register(ambientData);
    }

    public GameObject GetRandomProp()
    {
        return propList.PickRandom();
    }

    public GameObject GetRandomTextureProp()
    {
        return TextureProps.PickRandom();
    }

    public GameObject GetRandomOnLiquidProp()
    {
        return OnliquidProps.PickRandom();
    }

    private void InitializePropList()
    {
        propList = new List<GameObject>();

        foreach (var prop in props.ToDictionary())
        {
            for (int i = 0; i < prop.Value; i++)
            {
                propList.Add(prop.Key);
            }
        }
    }

    
}
