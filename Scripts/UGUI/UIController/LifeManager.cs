using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class LifeManager : MonoBehaviour
{
    [SerializeField] private PlayerChannelSO playerChannel; // Serialized reference to the player channel

    [SerializeField]
    private EventChannelSO playerDeadEC;

    [SerializeField]
    private EventChannelSO startNewGameEC;

    [SerializeField]
    private EventChannelSO playerFallEC;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private InterstitialAd interstitialAd;

    [SerializeField]
    private PopupScreenController popupScreenController;

    [SerializeField]
    private ScoreScreenController scoreScreenController;

    [SerializeField]
    private LifeDisplay lifeDisplayer;


    private Player player;
    private int initialLife = 1;


    private void OnEnable()
    {
        playerDeadEC.SubscribeLast(OnPlayerDead);
        playerChannel.Subscribe(SetPlayer);
        startNewGameEC.Subscribe(SetStartLife);
        playerFallEC.Subscribe(ShowScoreScreen);
    }

    private void OnDisable()
    {
        playerDeadEC.Unsubscribe(OnPlayerDead);
        playerChannel.Unsubscribe(SetPlayer);
        startNewGameEC.Unsubscribe(SetStartLife);
        playerFallEC.Unsubscribe(ShowScoreScreen);
    }



    public void AddLife()
    {
        lifeDisplayer.AddLife();
    }

    private void SetStartLife()
    {
        lifeDisplayer.SetMaxLife(initialLife);

        if (saveLoadManager.AdsRemoved())
        {
            AddLife();
        }

        for (int i = 0; i < saveLoadManager.SaveFile.ExtraLifeLevel; i++)
        {
            AddLife();
        }
    }    

    private void SetPlayer(Player newPlayer)
    {
        player = newPlayer;
    }

    private void OnPlayerDead()
    {
        lifeDisplayer.ConsumeLife();

        if (lifeDisplayer.LifeCount <= 0)
        {
            if (!saveLoadManager.AdsRemoved() && interstitialAd.Loadable())
            { 
                StartCoroutine(ShowPopupScreenOneSecLater());
            }
            else
            {
                ShowScoreScreen();
            }
        }
        else
        {
            player.ExplodeAndRevive();
        }
    }

    private IEnumerator ShowPopupScreenOneSecLater()
    {
        yield return new WaitForSeconds(1f);
        popupScreenController.ShowPopupScreen(UIText.WATCH_ADS_FOR_FREE_LIFE, UIText.YES, UIText.NO, OnWatchADButtonClicked, ShowScoreScreen, 0.4f);
    }

    private void ShowScoreScreen()
    {
        scoreScreenController.ShowScoreScreen(UIText.CONTINUE , GoBackToMainMenu);
    }

    private void GoBackToMainMenu()
    {
        saveLoadManager.Save();
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void OnWatchADButtonClicked()
    {
        interstitialAd.TryLoadAndShow(ReviveAfterWatchAD, ShowScoreScreen);
    }

    private void ReviveAfterWatchAD()
    {
        interstitialAd.AdsShowCompleted -= ReviveAfterWatchAD;
        AddLife();
        playerChannel.Revive();
    }
}
