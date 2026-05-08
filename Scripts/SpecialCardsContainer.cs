using UnityEngine;
using System.Collections.Generic;
using System;
using Local.Scripts.Extensions;


[CreateAssetMenu(menuName = "ScriptableObjects/DataContainer/SpecialCardsContainer")]
public class SpecialCardsContainer : ScriptableObject, IDependentInitialization
{
    [Header("Kill All Enemies")]
    [SerializeField]
    private EventChannelSO killAllEnemiesEC;

    [SerializeField]
    private FXCameraChannelSO killAllFXCameraChannel;

    [SerializeField]
    private float killAllProbability = 0.2f;

    [Header("Take Emergency Health Pack")]
    [SerializeField]
    private EventChannelSO takeHealthPackEC;

    [SerializeField]
    private Sprite takeHealthPackIcon;

    [SerializeField]
    private SFXTags healthPackSFX;

    [SerializeField]
    private float healthPackProbability = 0.8f;

    private FXCameraController killAllFXCameraController;

    private SpecialCardData killAllCardData;
    private SpecialCardData takeHealthPackCardData;

    public void Initialize()
    {
       AddDatasNotUsingFXCam();
    }

    public bool TryGetRandomSpecialCardData(out SpecialCardData specialCardData)
    {
        specialCardData = null;

        if (killAllCardData == null && takeHealthPackCardData == null)
        {
            return false;
        }

        RefreshCards();

        float randomValue = RandomExtenstion.GetFloatInRange(0f, 1f);

        if (randomValue <= killAllProbability)
        {
            specialCardData = killAllCardData;
        }
        else if (randomValue <= healthPackProbability)
        {
            specialCardData = takeHealthPackCardData;
        }

        return specialCardData != null;
    }

    private void AddDatasNotUsingFXCam()
    {
        Action takeHealthPackAction = OnTakeHealthPack;
        IconizedAction iconizedTakeHealthPackAction = new IconizedAction(takeHealthPackAction, takeHealthPackIcon,
            CardText.EMERGENCY_HEALTH_PACK);

        takeHealthPackCardData = new SpecialCardData(iconizedTakeHealthPackAction);
    }

    private void OnEnable()
    {
        killAllFXCameraChannel.Subscribe(SetKillAllFXCamera);
    }

    private void OnDisable()
    {
        killAllFXCameraChannel.Unsubscribe(SetKillAllFXCamera);
    }

    private void SetKillAllFXCamera(FXCameraController killAllFXCameraController)
    {
        // null�� �ƴ� ��� ȣ��Ǵ� Ƚ���� 1ȸ�� ����Ǿ���Ѵ�. �׷��� specialCardDatas�� �ϳ��� �߰���
        this.killAllFXCameraController = killAllFXCameraController;

        if (ReferenceEquals(killAllFXCameraController, null))
        {
            return;
        }

        Action killAllEnmiesAction = () => { killAllEnemiesEC.Raise(); };
        IconizedAction iconizedkillAllEnmiesAction = new IconizedAction(killAllEnmiesAction, null,
            CardText.KILL_ALL_ENEMIES);

        iconizedkillAllEnmiesAction.SetParticleIconCam(killAllFXCameraController);

        killAllCardData = new SpecialCardData(iconizedkillAllEnmiesAction);
    }
    
    private void OnTakeHealthPack()
    {
        takeHealthPackEC.Raise();
        FMODAudioManager.instance.PlayOneShot(healthPackSFX);
    }

    private void RefreshCards()
    {
        killAllCardData.Refresh();
        takeHealthPackCardData.Refresh();
    }
}