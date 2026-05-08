using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UpgradeCanvasController : MonoBehaviour
{
    [SerializeField] 
    private List<GameObject> upgradeEffects;

    [SerializeField]
    private BooleanEventChannelSO upgradeCanvasControlChannel;
    
    [SerializeField]
    private TransformChannelSO upgradeCanvasTransformChannel;
    
    [FormerlySerializedAs("characterStatUpgradeUI")] [SerializeField]
    private GameObject permanentStatUpgradeUI;
    
    [SerializeField]
    private AudioClip upgradeSound;

    private Canvas canvas;
    
    private void Awake()
    {
        permanentStatUpgradeUI.SetActive(false);
        canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        upgradeCanvasTransformChannel.Register(transform);
        upgradeCanvasControlChannel.Subscribe(SetCanvas);
    }

    private void OnDisable()
    {
        upgradeCanvasTransformChannel.Unregister(transform);
        upgradeCanvasControlChannel.Unsubscribe(SetCanvas);
    }

    private void SetCanvas(bool value)
    {
        canvas.enabled = value;

        if (value)
        {
            upgradeEffects.ForEach(go => go.SetActive(true));
            SoundManager.Instance.PlaySound(upgradeSound);
        }
        else
        {
            upgradeEffects.ForEach(go => go.SetActive(false));
        }
    }
    
    private void SetCharacterStatsUpgradeUI(bool value)
    {
        if (!value ^ canvas.enabled)
        {
            Debug.LogError("Cannot open characterAbillity stat upgrade UI when upgrade canvas is open");
            return;
        }
        
        SetCanvas(value);
        permanentStatUpgradeUI.SetActive(value);
    }
}
