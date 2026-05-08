using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;
using UnityEngine.UIElements;
using UnityStandardAssets.Utility;

public class CharSelectionScreen : MenuScreen
{
    [SerializeField]
    private CharacterManagerSO characterManager;
    
    public static event Action CharSelectButtonClicked;
    private const string selectButtonName = "button-select";
    private const string backButtonName = "button-back";
    
    //define charNameLabelName and charDescriptionLabelName here
    
    private const string charNameLabelName = "label-char-name";
    private const string cubieAbilityLabelName = "label-cubie-ability";
    private const string charDescriptionLabelName = "label-char-description";
    

    private Button selectButton, backButton;
    private Label charNameLabel, charDescriptionLabel, cubieAbilityLabel;

    private void OnEnable()
    {
        CharSelectionBar.CharPortraitClicked += OnCharPortraitClicked;
    }
    
    private void OnDisable()
    {
        CharSelectionBar.CharPortraitClicked -= OnCharPortraitClicked;
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        selectButton = screen.Q<Button>(selectButtonName);
        backButton = screen.Q<Button>(backButtonName);
        charNameLabel = screen.Q<Label>(charNameLabelName);
        cubieAbilityLabel = screen.Q<Label>(cubieAbilityLabelName);
        charDescriptionLabel = screen.Q<Label>(charDescriptionLabelName);
    }
    
    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        selectButton.RegisterCallback<ClickEvent>(OnClickSelectBtn);
        selectButton.RegisterCallback<KeyDownEvent>(OnKeyDownSelectBtn);
        selectButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitSelectBtn);
        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();
        selectButton.text = GetLocalizedString(UIText.BTN_SELECT);
        cubieAbilityLabel.text = GetLocalizedString(UIText.LBL_CUBIE_ABILITY);
        characterManager.GetCurrentCharData(out CharDataSO charData);
        charNameLabel.text = GetLocalizedCharName(charData.CharName);
        charDescriptionLabel.text = GetLocalizedCharDescription(charData.Description);
    }

    private void OnClickSelectBtn(ClickEvent evt)
    {
        CharSelectButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownSelectBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            CharSelectButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnSubmitSelectBtn(NavigationSubmitEvent evt)
    {
        CharSelectButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnCharPortraitClicked(int index)
    {
        if (characterManager.TryGetCharName(index, out string charName))
        {
            charNameLabel.text = GetLocalizedCharName(charName);
        }
        else
        {
            charNameLabel.text = "???";
        }
        
        if (characterManager.TryGetCharDescription(index, out string charDescription))
        {
            charDescriptionLabel.text = GetLocalizedCharDescription(charDescription);
        }
        else
        {
            charDescriptionLabel.text = "???";
        }

        FocusSelectButton();
        HandleButtonClickPositiveSFX();
    }

    private void OnClickBackBtn(ClickEvent evt)
    {
        mainMenuUIManager?.ShowHomeScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            mainMenuUIManager?.ShowHomeScreen();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        mainMenuUIManager?.ShowHomeScreen();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void BackToHomeScreen(NavigationCancelEvent evt)
    {
        mainMenuUIManager?.ShowHomeScreen();
    }

    //private void BackButtonToSelectButton(NavigationMoveEvent evt)
    //{
    //    switch (evt.direction)
    //    {
    //        case NavigationMoveEvent.Direction.Right:
    //            selectButton.Focus();
    //            break;
    //        case NavigationMoveEvent.Direction.Down:
    //            selectButton.Focus();
    //            break;
    //    }
    //}    

    public void FocusSelectButton()
    {
        selectButton.Focus();
    }

    private string GetLocalizedCharName(string charName)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CHARACTER_NAMES, charName );
    }

    private string GetLocalizedCharDescription(string charDescription)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_CHARACTER_DESCRIPTIONS, charDescription);
    }
}
