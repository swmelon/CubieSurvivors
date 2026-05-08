
using MyUILibrary;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;
using TMPro;

public class DifficultySettingsScreen : MenuScreen
{
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private TMP_Text debugText;

    private const string backButtonName = "button-back";
    private const string applyButtonName = "button-apply";

    private const string sliderNumEnemiesMultName = "slider-num-enemies-mult";
    private const string sliderEnemiesHealthMultName = "slider-enemies-health-mult";
    private const string sliderInitialExpName = "slider-initial-exp";
    private const string sliderMinESDName = "slider-min-esd";
    private const string sliderMaxESDName = "slider-max-esd";
    private const string sliderNumEnemiesExpoName = "slider-num-enemies-expo";
    private const string sliderPPRExpoName = "slider-ppr-expo";
    private const string sliderPPRInitial = "slider-ppr-initial";

    private Button backButton, applyButton;
    private Slider numEnemiesMult, enemiesHealthMult, initialExp, minESD, maxESD, numEnemiesExpo, pprExpo, pprInitial;

    private static bool haveChanges = false;

    // saved values

    private static float numEnemiesMultValue, numEnemiesHealthMultValue,  numEnemiesExpoValue, pprExpoValue, minESDValue, maxESDValue;
    private static int initialExpValue, pprInitialValue;
    
    public static int InitialExpValue => initialExpValue;
    public static int PPRInitialValue => pprInitialValue;
    public static float MinESDValue => minESDValue;
    public static float MaxESDValue => maxESDValue;
    public static float NumEnemiesMultValue => numEnemiesMultValue;
    public static float EnemiesHealthMultValue => numEnemiesHealthMultValue;
    public static float NumEnemiesExpoValue => numEnemiesExpoValue;
    public static float PPRExpoValue => pprExpoValue;
    public static bool HasChanges => haveChanges;



    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        backButton = screen.Q<Button>(backButtonName);
        applyButton = screen.Q<Button>(applyButtonName);

        numEnemiesMult = screen.Q<Slider>(sliderNumEnemiesMultName);
        enemiesHealthMult = screen.Q<Slider>(sliderEnemiesHealthMultName);
        initialExp = screen.Q<Slider>(sliderInitialExpName);
        minESD = screen.Q<Slider>(sliderMinESDName);
        maxESD = screen.Q<Slider>(sliderMaxESDName);
        numEnemiesExpo = screen.Q<Slider>(sliderNumEnemiesExpoName);
        pprExpo = screen.Q<Slider>(sliderPPRExpoName);
        pprInitial = screen.Q<Slider>(sliderPPRInitial);
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);

        applyButton.RegisterCallback<ClickEvent>(OnClickApplyBtn);
        applyButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitApplyBtn);
        applyButton.RegisterCallback<KeyDownEvent>(OnKeyDownApplyBtn);
    }


    private void OnClickBackBtn(ClickEvent evt)
    {
        GoBackToHomeScreen();
        HideScreen();
        HandleButtonClickPositiveSFX();
    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        GoBackToHomeScreen();
        HideScreen();

        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            GoBackToHomeScreen();
            HideScreen();

            HandleButtonClickPositiveSFX();
        }
    }

    private void OnClickApplyBtn(ClickEvent evt)
    {
        OnApply();
        HandleButtonClickPositiveSFX();
    }

    private void OnSubmitApplyBtn(NavigationSubmitEvent evt)
    {
        OnApply();
        HandleButtonClickPositiveSFX();
    }

    private void OnKeyDownApplyBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            OnApply();
            HandleButtonClickPositiveSFX();
        }
    }


    private void OnApply()
    {
        haveChanges = true;
        numEnemiesMultValue = numEnemiesMult.value;
        numEnemiesHealthMultValue = enemiesHealthMult.value;
        initialExpValue = Mathf.RoundToInt(initialExp.value);
        minESDValue = minESD.value;
        maxESDValue = maxESD.value;
        numEnemiesExpoValue = numEnemiesExpo.value;
        pprExpoValue = pprExpo.value;
        pprInitialValue = Mathf.RoundToInt(pprInitial.value);

        debugText.text = $"numEnemiesMultValue: {numEnemiesMultValue}\n" +
            $"EnemiesHealthMultValue: {numEnemiesHealthMultValue}\n" +
            $"initialExpValue: {initialExpValue}\n" +
            $"minESDValue: {minESDValue}\n" +
            $"maxESDValue: {maxESDValue}\n" +
            $"numEnemiesExpoValue: {numEnemiesExpoValue}\n" +
            $"pprExpoValue: {pprExpoValue}\n" +
            $"pprInitialValue: {pprInitialValue}\n";
    }


    private void GoBackToHomeScreen()
    {
        mainMenuUIManager?.ShowHomeScreen();
    }

    private void GoBackToPauseMenuScreen()
    {

    }
}
