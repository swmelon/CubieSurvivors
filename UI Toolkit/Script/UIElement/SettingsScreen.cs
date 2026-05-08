
using MyUILibrary;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;

public class SettingsScreen : MenuScreen
{
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    private const string backButtonName = "button-back";
    private const string sliderMusicName = "slider-music";
    private const string sliderSFXName = "slider-sfx";
    private const string musicLabelName = "label-music";
    private const string sfxLabelName = "label-sfx";
    private const string joystickPosLabelName = "label-joystick-position";
    private const string hapticLabelName = "label-haptic";
    private const string graphicsQualityLabelName = "label-graphics-quality";
    private const string languageLabelName = "label-language";


    private const string graphicsQualityDropdownName = "CustomEnumField";
    private const string languageDropdownName = "MyDropdown";

    public event Action BackButtonClicked;
    public event Action<bool> JoystickPositionToggled, JoystickPositionLoaded;
    public event Action<bool> VibrationToggled, HapticLoaded;
    public event Action<float> MusicVolumeChanged, MusicVolumeLoaded;
    public event Action<float> SFXVolumeChanged, SFXVolumeLoaded;
    public event Action<GraphicQuality> GraphicsQualityChanged, GraphicsQualityLoaded;

    private SlideToggleControlJoystickPos slideToggleJ;
    private SlideToggleControlVibration slideToggleV;
    private Button backButton;
    private VisualElement sliderMusic;
    private VisualElement sliderSFX;
    private DropdownField languageDropdown, graphicsQualityDropdown;
    private Label musicLabel, sfxLabel, joystickPosLabel, hapticLabel, graphicsQualityLabel, languageLabel;
    public bool ShownByHomeScreen => shownByHomeScreen;
    private bool shownByHomeScreen;
    private List<KeyValuePair<string, string>> sortedLanguages;
    private List<string> localizedGraphicsSettings;

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        backButton = screen.Q<Button>(backButtonName);
        slideToggleJ = screen.Q<SlideToggleControlJoystickPos>();
        slideToggleV = screen.Q<SlideToggleControlVibration>();
        sliderMusic = screen.Q<VisualElement>(sliderMusicName);
        sliderSFX = screen.Q<VisualElement>(sliderSFXName);
        //graphicsQualityDropdown = screen.Q<VisualElement>(graphicsQualityDropdownName);

        List<DropdownField> dropdowns = screen.Query<DropdownField>(languageDropdownName).ToList();
        graphicsQualityDropdown = dropdowns[1];
        languageDropdown = dropdowns[0];

        musicLabel = screen.Q<Label>(musicLabelName);
        sfxLabel = screen.Q<Label>(sfxLabelName);
        joystickPosLabel = screen.Q<Label>(joystickPosLabelName);
        hapticLabel = screen.Q<Label>(hapticLabelName);
        graphicsQualityLabel = screen.Q<Label>(graphicsQualityLabelName);
        languageLabel = screen.Q<Label>(languageLabelName);

        sortedLanguages = LanguageManager.GetSortedLanguages();
        languageDropdown.choices = sortedLanguages.ConvertAll(x => x.Value);

