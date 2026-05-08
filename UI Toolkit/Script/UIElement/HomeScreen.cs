using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

public class HomeScreen : MenuScreen
{
    private const string startButtonName = "start-button";
    private const string collectionButtonName = "upgrade-button";
    private const string settingsButtonName = "settings-button";

    private const string difficultyButtonName = "button-difficulty";
    private const string aboutButtonName = "button-about";

    [SerializeField]
    private DifficultySettingsScreen difficultySettingsScreen;

    [SerializeField]
    private AboutScreen aboutScreen;

    private Button startButton;
    private Button upgradeButton;
    private Button settingsButton;
    private Button difficultyButton;
    private Button aboutButton;
    
    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        startButton = screen.Q<Button>(startButtonName);
        upgradeButton = screen.Q<Button>(collectionButtonName);
        settingsButton = screen.Q<Button>(settingsButtonName);

        difficultyButton = screen.Q<Button>(difficultyButtonName);
        aboutButton = screen.Q<Button>(aboutButtonName);
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        
        startButton.RegisterCallback<ClickEvent>(OnClickStartBtn);
        startButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitStartBtn);
        startButton.RegisterCallback<KeyDownEvent>(OnKeyDownStartBtn);
        
        upgradeButton.RegisterCallback<ClickEvent>(OnClickUpgradeBtn);
        upgradeButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitUpgradeBtn);
        upgradeButton.RegisterCallback<KeyDownEvent>(OnKeyDownUpgradeBtn);

        settingsButton.RegisterCallback<ClickEvent>(OnClickSettingsBtn);
        settingsButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitSettingsBtn);
        settingsButton.RegisterCallback<KeyDownEvent>(OnKeyDownSettingsBtn);

        difficultyButton.RegisterCallback<ClickEvent>(OnClickDifficultyBtn);

        aboutButton.RegisterCallback<ClickEvent>(OnClickAboutBtn);
        aboutButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitAboutBtn);
        aboutButton.RegisterCallback<KeyDownEvent>(OnKeyDownAboutBtn);
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();

        startButton.text = GetLocalizedString(UIText.BTN_START);
        upgradeButton.text = GetLocalizedString(UIText.BTN_UPGRADE);
        settingsButton.text = GetLocalizedString(UIText.BTN_SETTINGS);
    }

    private void OnClickStartBtn(ClickEvent evt)
    {
        //ActivateButton(m_CharScreenMenuButton);
        mainMenuUIManager?.ShowCharScreen();
        evt.StopPropagation();
        //ClickMarker(evt);

        HandleButtonClickPositiveSFX();
    }

    private void OnSubmitStartBtn(NavigationSubmitEvent evt)
    {
        mainMenuUIManager?.ShowCharScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownStartBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            mainMenuUIManager?.ShowCharScreen();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }
    
    private void OnClickUpgradeBtn(ClickEvent evt)
    {
        //ActivateButton(m_InfoScreenMenuButton);
        mainMenuUIManager?.ShowCollectionScreen();
        evt.StopPropagation();
        //ClickMarker(evt);
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitUpgradeBtn(NavigationSubmitEvent evt)
    {
        mainMenuUIManager?.ShowCollectionScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownUpgradeBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            mainMenuUIManager?.ShowCollectionScreen();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickSettingsBtn(ClickEvent evt)
    {
        //ActivateButton(m_InfoScreenMenuButton);
        mainMenuUIManager?.ShowSettingsScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
    }

    private void OnSubmitSettingsBtn(NavigationSubmitEvent evt)
    {
        mainMenuUIManager?.ShowSettingsScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownSettingsBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            mainMenuUIManager?.ShowSettingsScreen();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }
    

    private void OnClickDifficultyBtn(ClickEvent evt)
    {
        HideScreen();
        difficultySettingsScreen.ShowScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
    }

    private void OnClickAboutBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        ShowAbout();
    }

    private void OnSubmitAboutBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        ShowAbout();
    }

    private void OnKeyDownAboutBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            ShowAbout();
        }
    }

    private void ShowAbout()
    {
        HideScreen();
        aboutScreen.ShowScreen();
    }
}
