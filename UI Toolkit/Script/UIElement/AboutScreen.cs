using UnityEngine;
using UnityEngine.UIElements;
using System;
using UnityEngine.Localization.Settings;


public class AboutScreen : MenuScreen
{

    private Button termsOfSurviceButton, privacyPolicyButton, backButton;

    private const string backButtonName = "button-back";
    private const string termsOfSurviceButtonName = "button-terms-of-service";
    private const string privacyPolicyButtonName = "button-privacy-policy";

    [SerializeField]
    private string privacyPolicyURL;

    [SerializeField]
    private string termsOfServiceURL;

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        backButton = screen.Q<Button>(backButtonName);
        termsOfSurviceButton = screen.Q<Button>(termsOfSurviceButtonName);
        privacyPolicyButton = screen.Q<Button>(privacyPolicyButtonName);
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
      
        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);

        termsOfSurviceButton.RegisterCallback<ClickEvent>(OnClickTermsOfServiceBtn);
        termsOfSurviceButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitTermsOfServiceBtn);
        termsOfSurviceButton.RegisterCallback<KeyDownEvent>(OnKeyDownTermsOfServiceBtn);

        privacyPolicyButton.RegisterCallback<ClickEvent>(OnClickPrivacyPolicyBtn);
        privacyPolicyButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitPrivacyPolicyBtn);
        privacyPolicyButton.RegisterCallback<KeyDownEvent>(OnKeyDownPrivacyPolicyBtn);
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();
    }
    private void OnClickBackBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        GoBackToHomeScreen();
    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        GoBackToHomeScreen();
    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            GoBackToHomeScreen();
        }
    }

    private void OnClickPrivacyPolicyBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        ShowPrivacyPolicy();
    }

    private void OnSubmitPrivacyPolicyBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        ShowPrivacyPolicy();
    }

    private void OnKeyDownPrivacyPolicyBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            ShowPrivacyPolicy();
        }
    }


    private void OnClickTermsOfServiceBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        ShowTermsOfService();
    }

    private void OnSubmitTermsOfServiceBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();
        ShowTermsOfService();
    }

    private void OnKeyDownTermsOfServiceBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
            ShowTermsOfService();
        }
    }

    private void GoBackToHomeScreen()
    {
        HideScreen();
        mainMenuUIManager?.ShowHomeScreen();
    }

    private void ShowTermsOfService()
    {
        Application.OpenURL(termsOfServiceURL);
    }

    private void ShowPrivacyPolicy()
    {
        Application.OpenURL(privacyPolicyURL);
    }
}
