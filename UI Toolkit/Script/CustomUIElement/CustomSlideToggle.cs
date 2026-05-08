using UnityEngine;
using UnityEngine.UIElements;
using System;
using Unity.VisualScripting;

namespace MyUILibrary
{
    // Derives from BaseField<bool> base class. Represents a container for its input part.
    public class SlideToggleControlJoystickPos : BaseField<bool>
    {
        public new class UxmlFactory : UxmlFactory<SlideToggleControlJoystickPos, UxmlTraits> { }

        public new class UxmlTraits : BaseFieldTraits<bool, UxmlBoolAttributeDescription> { }

        // In the spirit of the BEM standard, the SlideToggle has its own block class and two element classes. It also
        // has a class that represents the enabled state of the toggle.
        public static readonly new string ussClassName = "slide-toggle-alt";
        public static readonly new string inputUssClassName = "slide-toggle-alt__input";
        public static readonly string inputKnobUssClassName = "slide-toggle-alt__input-knob";
        public static readonly string inputCheckedUssClassName = "slide-toggle-alt__input--checked";

        public event Action<bool> ValueChanged;

        VisualElement m_Input;
        VisualElement m_Knob;

        // Custom controls need a default constructor. This default constructor calls the other constructor in this
        // class.
        public SlideToggleControlJoystickPos() : this(null) { }

        // This constructor allows users to set the contents of the label.
        public SlideToggleControlJoystickPos(string label) : base(label, null)
        {
            // Style the control overall.
            AddToClassList(ussClassName);

            // Get the BaseField's visual input element and use it as the background of the slide.
            m_Input = this.Q(className: BaseField<bool>.inputUssClassName);
            m_Input.name = "input";
            m_Input.AddToClassList(inputUssClassName);
            Add(m_Input);

            // Create a "knob" child element for the background to represent the actual slide of the toggle.
            m_Knob = new();
            m_Knob.name = "knob";
            m_Knob.AddToClassList(inputKnobUssClassName);
            m_Input.Add(m_Knob);

            // There are three main ways to activate or deactivate the SlideToggle. All three event handlers use the
            // static function pattern described in the Custom control best practices.

            // ClickEvent fires when a sequence of pointer down and pointer up actions occurs.
            RegisterCallback<ClickEvent>(evt => OnClick(evt));
            // KeydownEvent fires when the field has focus and a user presses a key.
            RegisterCallback<KeyDownEvent>(evt => OnKeydownEvent(evt));
            // NavigationSubmitEvent detects input from keyboards, gamepads, or other devices at runtime.
            RegisterCallback<NavigationSubmitEvent>(evt => OnSubmit(evt));
        }

        static void OnClick(ClickEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggleControlJoystickPos;
            slideToggle.ToggleValue();

            evt.StopPropagation();
            FMODAudioManager.instance.UIButtonClickedPositive();

        }

        static void OnSubmit(NavigationSubmitEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggleControlJoystickPos;
            slideToggle.ToggleValue();

            evt.StopPropagation();
            FMODAudioManager.instance.UIButtonClickedPositive();

        }

        static void OnKeydownEvent(KeyDownEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggleControlJoystickPos;

            // NavigationSubmitEvent event already covers keydown events at runtime, so this method shouldn't handle
            // them.
            if (slideToggle.panel?.contextType == ContextType.Player)
                return;

            // Toggle the value only when the user presses Enter, Return, or Space.
            if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
            {
                slideToggle.ToggleValue();
                evt.StopPropagation();
                FMODAudioManager.instance.UIButtonClickedPositive();

            }
        }

        // All three callbacks call this method.
        public void ToggleValue()
        {
            value = !value;
            ValueChanged?.Invoke(value);
        }

        // Because ToggleValue() sets the value property, the BaseField class dispatches a ChangeEvent. This results in a
        // call to SetValueWithoutNotify(). This example uses it to style the toggle based on whether it's currently
        // enabled.
        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);

