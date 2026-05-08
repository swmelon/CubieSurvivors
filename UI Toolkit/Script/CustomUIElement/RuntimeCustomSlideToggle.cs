using MyUILibrary;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class RuntimeCustomSlideToggle : MonoBehaviour
{
    // load saved value
    [SerializeField]
    private SaveLoadManagerSO saveLoadManager;

    [SerializeField]
    private Texture2D knobTextureL, knobTextureR, knobTextureOff, knobTextureOn;
    
    private VisualElement root;
    private VisualElement joystickSlideToggle, vibrationSlideToggle;
    private VisualElement JSTKnob, VSTKnob;
    private VisualElement JSTInputPanel, VSTInputPanel;

    private void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        joystickSlideToggle = root.Q<VisualElement>("SlideToggleControlJoystickPos");
        JSTInputPanel = joystickSlideToggle.Q<VisualElement>("input");
        JSTKnob = joystickSlideToggle.Q<VisualElement>("knob");

        vibrationSlideToggle = root.Q<VisualElement>("SlideToggleControlVibration");
        VSTInputPanel = vibrationSlideToggle.Q<VisualElement>("input");
        VSTKnob = vibrationSlideToggle.Q<VisualElement>("knob");

        joystickSlideToggle.RegisterCallback<ClickEvent>(OnClickJST);
        joystickSlideToggle.RegisterCallback<KeyDownEvent>(OnKeydownEventJST);
        joystickSlideToggle.RegisterCallback<NavigationSubmitEvent>(OnSubmitJST);

        vibrationSlideToggle.RegisterCallback<ClickEvent>(OnClickVST);
        vibrationSlideToggle.RegisterCallback<KeyDownEvent>(OnKeydownEventVST);
        vibrationSlideToggle.RegisterCallback<NavigationSubmitEvent>(OnSubmitVST);

        SetInitialTexture();
    }

    private void SetInitialTexture()
    {
        var slideToggleJ = this.joystickSlideToggle as SlideToggleControlJoystickPos;

        if (slideToggleJ.value)
        {
            IStyle knobStyle = JSTKnob.style;
            knobStyle.backgroundImage = knobTextureR;
        }
        else
        {
            IStyle knobStyle = JSTKnob.style;
            knobStyle.backgroundImage = knobTextureL;
        }

        var slideToggleV = this.vibrationSlideToggle as SlideToggleControlVibration;

        if (slideToggleV.value)
        {
            IStyle knobStyle = VSTKnob.style;
            knobStyle.backgroundImage = knobTextureOn;
        }
        else
        {
            IStyle knobStyle = VSTKnob.style;
            knobStyle.backgroundImage = knobTextureOff;
        }
    }

    private void OnClickJST(ClickEvent evt)
    {
        var slideToggle = this.joystickSlideToggle as SlideToggleControlJoystickPos;

        if (slideToggle.value)
        {
            IStyle knobStyle = JSTKnob.style;
            knobStyle.backgroundImage = knobTextureR;
        }
        else
        {
            IStyle knobStyle = JSTKnob.style;
            knobStyle.backgroundImage = knobTextureL;
        }
    }

    private void OnKeydownEventJST(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Space || evt.keyCode == KeyCode.Return)
        {
            var slideToggle = this.joystickSlideToggle as SlideToggleControlJoystickPos;

            if (slideToggle.value)
            {
                IStyle knobStyle = JSTKnob.style;
                knobStyle.backgroundImage = knobTextureR;
            }
            else
            {
                IStyle knobStyle = JSTKnob.style;
                knobStyle.backgroundImage = knobTextureL;
            }
        }
    }

    private void OnSubmitJST(NavigationSubmitEvent evt)
    {
        var slideToggle = this.joystickSlideToggle as SlideToggleControlJoystickPos;

        if (slideToggle.value)
        {
            IStyle knobStyle = JSTKnob.style;
            knobStyle.backgroundImage = knobTextureR;
        }
        else
        {
            IStyle knobStyle = JSTKnob.style;
            knobStyle.backgroundImage = knobTextureL;
        }
    }

    private void OnClickVST(ClickEvent evt)
    {
        var slideToggle = this.vibrationSlideToggle as SlideToggleControlVibration;

        if (slideToggle.value)
        {
            IStyle knobStyle = VSTKnob.style;
            knobStyle.backgroundImage = knobTextureOn;
        }
        else
        {
            IStyle knobStyle = VSTKnob.style;
            knobStyle.backgroundImage = knobTextureOff;
        }
    }

    private void OnKeydownEventVST(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Space || evt.keyCode == KeyCode.Return)
        {
            var slideToggle = this.vibrationSlideToggle as SlideToggleControlVibration;

            if (slideToggle.value)
            {
                IStyle knobStyle = VSTKnob.style;
                knobStyle.backgroundImage = knobTextureOn;
            }
            else
            {
                IStyle knobStyle = VSTKnob.style;
                knobStyle.backgroundImage = knobTextureOff;
            }
        }
    }

    private void OnSubmitVST(NavigationSubmitEvent evt)
    {
        var slideToggle = this.vibrationSlideToggle as SlideToggleControlVibration;

        if (slideToggle.value)
        {
            IStyle knobStyle = VSTKnob.style;
            knobStyle.backgroundImage = knobTextureOn;
        }
        else
        {
            IStyle knobStyle = VSTKnob.style;
            knobStyle.backgroundImage = knobTextureOff;
        }
    }



    private void ToggleJST()
    {
        var slideToggle = this.joystickSlideToggle as SlideToggleControlJoystickPos;
        slideToggle.ToggleValue();
    }

    private void ToggleVST()
    {
        var slideToggle = this.vibrationSlideToggle as SlideToggleControlVibration;
        slideToggle.ToggleValue();
    }
}
