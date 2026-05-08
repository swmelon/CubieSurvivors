using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class InitialWeaponSelectionScreen : MenuScreen
{
    public Color activeTextColor, inactiveTextColor;

    public event Action BackButtonClicked;

    public event Action BattleButtonClicked;
    public event Action RewardButtonClicked;

    public event Action LeftButtonClicked;
    public event Action RightButtonClicked;

    public event Action NormalButtonClicked;
    public event Action HardButtonClicked;
    public event Action HellButtonClicked;

    public event Action StartButtonClicked;

    private const string backButtonName = "button-back";

    private const string battleTabName = "tab-battle";
    private const string battleButtonName = "button-battle";
    private const string rewardTabName = "tab-reward";
    private const string rewardButtonName = "button-reward";
    private const string leftButtonName = "button-left";
    private const string rightButtonName = "button-right";
    private const string normalButtonName = "button-normal";
    private const string hardButtonName = "button-hard";
    private const string hellButtonName = "button-hell";
    private const string startButtonName = "button-start";
    private const string difficultyBarName = "bar-difficulty";
    private const string scenarioIconImageName = "image-scenario-icon";
    private const string scenarioNameLabelName = "label-scenario-name";
    private const string scoreLabelName = "label-score";


    private Button backButton, battleButton, rewardButton, leftButton, rightButton, 
        normalButton, hardButton, hellButton, startButton;

    private VisualElement battleTab, rewardTab, difficultyBar, scenarioIconImage;
    private Label scenarioNameLabel, scoreLabel;
    
    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        
        backButton = screen.Q<Button>(backButtonName);
        battleTab = screen.Q<VisualElement>(battleTabName);
        battleButton = screen.Q<Button>(battleButtonName);
        rewardTab = screen.Q<VisualElement>(rewardTabName);
        rewardButton = screen.Q<Button>(rewardButtonName);
        leftButton = screen.Q<Button>(leftButtonName);
        rightButton = screen.Q<Button>(rightButtonName);
        scenarioIconImage = screen.Q<VisualElement>(scenarioIconImageName);
        scenarioNameLabel = screen.Q<Label>(scenarioNameLabelName);
        scoreLabel = screen.Q<Label>(scoreLabelName);
        normalButton = screen.Q<Button>(normalButtonName);
        hardButton = screen.Q<Button>(hardButtonName);
        hellButton = screen.Q<Button>(hellButtonName);
        startButton = screen.Q<Button>(startButtonName);
        difficultyBar = screen.Q<VisualElement>(difficultyBarName);
    }

    //public void SetupScreen(string noticeText, string leftButtonText, string rightButtonText, float alpha)
    //{
    //    leftButton.text = leftButtonText;
    //    rightButton.text = rightButtonText;
    //    //StyleColor color = container.resolvedStyle.backgroundColor;
    //    //color.value = new Color(color.value.r, color.value.g, color.value.b, alpha);
    //    //container.style.backgroundColor = color;
    //}

    public void ShowScenario(string name, Texture2D icon)
    {
        scenarioIconImage.style.backgroundImage = icon;
        scenarioNameLabel.text = name;
    }

    public void ShowRewardStage(string name, Texture2D icon)
    {
        scenarioIconImage.style.backgroundImage = icon;
        scenarioNameLabel.text = name;
    }

    public void ShowHighestScore(long highestScore)
    {
        scoreLabel.text = UIText.LBL_HIGHEST_SCORE + " : " + highestScore;
    }

    public void ActivateBattleTab()
    {
        IResolvedStyle battleTabStyle = battleTab.resolvedStyle;
        IResolvedStyle rewardTabStyle = rewardTab.resolvedStyle;

        IResolvedStyle battleButtonStyle = battleButton.resolvedStyle;
        IResolvedStyle rewardButtonStyle = rewardButton.resolvedStyle;

        float borderWidth = battleTabStyle.borderBottomWidth;
        battleTab.style.borderBottomWidth = 0;
        rewardTab.style.borderBottomWidth = borderWidth;

        Color textColor = rewardButtonStyle.color;

        //rewardButtonStyle.color = battleButtonStyle.color;
        //battleButtonStyle.color = textColor;
        rewardButton.style.color = battleButtonStyle.color;
        battleButton.style.color = textColor;

        Color backgroundColor = battleTabStyle.backgroundColor;
        battleTab.style.backgroundColor = rewardTabStyle.backgroundColor;
        rewardTab.style.backgroundColor = backgroundColor;



        SetDifficultyEnabled(true);

        scoreLabel.text = "HIGHEST SCORE : ";
    }

    public void ActivateRewardTab()
    {
        IResolvedStyle battleTabStyle = battleTab.resolvedStyle;
        IResolvedStyle rewardTabStyle = rewardTab.resolvedStyle;

        IResolvedStyle battleButtonStyle = battleButton.resolvedStyle;
        IResolvedStyle rewardButtonStyle = rewardButton.resolvedStyle;

        float borderWidth = rewardTabStyle.borderBottomWidth;
        rewardTab.style.borderBottomWidth = 0;
        battleTab.style.borderBottomWidth = borderWidth;

        Color textColor = battleButtonStyle.color;

        //battleButtonStyle.color = rewardButtonStyle.color;
        //rewardButtonStyle.color = textColor;
        battleButton.style.color = rewardButtonStyle.color;
        rewardButton.style.color = textColor;

        Color backgroundColor = battleTabStyle.backgroundColor;
        battleTab.style.backgroundColor = rewardTabStyle.backgroundColor;
        rewardTab.style.backgroundColor = backgroundColor;

        scoreLabel.text = "";

        SetDifficultyEnabled(false);
    }

    public async void SelectNormalButton()
    {
        normalButton.style.color = activeTextColor;
        hardButton.style.color = inactiveTextColor;
        hellButton.style.color = inactiveTextColor;
        
        await Task.Yield();
        await Task.Yield();

        difficultyBar.style.left = 0;
    }

    public async void SelectHardButton()
    {
        normalButton.style.color = inactiveTextColor;
        hardButton.style.color = activeTextColor;
        hellButton.style.color = inactiveTextColor;

        // 이러면 되긴 한다. 두 프레임 기다리는건 안돼. 근데 왜 그런지 몰라. 젠장

        await Task.Yield();
        await Task.Yield();

        difficultyBar.style.left = normalButton.resolvedStyle.width;
    }

    public async void SelectHellButton()
    {
        normalButton.style.color = inactiveTextColor;
        hardButton.style.color = inactiveTextColor;
        hellButton.style.color = activeTextColor;

        await Task.Yield();
        await Task.Yield();

        difficultyBar.style.left = new StyleLength(normalButton.resolvedStyle.width + hardButton.resolvedStyle.width);
    }

    private void SetDifficultyEnabled(bool val)
    {
        if (val)
        {
            normalButton.SetEnabled(true);
            hardButton.SetEnabled(true);
            hellButton.SetEnabled(true);

            normalButton.style.color = activeTextColor;
            hardButton.style.color = inactiveTextColor;
            hellButton.style.color = inactiveTextColor;

            difficultyBar.style.display = DisplayStyle.Flex;
            List<TimeValue> durations = new List<TimeValue>();
            durations.Add(new TimeValue(0.2f, TimeUnit.Second));
            difficultyBar.style.transitionDuration = durations;
        }
        else
        {
            normalButton.SetEnabled(false);
            hardButton.SetEnabled(false);
            hellButton.SetEnabled(false);

            normalButton.style.color = inactiveTextColor;
            hardButton.style.color = inactiveTextColor;
            hellButton.style.color = inactiveTextColor;

            difficultyBar.style.display = DisplayStyle.None;
            List<TimeValue> durations = new List<TimeValue>();
            durations.Add(new TimeValue(0f, TimeUnit.Second));
            difficultyBar.style.transitionDuration = durations;
            difficultyBar.style.left = 0;
        }
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();

        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);

        battleButton.RegisterCallback<ClickEvent>(OnClickBattleBtn);
        battleButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBattleBtn);
        battleButton.RegisterCallback<KeyDownEvent>(OnKeyDownBattleBtn);

        rewardButton.RegisterCallback<ClickEvent>(OnClickRewardBtn);
        rewardButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitRewardBtn);
        rewardButton.RegisterCallback<KeyDownEvent>(OnKeyDownRewardBtn);
        
        leftButton.RegisterCallback<ClickEvent>(OnClickLeftBtn);
        leftButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitLeftBtn);
        leftButton.RegisterCallback<KeyDownEvent>(OnKeyDownLeftBtn);

        rightButton.RegisterCallback<ClickEvent>(OnClickRightBtn);
        rightButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitRightBtn);
        rightButton.RegisterCallback<KeyDownEvent>(OnKeyDownRightBtn);

        normalButton.RegisterCallback<ClickEvent>(OnClickNormalBtn);
        normalButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitNormalBtn);
        normalButton.RegisterCallback<KeyDownEvent>(OnKeyDownNormalBtn);

        hardButton.RegisterCallback<ClickEvent>(OnClickHardBtn);
        hardButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitHardBtn);
        hardButton.RegisterCallback<KeyDownEvent>(OnKeyDownHardBtn);

        hellButton.RegisterCallback<ClickEvent>(OnClickHellBtn);
        hellButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitHellBtn);
        hellButton.RegisterCallback<KeyDownEvent>(OnKeyDownHellBtn);

        startButton.RegisterCallback<ClickEvent>(OnClickStartBtn);
        startButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitStartBtn);
        startButton.RegisterCallback<KeyDownEvent>(OnKeyDownStartBtn);
    }

    private void OnClickBackBtn(ClickEvent evt)
    {
        BackButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        BackButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            BackButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickBattleBtn(ClickEvent evt)
    {
        BattleButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitBattleBtn(NavigationSubmitEvent evt)
    {
        BattleButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBattleBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            BattleButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickRewardBtn(ClickEvent evt)
    {
        RewardButtonClicked?.Invoke();
        evt.StopPropagation();
    }

    private void OnSubmitRewardBtn(NavigationSubmitEvent evt)
    {
        RewardButtonClicked?.Invoke();
        evt.StopPropagation();

    }

    private void OnKeyDownRewardBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            RewardButtonClicked?.Invoke();
            evt.StopPropagation();
        }
    }
    
    private void OnClickLeftBtn(ClickEvent evt)
    {
        LeftButtonClicked?.Invoke();
        evt.StopPropagation();
    }

    private void OnSubmitLeftBtn(NavigationSubmitEvent evt)
    {
        LeftButtonClicked?.Invoke();
        evt.StopPropagation();
    }

    private void OnKeyDownLeftBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            LeftButtonClicked?.Invoke();
            evt.StopPropagation();

        }
    }

    private void OnClickRightBtn(ClickEvent evt)
    {
        RightButtonClicked?.Invoke();
        evt.StopPropagation();
    }

    private void OnSubmitRightBtn(NavigationSubmitEvent evt)
    {
        RightButtonClicked?.Invoke();
        evt.StopPropagation();
    }

    private void OnKeyDownRightBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            RightButtonClicked?.Invoke();
            evt.StopPropagation();
        }
    }

    private void OnClickNormalBtn(ClickEvent evt)
    {
        NormalButtonClicked?.Invoke();
        evt.StopPropagation();

    }

    private void OnSubmitNormalBtn(NavigationSubmitEvent evt)
    {
        NormalButtonClicked?.Invoke();
        evt.StopPropagation();

    }

    private void OnKeyDownNormalBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            NormalButtonClicked?.Invoke();
            evt.StopPropagation();

        }
    }

    private void OnClickHardBtn(ClickEvent evt)
    {
        HardButtonClicked?.Invoke();
        evt.StopPropagation();
    }

    private void OnSubmitHardBtn(NavigationSubmitEvent evt)
    {
        HardButtonClicked?.Invoke();
        evt.StopPropagation();

    }

    private void OnKeyDownHardBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            HardButtonClicked?.Invoke();
            evt.StopPropagation();

        }
    }

    private void OnClickHellBtn(ClickEvent evt)
    {
        HellButtonClicked?.Invoke();
        evt.StopPropagation();

    }

    private void OnSubmitHellBtn(NavigationSubmitEvent evt)
    {
        HellButtonClicked?.Invoke();
        evt.StopPropagation();

    }

    private void OnKeyDownHellBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            HellButtonClicked?.Invoke();
            evt.StopPropagation();

        }
    }

    private void OnClickStartBtn(ClickEvent evt)
    {
        StartButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitStartBtn(NavigationSubmitEvent evt)
    {
        StartButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownStartBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            StartButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();
        }
    }
}