        if (saveLoadManager.TryGetLocale(out string locale))
        {
            languageDropdown.index = sortedLanguages.FindIndex(x => x.Key == locale);
        }
        else
        {
            languageDropdown.index = 0;
        }
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);

        slideToggleJ.ValueChanged += (val) => JoystickPositionToggled?.Invoke(val);
        slideToggleV.ValueChanged += (val) => VibrationToggled?.Invoke(val);

        sliderMusic.RegisterCallback<ChangeEvent<float>>(evt => MusicVolumeChanged?.Invoke(evt.newValue));
        sliderSFX.RegisterCallback<ChangeEvent<float>>(evt => SFXVolumeChanged?.Invoke(evt.newValue));
        //graphicsQualityDropdown.RegisterCallback<ChangeEvent<Enum>>(OnGrapicQualityDropdownSelected);

        graphicsQualityDropdown.RegisterCallback<ChangeEvent<string>>(OnGraphicsDropdownSelected);
        languageDropdown.RegisterCallback<ChangeEvent<string>>(OnLanguageDropdownSelected);
        //sliderSFX.RegisterCallback<ChangeEvent<float>>(OnSFXVolumeSliderValueChanged);

        SaveFile saveFile = saveLoadManager.SaveFile;

        slideToggleJ.value = saveFile.RightJoystick;
        slideToggleV.value = saveFile.Vibration;
        sliderMusic.Q<Slider>().value = saveFile.MusicVolume;
        sliderSFX.Q<Slider>().value = saveFile.SFXVolume;
        graphicsQualityDropdown.index = saveFile.GraphicsQuality;


        // invoke the event to set initial values

        JoystickPositionLoaded?.Invoke(saveFile.RightJoystick);
        HapticLoaded?.Invoke(saveFile.Vibration);
        MusicVolumeLoaded?.Invoke(saveFile.MusicVolume);
        SFXVolumeLoaded?.Invoke(saveFile.SFXVolume);
        GraphicsQualityLoaded?.Invoke((GraphicQuality)saveFile.GraphicsQuality);

        //Debug.Log("graphic quality : " + graphicsQualityDropdown.Q<EnumField>().value);
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();

        musicLabel.text = GetLocalizedString(UIText.LBL_MUSIC);
        sfxLabel.text = GetLocalizedString(UIText.LBL_SFX);
        joystickPosLabel.text = GetLocalizedString(UIText.LBL_JOYSTICK_POSITION);
        hapticLabel.text = GetLocalizedString(UIText.LBL_HAPTIC);
        graphicsQualityLabel.text = GetLocalizedString(UIText.LBL_GRAPHICS_QUALITY);
        languageLabel.text = GetLocalizedString(UIText.LBL_LANGUAGE);

        localizedGraphicsSettings = new List<string>
        {
            GetLocalizedString(UIText.LBL_GRAPHICS_QUALITY_ULTRA),
            GetLocalizedString(UIText.LBL_GRAPHICS_QUALITY_HIGH),
            GetLocalizedString(UIText.LBL_GRAPHICS_QUALITY_MEDIUM),
            GetLocalizedString(UIText.LBL_GRAPHICS_QUALITY_LOW),
        };

        graphicsQualityDropdown.choices = localizedGraphicsSettings;
        graphicsQualityDropdown.index = saveLoadManager.SaveFile.GraphicsQuality;
    }

    private void OnGraphicsDropdownSelected(ChangeEvent<string> evt)
    {
        GraphicsQualityChanged?.Invoke((GraphicQuality)graphicsQualityDropdown.index);
    }

    private void OnLanguageDropdownSelected(ChangeEvent<string> evt)
    {
        // Get the selected language name from the event
        string selectedLanguage = evt.newValue;

        // Find the corresponding locale code in sortedLanguages
        var selectedLanguagePair = sortedLanguages.Find(lang => lang.Value == selectedLanguage);

        if (!string.IsNullOrEmpty(selectedLanguagePair.Key))
        {
            saveLoadManager.ChangeLanguage(selectedLanguagePair.Key);
            ShowScreen();
        }
        else
        {
            Debug.LogWarning($"Selected language '{selectedLanguage}' is not in the sortedLanguages list.");
        }
    }

    private void OnClickBackBtn(ClickEvent evt)
    {
        BackButtonClicked?.Invoke();
        HandleButtonClickPositiveSFX();
    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        BackButtonClicked?.Invoke();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            BackButtonClicked?.Invoke();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnGrapicQualityDropdownSelected(ChangeEvent<Enum> evt)
    {
        GraphicsQualityChanged?.Invoke((GraphicQuality)evt.newValue);
        HandleButtonClickPositiveSFX();
    }

    public void SetShownByHomeScreen(bool val)
    {
        shownByHomeScreen = val;
    }



    private void GoBackToHomeScreen()
    {
        mainMenuUIManager?.ShowHomeScreen();
    }

    private void GoBackToPauseMenuScreen()
    {

    }

    private void OnSFXVolumeSliderValueChanged(ChangeEvent<float> evt)
    {
        SFXVolumeChanged?.Invoke(evt.newValue);
    }

    private void OnMusicVolumeSliderValueChanged(ChangeEvent<float> evt)
    {
        MusicVolumeChanged?.Invoke(evt.newValue);
    }
}
