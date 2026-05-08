using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using System;

public class AccessoryInventoryScreen : MenuScreen
{
    //public event Action
    [SerializeField]
    private BooleanEventChannelSO accessoryInventoryScreenControlChannel;

    [SerializeField]
    private GameAccessoryManager gameAccessoryManager;

    [SerializeField]
    private EventChannelSO exitShelfEC, refreshAccessoryEC;

    [SerializeField]
    private TransformChannelSO playerTransformChannel;

    [SerializeField]
    private AccessoryShelf accessoryShelf;

    [SerializeField]
    private VisualTreeAsset shelfSlotFrameTemplate;

    private Button leftButton, rightButton, backButton, refreshButton;
    private ProgressBar attackBar, defenseBar, agilityBar, luckBar;
    private AccessoryManager playerAccManager;

    private float attackChange, defenseChange, agilityChange, luckChange;

    private const string leftButtonName = "button-left";
    private const string rightButtonName = "button-right";
    private const string backButtonName = "button-back";
    private const string refreshButtonName = "button-refresh";
    private const string attackBarName = "bar-attack";
    private const string defenseBarName = "bar-defense";
    private const string agilityBarName = "bar-agility";
    private const string luckBarName = "bar-luck";
    private const string cardCountLabelName = "label-card-count";
    private const string frameButtonName = "button-frame";

    private List<VisualElement> frames;
    private AccessorySlot[] shelfSlots;

    // this should be hard coded. because frame.resolvedStyle contains NaN at screen is just shown.
    private float halfFrameLength = 125;
    private float slotHeightOffset = 0.2f;

    // 최대 +8 스탯 표시 가능
    private float barChangeAmount = 6.25f;

    private void OnEnable()
    {
        accessoryInventoryScreenControlChannel.Subscribe(ShowAccessoryInventoryScreen);
        playerTransformChannel.Subscribe(SetPlayerAccManager);
        gameAccessoryManager.DataChanged += OnDataChanged;
        
    }

    private void OnDisable()
    {
        accessoryInventoryScreenControlChannel.Unsubscribe(ShowAccessoryInventoryScreen);
        playerTransformChannel.Unsubscribe(SetPlayerAccManager);
        gameAccessoryManager.DataChanged -= OnDataChanged;
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        leftButton = screen.Q<Button>(leftButtonName);
        rightButton = screen.Q<Button>(rightButtonName);
        backButton = screen.Q<Button>(backButtonName);
        refreshButton = screen.Q<Button>(refreshButtonName);


        attackBar = screen.Q<ProgressBar>(attackBarName);
        defenseBar = screen.Q<ProgressBar>(defenseBarName);
        agilityBar = screen.Q<ProgressBar>(agilityBarName);
        luckBar = screen.Q<ProgressBar>(luckBarName);

        frames = new List<VisualElement>();

        InitializeBar();
    }

    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();

