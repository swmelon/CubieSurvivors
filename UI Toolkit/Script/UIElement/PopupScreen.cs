using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PopupScreen : MenuScreen
{
    public event Action LeftButtonClicked;
    public event Action RightButtonClicked;

    private const string noticeLabelName = "label-notice";
    private const string leftButtonName = "button-left";
    private const string rightButtonName = "button-right";
    private const string containerName = "container";

    private Label noticeLabel;
    private Button leftButton;
    private Button rightButton;
    private VisualElement container;
    
    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        noticeLabel = screen.Q<Label>(noticeLabelName);
        leftButton = screen.Q<Button>(leftButtonName);
        rightButton = screen.Q<Button>(rightButtonName);
        container = screen.Q<VisualElement>(containerName);
    }

    public void SetupScreen(string noticeText, string leftButtonText, string rightButtonText, float alpha)
    {
        noticeLabel.text = GetLocalizedString(noticeText);
        leftButton.text = GetLocalizedString(leftButtonText);
        rightButton.style.display = DisplayStyle.Flex;
        rightButton.text = GetLocalizedString(rightButtonText);
        StyleColor color = container.resolvedStyle.backgroundColor;
        color.value = new Color(color.value.r, color.value.g, color.value.b, alpha);
        container.style.backgroundColor = color;
    }


    public void SetupScreen(string noticeText, string leftButtonText, float alpha)
    {
        noticeLabel.text = GetLocalizedString(noticeText);
        leftButton.text = GetLocalizedString(leftButtonText);
        rightButton.style.display = DisplayStyle.None;
        StyleColor color = container.resolvedStyle.backgroundColor;
        color.value = new Color(color.value.r, color.value.g, color.value.b, alpha);
        container.style.backgroundColor = color;
    }


    protected override void RegisterButtonCallbacks()
    {
        base.RegisterButtonCallbacks();
        
        leftButton.RegisterCallback<ClickEvent>(OnClickLeftBtn);
        leftButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitLeftBtn);
        leftButton.RegisterCallback<KeyDownEvent>(OnKeyDownLeftBtn);

        rightButton.RegisterCallback<ClickEvent>(OnClickRightBtn);
        rightButton.RegisterCallback<NavigationSubmitEvent>(OnSubmitRightBtn);
        rightButton.RegisterCallback<KeyDownEvent>(OnKeyDownRightBtn);
    }
    
    private void OnClickLeftBtn(ClickEvent evt)
    {
        //ActivateButton(m_InfoScreenMenuButton);
        LeftButtonClicked?.Invoke();
        evt.StopPropagation();
        //ClickMarker(evt);
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitLeftBtn(NavigationSubmitEvent evt)
    {
        LeftButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownLeftBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            LeftButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }

    private void OnClickRightBtn(ClickEvent evt)
    {
        RightButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnSubmitRightBtn(NavigationSubmitEvent evt)
    {
        RightButtonClicked?.Invoke();
        evt.StopPropagation();
        HandleButtonClickPositiveSFX();

    }

    private void OnKeyDownRightBtn(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
        {
            RightButtonClicked?.Invoke();
            evt.StopPropagation();
            HandleButtonClickPositiveSFX();

        }
    }
}
