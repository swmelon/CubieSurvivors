using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreScreen : MenuScreen
{
    [SerializeField]
    private CustomNumAnimController numberAnimationController;

    [SerializeField]
    private GameScenarioManagerSO gameScenarioManager;

    [SerializeField]
    private LongEventChannelSO updateTotalScoreEC;

    [Tooltip("1초 동안 애니메이트 할 기준 값에 루트를 씌운 값")]
    [SerializeField]
    private float animUnitTotalDamage, animUnitUpgrades, animUnitBossCount, animUnitTime;

    public event Action BottomButtonClicked;

    private const string totalSocreLabelName = "label-total-score";
    private const string totalDamageLabelName = "label-total-damage";
    private const string bossCountLabelName = "label-boss-count";
    private const string revivedLabelName = "label-revived";
    private const string timeLabelName = "label-time";
    
    private const string totalDamageNumberLabelName = "label-total-damage-number";
    private const string bossCountNumberLabelName = "label-boss-count-number";
    private const string revivedNumberLabelName = "label-revived-number";
    private const string timeNumberLabelName = "label-time-number";
    
    private const string bottomButtonName = "button-bottom";

    private Label totalScoreLabel, totalDamageLabel, bossCountLabel, revivedLabel, timeLabel,
        totalScoreNumberLabel, totalDamageNumberLabel, bossCountNumberLabel, revivedNumberLabel, timeNumberLabel;
    private Button bottomButton;


    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        totalScoreLabel = screen.Q<Label>(totalSocreLabelName);

        totalDamageLabel = screen.Q<Label>(totalDamageLabelName);
        bossCountLabel = screen.Q<Label>(bossCountLabelName);
        revivedLabel = screen.Q<Label>(revivedLabelName);
        timeLabel = screen.Q<Label>(timeLabelName);

        totalDamageNumberLabel = screen.Q<Label>(totalDamageNumberLabelName);
        bossCountNumberLabel = screen.Q<Label>(bossCountNumberLabelName);
        revivedNumberLabel = screen.Q<Label>(revivedNumberLabelName);
        timeNumberLabel = screen.Q<Label>(timeNumberLabelName);

        bottomButton = screen.Q<Button>(bottomButtonName);
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();

        totalDamageLabel.text = GetLocalizedString(UIText.LBL_TOTAL_DAMAGE);
        bossCountLabel.text = GetLocalizedString(UIText.LBL_UPGRADES);
        revivedLabel.text = GetLocalizedString(UIText.LBL_BOSS_KILLED);
        timeLabel.text = GetLocalizedString(UIText.LBL_TIME);
    }

    public void SetupScreen(int totalDamage, int bossCount, int revived, int time, string buttonText)
    {
        totalDamageNumberLabel.text = totalDamage.ToString();
        bossCountNumberLabel.text = bossCount.ToString();
        revivedNumberLabel.text = revived.ToString();

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timeNumberLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        bottomButton.text = GetLocalizedString(buttonText);

        HideAllLabels();

        StartNumberAnim(totalDamage, bossCount, revived, time);
    }

    private void HideAllLabels()
    {
        totalDamageLabel.style.display = DisplayStyle.None;
        bossCountLabel.style.display = DisplayStyle.None;
        revivedLabel.style.display = DisplayStyle.None;
        timeLabel.style.display = DisplayStyle.None;

        totalDamageNumberLabel.style.display = DisplayStyle.None;
        bossCountNumberLabel.style.display = DisplayStyle.None;
        revivedNumberLabel.style.display = DisplayStyle.None;
        timeNumberLabel.style.display = DisplayStyle.None;

        bottomButton.style.display = DisplayStyle.None;
    }

    private async void StartNumberAnim(int totalDamage, int upgrades, int bossCount, int time)
    {
        // 그냥 await 써봄
        numberAnimationController._numberText = totalScoreLabel;
        long totalScore = totalDamage;
        //NumAnimData data = new NumAnimData();
        NumAnimData data = ScriptableObject.CreateInstance<NumAnimData>();
        data._total = totalScore;

        float animTime = Mathf.Pow(totalDamage, 0.5f) / animUnitTotalDamage;

        data._animationTime = animTime;
        //data._animationTime = 2f;

        totalDamageLabel.style.display = DisplayStyle.Flex;
        totalDamageNumberLabel.style.display = DisplayStyle.Flex;
        
        numberAnimationController.Animate(data);

        await System.Threading.Tasks.Task.Delay((int)(animTime * 1000) + 1);
        //await System.Threading.Tasks.Task.Delay(2000);


        totalScore = (long)(totalScore * (1.1f + upgrades / 5));

        animTime = Mathf.Pow(upgrades, 0.5f) / animUnitUpgrades;

        data._animationTime = animTime;
        data._total = totalScore;
        
        bossCountLabel.style.display = DisplayStyle.Flex;
        bossCountNumberLabel.style.display = DisplayStyle.Flex;

        numberAnimationController.Animate(data);

        await System.Threading.Tasks.Task.Delay((int)(animTime * 1000) + 1);

        totalScore = (long)(totalScore * (1.1f + bossCount));

        animTime = Mathf.Pow(bossCount, 0.5f) / animUnitBossCount;
        
        data._total = totalScore;
        data._animationTime = animTime;

        revivedLabel.style.display = DisplayStyle.Flex;
        revivedNumberLabel.style.display = DisplayStyle.Flex;

        numberAnimationController.Animate(data);

        await System.Threading.Tasks.Task.Delay((int)(animTime * 1000) + 1);





        int estimatedTime = gameScenarioManager.GetCurrentScenario().GetEstimatedCompleteTime();
        

        totalScore = (long)(totalScore * (1.1f + Mathf.Clamp(estimatedTime - time, 0, int.MaxValue) / 60));

        animTime = Mathf.Pow(time, 0.5f) / animUnitTime;
        
        data._total = totalScore;
        data._animationTime = animTime;
        
        timeLabel.style.display = DisplayStyle.Flex;
        timeNumberLabel.style.display = DisplayStyle.Flex;

        numberAnimationController.Animate(data);        
        updateTotalScoreEC.Raise(totalScore);

        await System.Threading.Tasks.Task.Delay((int)(animTime * 1000) + 1);
        
        bottomButton.style.display = DisplayStyle.Flex;
    }
    

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        
        bottomButton.RegisterCallback<ClickEvent>(OnClickBottomBtn);
        bottomButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBottomBtn);
        bottomButton.RegisterCallback<KeyDownEvent>(OnKeyDownBottomBtn);
    }
    
    private void OnClickBottomBtn(ClickEvent evt)
    {
        //ActivateButton(m_InfoScreenMenuButton);
        BottomButtonClicked?.Invoke();
        evt.StopPropagation();
        //ClickMarker(evt);
    }

    private void OnSubmitBottomBtn(NavigationSubmitEvent evt)
    {
        BottomButtonClicked?.Invoke();
        evt.StopPropagation();
    }

    private void OnKeyDownBottomBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            BottomButtonClicked?.Invoke();
            evt.StopPropagation();
        }
    }
}
