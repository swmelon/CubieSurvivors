using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;

public class PauseMenuScreen : MenuScreen
{
    public event Action ResumeButtonClicked;
    public event Action SettingsButtonClicked;
    public event Action RestartButtonClicked;
    public event Action QuitButtonClicked;

    [SerializeField] private BooleanEventChannelSO pauseMenuUiControlChannel;
    [SerializeField] private GamePauser gamePauser; 

    private const string resumeButtonName = "resume-button";
    private const string settingsButtonName = "settings-button";
    private const string restartButtonName = "restart-button";
    private const string quitButtonName = "quit-button";
    
    private Button resumeButton;
    private Button settingsButton;
    private Button restartButton;
    private Button quitButton;

    // 다른 screen이 띄워져있다면, 임시로 가리고 pause menu가 끝났을 때 띄워줘야한다.
    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        resumeButton = screen.Q<Button>(resumeButtonName);
        settingsButton = screen.Q<Button>(settingsButtonName);
        restartButton = screen.Q<Button>(restartButtonName);
        quitButton = screen.Q<Button>(quitButtonName);
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        
        resumeButton.RegisterCallback<ClickEvent>(OnClickResumeBtn);
        resumeButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitResumeBtn);
        resumeButton.RegisterCallback<KeyDownEvent>(OnKeyDownResumeBtn);

        settingsButton.RegisterCallback<ClickEvent>(OnClickSettingsBtn);
        settingsButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitSettingsBtn);
        settingsButton.RegisterCallback<KeyDownEvent>(OnKeyDownSettingsBtn);

        restartButton.RegisterCallback<ClickEvent>(OnClickRestartBtn);
        restartButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitRestartBtn);
        restartButton.RegisterCallback<KeyDownEvent>(OnKeyDownRestartBtn);

        quitButton.RegisterCallback<ClickEvent>(OnClickQuitBtn);
        quitButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitQuitBtn);
        quitButton.RegisterCallback<KeyDownEvent>(OnKeyDownQuitBtn);
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();

        resumeButton.text = GetLocalizedString(UIText.BTN_RESUME);
        settingsButton.text = GetLocalizedString(UIText.BTN_SETTINGS);
        restartButton.text = GetLocalizedString(UIText.BTN_RESTART);
        quitButton.text = GetLocalizedString(UIText.BTN_QUIT);
    }

    private void OnClickResumeBtn(ClickEvent evt)
    {
        ResumeButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitResumeBtn(NavigationSubmitEvent evt)
    {
        ResumeButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownResumeBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            ResumeButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }
    
    private void OnClickSettingsBtn(ClickEvent evt)
    {
        SettingsButtonClicked?.Invoke();
        HandleButtonClickPositiveSFX();

        //ActivateButton(m_InfoScreenMenuButton);
        //ClickMarker(evt);
    }

    private void OnSubmitSettingsBtn(NavigationSubmitEvent evt)
    {
        SettingsButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownSettingsBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            SettingsButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickRestartBtn(ClickEvent evt)
    {
        RestartButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitRestartBtn(NavigationSubmitEvent evt)
    {
        RestartButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownRestartBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            RestartButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickQuitBtn(ClickEvent evt)
    {
        QuitButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitQuitBtn(NavigationSubmitEvent evt)
    {
        QuitButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownQuitBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            QuitButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }
}
