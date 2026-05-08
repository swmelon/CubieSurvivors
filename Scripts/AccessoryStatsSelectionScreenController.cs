using Local.Scripts.Extensions;
using UnityEngine;
using System.Collections.Generic;
using System;

public class AccessoryStatsSelectionScreenController : MonoBehaviour
{
    public event Action AccessoryStatsSelected;

    [SerializeField]
    private AccessoryStatsDatabase accessoryStatsDatabase;

    [SerializeField]
    private CardSelectionScreenController cardSelectionScreenController;

    [SerializeField]
    private GameAccessoryManager gameAccessoryManager;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private CutSceneDirector cutSceneDirector;

    [SerializeField]
    private GameProcessManager gameProcessManager;

    [SerializeField]
    private GemManagerSO gemManager;

    [SerializeField]
    private MainMenuUIManager mainMenuUIManager;

    private int numCardsToSelect;
    private List<CardData> deck = new List<CardData>();

    private void Awake()
    {
        numCardsToSelect = saveLoadManager.SaveFile.numCardsToSelect;
    }

    /// <summary>
    /// Accessory : ������ ���� prefab
    /// </summary>
    /// <param name="accessory"></param>
    /// 

    // �� �Լ��� ���� ���� ���� ���� �������� �ʴ� ������ ������ִ� Ŭ������ �����
    public bool ShowAccessoryStatsSelectionScreen(Accessory accessory, AccessoryRank accessoryRank=AccessoryRank.Common)
    {
        // �������� ���� �ʴ� ���� ������ �� ���°�?
        // ���� �̺�Ʈ ������������ ���� �Ǽ������� ���� �ʴ´ٸ� �׷���.
        List<int> pendingSelectionCards = saveLoadManager.SaveFile.pendingSelectionCards;

        if (pendingSelectionCards.Count > 0)
        {
            // �߻��ϸ� �ȵ�. ���ο� AccStatsSelectionScreen�� ���� ���� ���� ���� ó���ؾ� ��.
            // ������ ���� �ٸ� ����� �߰��Ǿ� �̷� ���� �߻��� �� �ִٸ�,
            // ���⼭ ���� ��쵵��
            Debug.LogError("Pending selection cards exist.");
        }

        deck.Clear();
        cardSelectionScreenController.ShowCardSelectionScreen(true, UIText.OK, OnButtonClickedAfterCardSelection);
        CardSelectionScreen.CardClicked += ClearPendingQueue;

        int numAccCards = numCardsToSelect;
        List<AccStats> chosenStats = accessoryStatsDatabase.GetRandomAccessoryStats(accessory, accessoryRank, numAccCards);

        if (chosenStats.Count == 0)
        {
            cardSelectionScreenController.HideScreen();
            return false;
        }

        List<AccData> accDatas = new List<AccData>();

        foreach (var stats in chosenStats)
        {
            AccData accData = new AccData(accessory, stats);
            pendingSelectionCards.Add(accData.saveID);
            accDatas.Add(accData);
        }

        saveLoadManager.Save();

        foreach (var accData in accDatas)
        {
            deck.Add(new AccessoryCardData(accData, gameAccessoryManager, AccessoryCardData.InteractionMode.GetNew));
        }

        deck.FisherShuffle();
        ShowCardsInDeck();
        return true;
    }

    public void ShowPendingSelectionScreen()
    {
        if (!saveLoadManager.HasPendingCardSelection())
        {
            Debug.LogError("No pending selection but ShowPendingSelectionScreen is called.");
            return;
        }

        List<int> saveIDs = saveLoadManager.SaveFile.pendingSelectionCards;

        deck.Clear();
        cardSelectionScreenController.ShowCardSelectionScreen(true, UIText.OK, OnButtonClickedAfterCardSelection);
        CardSelectionScreen.CardClicked += ClearPendingQueue;
        AccessoryStatsSelected += ShowHomeScreen;

        foreach (var saveID in saveIDs)
        {
            if (!gameAccessoryManager.TryGetAccDataBySaveID(saveID, out AccData accData))
            {
                Debug.LogError($"Pending save ID {saveID} is invalid.");
                continue;
            }

            deck.Add(new AccessoryCardData(accData, gameAccessoryManager,
                AccessoryCardData.InteractionMode.GetNew));
        }

        ShowCardsInDeck();
    }

    private void OnButtonClickedAfterCardSelection()
    {
        // �ƾ����� ������, �����ϰ� �� ���ε� �Ұ���?
        // �̰� �ó����� �Ŵ����� ����

        AccessoryStatsSelected?.Invoke();
        AccessoryStatsSelected = null;
    }

    private void ShowCardsInDeck()
    {
        foreach (var cardData in deck)
        {
            cardSelectionScreenController.ShowCard(cardData);
        }
    }

    private void ClearPendingQueue()
    {
        saveLoadManager.SaveFile.pendingSelectionCards.Clear();
        saveLoadManager.Save();

        CardSelectionScreen.CardClicked -= ClearPendingQueue;
    }

    private void ShowHomeScreen()
    {
        mainMenuUIManager.ShowHomeScreen();
        CardSelectionScreen.CardClicked -= ShowHomeScreen;
    }
}