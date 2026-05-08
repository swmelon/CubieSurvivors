using Local.Scripts.Extensions;
using System.Collections.Generic;
using UnityEngine;

public class StageRewardScreenController : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField]
    private float lockedWeaponChance = 0.2f;

    [SerializeField]
    private CardSelectionScreenController cardSelectionScreenController;

    [SerializeField]
    private GameAccessoryManager gameAccessoryManager;

    [SerializeField]
    private AccRankProbContainer accRankProbContainer;

    [SerializeField]
    private DifficultyCurveManagerSO difficultyCurveManager;

    [SerializeField]
    private GameWeaponManagerSO gameWeaponManager;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private EventChannelSO getNewAccEC;

    [SerializeField]
    private CutSceneDirector cutSceneDirector;

    [SerializeField]
    private GameProcessManager gameProcessManager;

    [SerializeField]
    private GemManagerSO gemManager;

    [SerializeField]
    private AccessoryStatsDatabase accStatsDatabase;

    private int numCardsToSelect;
    private List<CardData> deck = new List<CardData>();

    private void Awake()
    {
        numCardsToSelect = saveLoadManager.SaveFile.numCardsToSelect;
    }

    public void ShowStageRewardScreen()
    {
        deck.Clear();
        cardSelectionScreenController.ShowCardSelectionScreen(true, UIText.CONTINUE, OnButtonClickedAfterCardSelection);
        // ī�带 ��� �����ұ�?

        // 1. ����� ���Ⱑ ������
        // 2. ������ -> �Ǽ��縮�� �ֱ�

        int numAccCards = numCardsToSelect - deck.Count;

        // numUnlockWeaponCards ��ŭ�� �Ǽ����� ����


        List<Accessory> chosenAccessories = gameAccessoryManager.AccessoriesOnSale;

        chosenAccessories.FisherShuffle();

        if (chosenAccessories.Count < numAccCards)
        {
            numAccCards = chosenAccessories.Count;
        }

        chosenAccessories = chosenAccessories.GetRange(0, numAccCards);



        foreach (var accessory in chosenAccessories)
        {
            DifficultyLevel difficultyLevel = difficultyCurveManager.CurrentDifficultyLevel;
            AccessoryRank rank = accRankProbContainer.GetRandomRank(difficultyLevel);
            
            List<AccStats> stats = accStatsDatabase.GetRandomAccessoryStats(accessory, rank, 1);

            if (stats.Count == 0)
            {
                continue;
            }

            deck.Add(new AccessoryCardData(new AccData(accessory, stats[0]), gameAccessoryManager,
                AccessoryCardData.InteractionMode.GetNew));
        }

        if (deck.Count == 0)
        {
            deck.Add(new CoinCardData(gemManager));
        }

        deck.FisherShuffle();

        foreach (var cardData in deck)
        {
            cardSelectionScreenController.ShowCard(cardData);
        }
    }

    private void OnButtonClickedAfterCardSelection()
    {
        cardSelectionScreenController.ShowCardSelectionScreen(false);
        cutSceneDirector.StartDefeatFinalBossCutScene();
        getNewAccEC?.Raise();
    }
}
