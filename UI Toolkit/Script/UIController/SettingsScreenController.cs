
using System;
using UnityEngine;

public class SettingsScreenController : MonoBehaviour
{
    [SerializeField]
    private SettingsScreen settingsScreen;

    [SerializeField]
    private MainMenuUIManager mainMenuUIManager;

    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private BooleanEventChannelSO joystickPositionControllChannel;

    [SerializeField]
    private FMODAudioManager fmodAudioManager;

    [SerializeField]
    private HapticController hapticController;

    [SerializeField]
    private GraphicsQualityManager graphicsQualityManager;

    private SaveFile saveFile;

    private void OnEnable()
    {
        settingsScreen.BackButtonClicked += OnClickBackBtn;
        settingsScreen.JoystickPositionToggled += OnJoystickPositionToggled;
        settingsScreen.JoystickPositionLoaded += OnJoyStickPositionLoaded;
        settingsScreen.MusicVolumeChanged += OnMusicVolumeChanged;
        settingsScreen.MusicVolumeLoaded += OnMusicVolumeLoaded;
        settingsScreen.SFXVolumeChanged += OnSFXVolumeChanged;
        settingsScreen.SFXVolumeLoaded += OnSFXVolumeLoaded;
        settingsScreen.VibrationToggled += OnVibrationToggled;
        settingsScreen.HapticLoaded += OnVibrationLoaded;
        settingsScreen.GraphicsQualityChanged += OnGraphicsQualityChanged;
        settingsScreen.GraphicsQualityLoaded += OnGraphicsQualityLoaded;
        saveFile = saveLoadManager.SaveFile;
    }
    
    private void OnDisable()
    {
        settingsScreen.BackButtonClicked -= OnClickBackBtn;
        settingsScreen.JoystickPositionToggled -= OnJoystickPositionToggled;
        settingsScreen.MusicVolumeChanged -= OnMusicVolumeChanged;
        settingsScreen.SFXVolumeChanged -= OnSFXVolumeChanged;
        settingsScreen.VibrationToggled -= OnVibrationToggled;
    }

    private void OnClickBackBtn()
    {
        if (settingsScreen.ShownByHomeScreen)
        {
            settingsScreen.HideScreen();
            mainMenuUIManager.ShowHomeScreen();
        }
        else
        {
            settingsScreen.HideScreen();
            mainMenuUIManager.ShowPauseMenuScreen();
        }
    }

    private void OnJoystickPositionToggled(bool value)
    {
        joystickPositionControllChannel.Raise(value);
        saveFile.RightJoystick = value;
        saveLoadManager.Save();
    }

    private void OnVibrationToggled(bool value)
    {
        hapticController.ActivateHapticFeedback(value);
        saveFile.Vibration = value;
        saveLoadManager.Save();
    }

    private void OnMusicVolumeChanged(float value)
    {
        fmodAudioManager.SetMusicVolume(value);
        saveFile.MusicVolume = value;
        saveLoadManager.Save();
    }

    private void OnSFXVolumeChanged(float value)
    {
        fmodAudioManager.SetSFXVolume(value);
        saveFile.SFXVolume = value;
        saveLoadManager.Save();
    }

    private void OnGraphicsQualityChanged(GraphicQuality value)
    {
        // change graphic quality
        graphicsQualityManager.SetGraphicsQuality(value);

        saveFile.GraphicsQuality = (int)value;
        saveLoadManager.Save();
    }

    private void OnJoyStickPositionLoaded(bool value)
    {
        joystickPositionControllChannel.Raise(value);
    }

    private void OnVibrationLoaded(bool value)
    {
        hapticController.ActivateHapticFeedback(value);
    }

    private void OnMusicVolumeLoaded(float value)
    {
        fmodAudioManager.SetMusicVolume(value);
    }

    private void OnSFXVolumeLoaded(float value)
    {
        fmodAudioManager.SetSFXVolume(value);
    }

    private void OnGraphicsQualityLoaded(GraphicQuality value)
    {
        graphicsQualityManager.SetGraphicsQuality(value);
        // change graphic quality
    }
}