            //This line of code styles the input element to look enabled or disabled.
            m_Input.EnableInClassList(inputCheckedUssClassName, newValue);
        }
    }

    public class SlideToggleControlVibration : BaseField<bool>
    {
        public new class UxmlFactory : UxmlFactory<SlideToggleControlVibration, UxmlTraits> { }

        public new class UxmlTraits : BaseFieldTraits<bool, UxmlBoolAttributeDescription> { }

        // In the spirit of the BEM standard, the SlideToggle has its own block class and two element classes. It also
        // has a class that represents the enabled state of the toggle.
        public static readonly new string ussClassName = "slide-toggle";
        public static readonly new string inputUssClassName = "slide-toggle__input";
        public static readonly string inputKnobUssClassName = "slide-toggle__input-knob";
        public static readonly string inputCheckedUssClassName = "slide-toggle__input--checked";

        public event Action<bool> ValueChanged;

        VisualElement m_Input;
        VisualElement m_Knob;

        // Custom controls need a default constructor. This default constructor calls the other constructor in this
        // class.
        public SlideToggleControlVibration() : this(null) { }

        // This constructor allows users to set the contents of the label.
        public SlideToggleControlVibration(string label) : base(label, null)
        {
            // Style the control overall.
            AddToClassList(ussClassName);

            // Get the BaseField's visual input element and use it as the background of the slide.
            m_Input = this.Q(className: BaseField<bool>.inputUssClassName);
            m_Input.name = "input";
            m_Input.AddToClassList(inputUssClassName);
            Add(m_Input);

            // Create a "knob" child element for the background to represent the actual slide of the toggle.
            m_Knob = new();
            m_Knob.name = "knob";
            m_Knob.AddToClassList(inputKnobUssClassName);
            m_Input.Add(m_Knob);

            // There are three main ways to activate or deactivate the SlideToggle. All three event handlers use the
            // static function pattern described in the Custom control best practices.

            // ClickEvent fires when a sequence of pointer down and pointer up actions occurs.
            RegisterCallback<ClickEvent>(evt => OnClick(evt));
            // KeydownEvent fires when the field has focus and a user presses a key.
            RegisterCallback<KeyDownEvent>(evt => OnKeydownEvent(evt));
            // NavigationSubmitEvent detects input from keyboards, gamepads, or other devices at runtime.
            RegisterCallback<NavigationSubmitEvent>(evt => OnSubmit(evt));
        }

        static void OnClick(ClickEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggleControlVibration;
            slideToggle.ToggleValue();

            evt.StopPropagation();
            FMODAudioManager.instance.UIButtonClickedPositive();
        }

        static void OnSubmit(NavigationSubmitEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggleControlVibration;
            slideToggle.ToggleValue();

            evt.StopPropagation();
            FMODAudioManager.instance.UIButtonClickedPositive();

        }

        static void OnKeydownEvent(KeyDownEvent evt)
        {
            var slideToggle = evt.currentTarget as SlideToggleControlJoystickPos;

            // NavigationSubmitEvent event already covers keydown events at runtime, so this method shouldn't handle
            // them.
            if (slideToggle.panel?.contextType == ContextType.Player)
                return;

            // Toggle the value only when the user presses Enter, Return, or Space.
            if (evt.keyCode == KeyCode.KeypadEnter || evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.Space)
            {
                slideToggle.ToggleValue();
                evt.StopPropagation();
                FMODAudioManager.instance.UIButtonClickedPositive();

            }
        }

        // All three callbacks call this method.
        public void ToggleValue()
        {
            value = !value;
            ValueChanged?.Invoke(value);
        }

        // Because ToggleValue() sets the value property, the BaseField class dispatches a ChangeEvent. This results in a
        // call to SetValueWithoutNotify(). This example uses it to style the toggle based on whether it's currently
        // enabled.
        public override void SetValueWithoutNotify(bool newValue)
        {
            base.SetValueWithoutNotify(newValue);

            //This line of code styles the input element to look enabled or disabled.
            m_Input.EnableInClassList(inputCheckedUssClassName, newValue);
        }
    }

}