using UnityEngine;
using AYellowpaper.SerializedCollections;
using System;
using UnityEngine.UIElements;
using UnityEngine.Serialization;
using UnityEngine.TextCore.Text;


[CreateAssetMenu(fileName = "CardSetupManager", menuName = "ScriptableObjects/CardSetupManager", order = 1)]
public class CardSetupManager : ScriptableObject
{
    [SerializeField]
    private SerializedDictionary<CardBackPlate, Sprite> cardBackPlate;

    [SerializeField]
    private SerializedDictionary<CardType, CardBackPlate> cardTypeAndBackPlate;

    [SerializeField]
    private SerializedDictionary<AccessoryRank, AccessoryCardThemeData> accessoryCardTheme;

    [SerializeField]
    private VisualTreeAsset cardTemplate;

    [SerializeField]
    private VisualTreeAsset accessoryCardTemplate;

    [SerializeField]
    private VisualTreeAsset accStatsControlCard;

    [SerializeField]
    private FontManager fontManager;


    public VisualElement SetupCard(CardData cardData, Action<VisualElement> callback)
    {
        VisualElement card;

        if (cardData is AccessoryCardData)
        {
            card = accessoryCardTemplate.CloneTree();
            AccessoryCardData accessoryCardData = cardData as AccessoryCardData;
            AccessoryRank rank = accessoryCardData.GetRank();

            AccessoryCardThemeData themeData = accessoryCardTheme[rank];
            accessoryCardData.SetCardVisualAndCallback(card, cardBackPlate[themeData.CardBackPlate],
                callback, themeData.TintColor, themeData.TextColor);
        }
        else
        {
            card = cardTemplate.CloneTree();
            cardData.SetCardVisualAndCallback(card, GetBackPlate(cardData.GetCardType()), callback);
        }

        //�Ǽ����� ī��� ���� ī�� ���ø��� ����ؾ���
        card.userData = cardData;
        cardData.SetFont(fontManager.GetCurrentCardStyleFont());
        return card;
    }

    public VisualElement SetupAccStatsControlCard(AccStatsControlCardData data, Action<AccStatsControlCardData> callback=null)
    {
        VisualElement card = accStatsControlCard.CloneTree();

        AccessoryRank rank = data.GetRank();
        AccessoryCardThemeData themeData = accessoryCardTheme[rank];
        data.SetCardVisualAndCallback(card, cardBackPlate[themeData.CardBackPlate], themeData.TintColor, themeData.TextColor, OnAccStatsChanged, callback);
        data.SetFont(fontManager.GetCurrentCardStyleFont());
        return card;
    }

    private Sprite GetBackPlate(CardType type)
    {
        return cardBackPlate[cardTypeAndBackPlate[type]];
    }

    public Color GetRankThemeColor(int rank)
    {
        return accessoryCardTheme[(AccessoryRank)rank].TintColor;
    }

    private void OnAccStatsChanged(AccStatsControlCardData data)
    {
        AccessoryRank rank = data.GetRank();
        AccessoryCardThemeData themeData = accessoryCardTheme[rank];
        data.SetCardVisualOnly(data.Card, cardBackPlate[themeData.CardBackPlate], themeData.TintColor, themeData.TextColor);
    }
}
