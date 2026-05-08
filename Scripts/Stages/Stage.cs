using Local.Scripts.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


public class Stage : FrameStage<Stage>, IColorable
{
    [SerializeField]
    private int size = 10;

    [SerializeField]
    private float pillarsFadeOutTime = 1f;


    [SerializeField]
    protected float defaultHeight;

    [FormerlySerializedAs("lowerHeight")]
    [SerializeField]
    protected float defaultLowerHeight;

    [SerializeField]
    private FloatEventChannelSO setFloorTimerEC;

    private List<ObjectFader> pillarFader = new List<ObjectFader>();
    private float height, lowerHeight;
    private float stageInterval = 13f;


    public int Size
    {
        get => size;
        set
        {          
            if (size == value)
            {
                return;
            }
        
            size = value;
            AdjustFrameSize(size);
            AdjustPillarPosition(size);
        }
    }

    public float Height
    {
        get => height;
        set
        {
            height = value;
            AdjustPillarHeight(height, lowerHeight);
        }
    }

    public float LowerHeight
    {
        get => lowerHeight;
        set
        {
            lowerHeight = value;
            AdjustPillarHeight(height, lowerHeight);
        }
    }

    public float StageInterval
    {
        set => stageInterval = value;
    }

    protected override void Awake()
    {
        base.Awake();
    
        foreach (Transform pillar in pillars)
        {
            pillarFader.Add(pillar.GetComponent<ObjectFader>());
        }

        AdjustPillarHeight(defaultHeight, defaultLowerHeight);
        AdjustFrameSize(size);
        AdjustPillarPosition(size);
    }

    protected override void OnDisable()
    {
        pillarFader.ForEach(item => item.gameObject.SetActive(true));
        AdjustPillarHeight(defaultHeight, defaultLowerHeight);
        base.OnDisable();
    }

    public void BeCurrentStage()
    {
        Height = 0f;
        gameObject.SetActive(true);
    }

    private void AdjustPillarPosition(int size)
    { 
        for (int i = 0; i < pillars.childCount; i++)
        {
            AdjustPillarPosition(pillars.GetChild(i), size);
        }
    }

    protected  void AdjustPillarHeight(float height, float lowerHeight)
    {
        this.height = height;
        this.lowerHeight = lowerHeight;

        float totalHeight = height + lowerHeight;
        pillars.localPosition = new Vector3(0, (height - lowerHeight) / 2, 0);

        foreach (Transform pillar in pillars)
        {
            pillar.localScale = new Vector3(1, totalHeight, 1);
        }
    }

    public void SetPillarHeight(float height, float lowerHeight)
    {
        this.height = height;
        this.lowerHeight = lowerHeight;
        AdjustPillarHeight(height, lowerHeight);
    }

    public void OnFinishStageMove()
    {
        if (stageType == StageType.MainStage)
        {
            setFloorTimerEC.Raise(stageInterval);
        }
    }

    public void OnPlayerFall()
    {
    }

    protected void AdjustPillarPosition(Transform pillar, int size)
    {
        pillar.localPosition = size * GetPillarSign(pillar);
    }

    public void SetColor(Color color)
    {

    }
}
