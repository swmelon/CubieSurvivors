using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CharSelectionBar : MenuScreen
{
    [SerializeField]
    private CharacterManagerSO characterManager;
    
    [SerializeField]
    private Sprite lockedCharIcon;
    
    public static event Action<int> CharPortraitClicked;
    private readonly string selectBarName = "bar-select";
    
    private List<Button> charButtons = new List<Button>();
    private VisualElement selectBar;


    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        charButtons = screen.Query<Button>().ToList();
        
        for (int i = 0; i < charButtons.Count; i++)
        {
            Button button = charButtons[i];
            
            int index = i;
            
            SetCharPortraitImage(button, index);
            button.RegisterCallback<ClickEvent>((evt) => CharButtonClicked(evt, index));
            button.RegisterCallback<NavigationSubmitEvent>((evt) => CharButtonSubmitted(evt, index));
        }
        
        selectBar = root.Q<VisualElement>(selectBarName);

        int charIndex = characterManager.GetCurrentCharIndex();
        SetSelectBarPosition(charIndex);
    }
    
    private void CharButtonClicked(ClickEvent evt, int index)
    {
        SetSelectBarPosition(index);
        CharPortraitClicked?.Invoke(index);
    }

    private void CharButtonSubmitted(NavigationSubmitEvent evt, int index)
    {
        SetSelectBarPosition(index);
        CharPortraitClicked?.Invoke(index);
    }
    
    private void SetCharPortraitImage(Button button, int index)
    {
        if (characterManager.TryGetCharIcon(index, out Sprite icon))
        {
            button.style.backgroundImage = new StyleBackground(icon);
            button.SetEnabled(true);
        }
        else
        {
            button.style.backgroundImage = new StyleBackground(lockedCharIcon);
            button.SetEnabled(false);
        }
    }
    
    private void SetSelectBarPosition(int index)
    {
        // how can i know the height of the selectbar?
        // its 20% of the screen height
        // so i need to get the screen height
        // and then set the selectbar height to 20% of that
        selectBar.style.top = index * 216 ;
    }
}
