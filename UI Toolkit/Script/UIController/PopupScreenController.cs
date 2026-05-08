
using System;
using UnityEngine;

public class PopupScreenController : MonoBehaviour
{
    [SerializeField]
    private PopupScreen popupScreen;

    [SerializeField]
    private MainMenuUIManager mainMenuUIManager;

    private Action leftAction, rightAction;

    private void OnEnable()
    {
        popupScreen.LeftButtonClicked += OnLeftButtonClicked;
        popupScreen.RightButtonClicked += OnRightButtonClicked;
    }
    
    private void OnDisable()
    {
        popupScreen.LeftButtonClicked -= OnLeftButtonClicked;
        popupScreen.RightButtonClicked -= OnRightButtonClicked;
    }

    private void OnLeftButtonClicked()
    {
        leftAction?.Invoke();
        popupScreen.HideScreen();
    }

    private void OnRightButtonClicked()
    {
        rightAction?.Invoke();
        popupScreen.HideScreen();
    }
    
    public void ShowPopupScreen(string noticeText, string leftButtonText, string rightButtonText, Action leftAction = null, Action rightAction = null, float alpha = 1f)
    {
        popupScreen.SetupScreen(noticeText, leftButtonText, rightButtonText, alpha);
        this.leftAction = leftAction;
        this.rightAction = rightAction;

        popupScreen.ShowScreen();
    }

    public void ShowPopupScreen(string noticeText, string buttonText, Action action = null, float alpha = 1f)
    {
        popupScreen.SetupScreen(noticeText, buttonText, alpha);
        this.leftAction = action;

        popupScreen.ShowScreen();
    }
}
