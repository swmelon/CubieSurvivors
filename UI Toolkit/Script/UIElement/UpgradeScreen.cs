using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.Localization.Settings;


public class UpgradeScreen : MenuScreen
{
    public event Action StatsButtonClicked;
    public event Action ItemsButtonClicked;

    private Button statsButton, itemsButton, backButton;

    private const string statsButtonName = "button-stats";
    private const string itemsButtonName = "button-items";
    private const string backButtonName = "button-back";

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        statsButton = screen.Q<Button>(statsButtonName);
        itemsButton = screen.Q<Button>(itemsButtonName);
        backButton = screen.Q<Button>(backButtonName);
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        statsButton.RegisterCallback<ClickEvent>(OnClickStatsBtn);
        statsButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitStatsBtn);
        statsButton.RegisterCallback<KeyDownEvent>(OnKeyDownStatsBtn);

        itemsButton.RegisterCallback<ClickEvent>(OnClickItemsBtn);
        itemsButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitItemsBtn);
        itemsButton.RegisterCallback<KeyDownEvent>(OnKeyDownItemsBtn);

        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);

        StatsButtonClicked += HideScreen;
        ItemsButtonClicked += HideScreen;
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();
        statsButton.text = GetLocalizedString(UIText.BTN_STATS);
        itemsButton.text = GetLocalizedString(UIText.BTN_ITEM);
    }

    private void OnClickStatsBtn(ClickEvent evt)
    {
        StatsButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
    }

    private void OnSubmitStatsBtn(NavigationSubmitEvent evt)
    {
        StatsButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownStatsBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            StatsButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickItemsBtn(ClickEvent evt)
    {
        ItemsButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitItemsBtn(NavigationSubmitEvent evt)
    {
        ItemsButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownItemsBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            ItemsButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickBackBtn(ClickEvent evt)
    {
        GoBackToHomeScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        GoBackToHomeScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            GoBackToHomeScreen();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void GoBackToHomeScreen()
    {
        mainMenuUIManager?.ShowHomeScreen();
    }
}