        screen.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        backButton.RegisterCallback<ClickEvent>(OnClickBackBtn);
        backButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitBackBtn);
        backButton.RegisterCallback<KeyDownEvent>(OnKeyDownBackBtn);
        refreshButton.RegisterCallback<ClickEvent>(OnClickRefreshBtn);
        refreshButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitRefreshBtn);
        refreshButton.RegisterCallback<KeyDownEvent>(OnKeyDownRefreshBtn);

        leftButton.RegisterCallback<ClickEvent>(OnClickLeftButton);
        rightButton.RegisterCallback<ClickEvent>(OnClickRightButton);
        leftButton.RegisterCallback<NavigationSubmitEvent>(OnSumitLeftButton);
        rightButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitRightButton);
        leftButton.RegisterCallback<KeyDownEvent>(OnKeyDownLeftButton);
        rightButton.RegisterCallback<KeyDownEvent>(OnKeyDownRightButton);
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
        SetBarsFont();
    }

    protected override void SetLocalizedText()
    {
        base.SetLocalizedText();

        attackBar.title = GetLocalizedString(UIText.LBL_ATTACK);
        defenseBar.title = GetLocalizedString(UIText.LBL_DEFENSE);
        agilityBar.title = GetLocalizedString(UIText.LBL_AGILITY);
        luckBar.title = GetLocalizedString(UIText.LBL_LUCK);
    }

    private void ShowAccessoryInventoryScreen(bool value)
    {
        if (value)
        {
            ShowScreen();
            (int, int, int, int) bonusStats = playerAccManager.GetAccessoryBonusStats();
            ResetBarChanges();
            ApplyAttackChange(bonusStats.Item1);
            ApplyDefenseChange(bonusStats.Item2);
            ApplyAgilityChange(bonusStats.Item3);
            ApplyLuckChange(bonusStats.Item4);
            AnimateBar();

            playerAccManager.OnAccessoryEquipped += OnAccessorySwapped;
            playerAccManager.OnRemoveAllAccessories += OnRefreshAccessories;

            SetSlotFrame();
        }
        else
        {
            HideScreen();

            playerAccManager.OnAccessoryEquipped -= OnAccessorySwapped;
            playerAccManager.OnRemoveAllAccessories -= OnRefreshAccessories;
        }
    }

    private void OnClickBackBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        ExitShelf();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitBackBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        ExitShelf();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownBackBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            ExitShelf();
            HandleButtonClickPositiveSFX();
        }
    }

    private void OnClickRefreshBtn(ClickEvent evt)
    {
        evt.StopPropagation();
        RefreshCharacterAccessory();
        HandleButtonClickPositiveSFX();

        //ExitShelf();
    }

    private void OnSubmitRefreshBtn(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        RefreshCharacterAccessory();
        HandleButtonClickPositiveSFX();

        //ExitShelf();
    }

    private void OnKeyDownRefreshBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            RefreshCharacterAccessory();
            HandleButtonClickPositiveSFX();

            //ExitShelf();
        }
    }


    private void OnClickLeftButton(ClickEvent evt)
    {
        evt.StopPropagation();
        OnClickLeft();
    }

    private void OnClickRightButton(ClickEvent evt)
    {
        evt.StopPropagation();
        OnClickRight();
    }

    private void OnSumitLeftButton(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        OnClickLeft();
    }

    private void OnSubmitRightButton(NavigationSubmitEvent evt)
    {
        evt.StopPropagation();
        OnClickRight();
    }

    private void OnKeyDownLeftButton(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            OnClickLeft();
        }
    }

    private void OnKeyDownRightButton(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            evt.StopPropagation();
            OnClickRight();
        }
    }

    private void ExitShelf()
    {
        exitShelfEC.Raise();
    }

    private void RefreshCharacterAccessory()
    {
        refreshAccessoryEC.Raise();
    }

    private void InitializeBar()
    {
        attackBar.value = 50;
        defenseBar.value = 50;
        agilityBar.value = 50;
        luckBar.value = 50;
    }

    private void ResetBarChanges()
    {
        attackChange = 50 - attackBar.value;
        defenseChange = 50 - defenseBar.value;
        agilityChange = 50 - agilityBar.value;
        luckChange = 50 - luckBar.value;
    }

    public void ApplyAttackChange(float change)
    {
        attackChange += barChangeAmount * change;
    }

    public void ApplyDefenseChange(float change)
    {
        defenseChange += barChangeAmount * change;
    }

    public void ApplyAgilityChange(float change)
    {
        agilityChange += barChangeAmount * change;
    }

    public void ApplyLuckChange(float change)
    {
        luckChange += barChangeAmount * change;
    }

    public void AnimateBar()
    {
        StartCoroutine(AnimateBarChange(attackBar, attackChange, 0.5f));
        StartCoroutine(AnimateBarChange(defenseBar, defenseChange, 0.5f));
        StartCoroutine(AnimateBarChange(agilityBar, agilityChange, 0.5f));
        StartCoroutine(AnimateBarChange(luckBar, luckChange, 0.5f));
        ResetBarChanges();
    }

    private IEnumerator AnimateBarChange(ProgressBar bar, float changeAmount, float duration)
    {
        float initialValue = bar.value;
        float targetValue = Mathf.Clamp(initialValue + changeAmount, 0, 100);
        float time = 0;

        while (time < duration)
        {
            time += Time.deltaTime;
            float newValue = Mathf.Lerp(initialValue, targetValue, time / duration);
            bar.value = newValue;
            yield return null;
        }

        bar.value = targetValue; // Ensure it ends exactly at the target value
    }

    private void SetPlayerAccManager(Transform playerTransform)
    {
        if (ReferenceEquals(playerTransform, null))
        {
            return;
        }

        playerAccManager = playerTransform.GetComponent<AccessoryManager>();
    }

    private void OnAccessorySwapped(AccData newAcc)
    {
        ResetBarChanges();

        int attack = 0;
        int defense = 0;
        int agility = 0;
        int luck = 0;

        //if (!ReferenceEquals(oldAcc, null))
        //{
        //    attack -= oldAcc.Attack;
        //    defense -= oldAcc.Defense;
        //    agility -= oldAcc.Agility;
        //    luck -= oldAcc.Luck;
        //}

        AccStats newAccStats = newAcc.accessoryStats;

        if (!ReferenceEquals(newAcc, null))
        {
            // TODO : 추후 스탯의 절대적 수치 적용을 조정해야 할 수도 있음
            attack += newAccStats.Attack;
            defense += newAccStats.Defense;
            agility += newAccStats.Agility;
            luck += newAccStats.Luck;
        }

        ApplyAttackChange(attack);
        ApplyDefenseChange(defense);
        ApplyAgilityChange(agility);
        ApplyLuckChange(luck);

        AnimateBar();
    }

    private void OnRefreshAccessories(List<AccData> accDatas)
    {
        ResetBarChanges();
        AnimateBar();
    }

    private void SetSlotFrame()
    {
        for (int i = 0; i < frames.Count; i++)
        {
            screen.Remove(frames[i]);
        }

        frames.Clear();
        shelfSlots = accessoryShelf.Slots;
        var currentAccDatas = accessoryShelf.currentAccData;


        for (int i = 0; i < shelfSlots.Length; i++)
        {
            AccessorySlot slot = shelfSlots[i];
            Vector3 screenPos = GetScreenPosOf(slot);
            int cardCount = 0;

            if (i < currentAccDatas.Count)
            {
                cardCount = currentAccDatas[i].Item2.Count;
            }

            VisualElement frame = shelfSlotFrameTemplate.CloneTree();
            screen.Add(frame);
            IStyle frameStyle = frame.style;
            frameStyle.position = Position.Absolute;

            //if(screen.resolvedStyle.width != Screen.width)
            //{
            //    // resolvedStyle의 height가 항상 1080으로 고정되어있네. 이유는 무엇일까?
            //}

            float ratio = screen.resolvedStyle.height / Screen.height;

            frameStyle.left = ratio * screenPos.x - halfFrameLength;
            frameStyle.bottom = ratio *screenPos.y + halfFrameLength;

            Label label = frame.Q<Label>(cardCountLabelName);
            label.text = cardCount.ToString();

            Button button = frame.Q<Button>(frameButtonName);
            button.RegisterCallback<ClickEvent>(evt =>
            {
                if (accessoryShelf.TrySelectSlot(slot))
                {
                    HideScreen();
                }
            });

            screen.Add(frame);
            frames.Add(frame);
        }
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        SetSlotFrame();
    }

    private Vector3 GetScreenPosOf(AccessorySlot slot)
    {
        return Camera.main.WorldToScreenPoint(slot.transform.position + slotHeightOffset * Vector3.up);
    }

    public List<Vector3> GetScreenPosOfSlots()
    {
        List<Vector3> positions = new List<Vector3>();

        foreach (var slot in accessoryShelf.Slots)
        {
            positions.Add(GetScreenPosOf(slot));
        }

        return positions;
    }

    private void OnDataChanged()
    {
        if (IsVisible()) 
        {
            SetSlotFrame();
        }
    }

    private void OnClickRight()
    {
        if (accessoryShelf.ShowRight())
        {
            HandleButtonClickPositiveSFX();
            SetSlotFrame();
        }
        else
        {
            HandleButtonClickNegativeSFX();
        }
    }

    private void OnClickLeft()
    {
        if (accessoryShelf.ShowLeft())
        {
            HandleButtonClickPositiveSFX();
            SetSlotFrame();
        }
        else
        {
            HandleButtonClickNegativeSFX();
        }
    }
}
