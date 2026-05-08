
using System;
using UnityEngine;

public class ScoreScreenController : MonoBehaviour
{
    [SerializeField]
    private ScoreScreen scoreScreen;

    [SerializeField]
    private MainMenuUIManager mainMenuUIManager;

    [SerializeField]
    private IntChannelSO damageCountChannel, upgradeCountChannel, bossCountChannel, timeCountChannel;

    private Action action;

    private void OnEnable()
    {
        scoreScreen.BottomButtonClicked += OnBottomButtonClicked;
    }
    
    private void OnDisable()
    {
        scoreScreen.BottomButtonClicked -= OnBottomButtonClicked;
    }

    private void OnBottomButtonClicked()
    {
        action?.Invoke();
        scoreScreen.HideScreen();
    }
    
    public void ShowScoreScreen(string buttonText, Action postAction = null)
    {
        action = postAction;
        scoreScreen.SetupScreen(damageCountChannel.Value, upgradeCountChannel.Value, bossCountChannel.Value, timeCountChannel.Value, buttonText);
        scoreScreen.ShowScreen();
    }
}
