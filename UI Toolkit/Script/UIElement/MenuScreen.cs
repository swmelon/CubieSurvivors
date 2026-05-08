using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.Localization.Settings;


// This is a base class for a functional unit of the Main Menu (a panel or a full-screen UI).

// In this example, a MenuScreen could include:
//      *full-screen UIs, e.g. HomeScreen, CharScreen, InfoScreen, ShopScreen, MailScreen, and OptionScreens
//      *toolbars, e.g. the MenuBar (left) and OptionsBar (upper right)

// A MenuScreen can be part of a larger UIDocument or use a separate UIDocument.

public abstract class MenuScreen : MonoBehaviour
{
    [FormerlySerializedAs("m_ScreenName")]
    [Tooltip("String ID from the UXML for this menu panel/screen.")]
    [SerializeField] 
    protected string screenName;

    [FormerlySerializedAs("m_MainMenuUIManager")]
    [Header("UI Management")]
    [Tooltip("Set the Main Menu here explicitly (or get automatically from current GameObject).")]
    [SerializeField] 
    protected MainMenuUIManager mainMenuUIManager;
    
    [FormerlySerializedAs("m_Document")]
    [Tooltip("Set the UI Document here explicitly (or get automatically from current GameObject).")]
    [SerializeField] 
    protected UIDocument document;

    [SerializeField]
    private EventChannelSO resetVisualEvent;

    [SerializeField]
    protected FontManager fontManager;

    // visual elements
    protected VisualElement screen;
    protected VisualElement root;

    public event Action ScreenStarted;
    public event Action ScreenEnded;



    //  UXML element name (defaults to the class name)
    protected virtual void OnValidate()
    {
        if (string.IsNullOrEmpty(screenName))
            screenName = this.GetType().Name;
    }

    protected virtual void Awake()
    {
        // set up MainMenuUIManager and UI Document
        if (mainMenuUIManager == null)
            mainMenuUIManager = GetComponent<MainMenuUIManager>();

        // default to current UIDocument if not set in Inspector
        if (document == null)
            document = GetComponent<UIDocument>();

        // alternately falls back to the MainMenu UI Document
        if (document == null && mainMenuUIManager != null)
            document = mainMenuUIManager.MainMenuDocument;

        if (document == null)
        {
            Debug.LogWarning("MenuScreen " + screenName + ": missing UIDocument. Check Script Execution Order.");
            return;
        }
        else
        {
            SetVisualElements();
            RegisterButtonCallbacks();
        }
    }

    private void OnEnable()
    {
        resetVisualEvent?.Subscribe(SetVisualElements);
    }

    private void OnDisable()
    {
        resetVisualEvent?.Unsubscribe(SetVisualElements);
    }

    // The general workflow uses string IDs to query the VisualTreeAsset and find matching Visual Elements in the UXML.
    // Customize this for each MenuScreen subclass to identify any functional Visual Elements (buttons, controls, etc.).
    protected virtual void SetVisualElements()
    {
        // get a reference to the root VisualElement 
        if (document != null)
            root = document.rootVisualElement;

        screen = GetVisualElement(screenName);
    }

    // Once you have the VisualElements, you can add button events here, using the RegisterCallback functionality. 
    // This allows you to use a number of different events (ClickEvent, ChangeEvent, etc.)
    protected virtual void RegisterButtonCallbacks()
    {

    }

    public bool IsVisible()
    {
        if (screen == null)
            return false;
        
        return (screen.style.display == DisplayStyle.Flex);
    }

    // Toggle a UI on and off using the DisplayStyle. 
    public static void ShowVisualElement(VisualElement visualElement, bool state)
    {
        if (visualElement == null)
            return;

        visualElement.style.display = (state) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // returns an element by name
    public VisualElement GetVisualElement(string elementName)
    {
        if (string.IsNullOrEmpty(elementName) || root == null)
            return null;

        // query and return the element
        return root.Q(elementName);
    }

    public virtual void ShowScreen()
    {
        SetButtonsFont();
        SetLabelsFont();
        ShowVisualElement(screen, true);
        SetLocalizedText();
        ScreenStarted?.Invoke();
    }

    public virtual void HideScreen()
    {
        if (IsVisible())
        {
            ShowVisualElement(screen, false);
            ScreenEnded?.Invoke();
        }
    }
    
    protected static void DestroyVisualElement(VisualElement visualElement)
    {
        if (!ReferenceEquals(visualElement, null))
        {
            visualElement.RemoveFromHierarchy();
        }
    }

    public void ResetVisualElement()
    {
        SetVisualElements();
    }

    public void HandleButtonClickPositiveSFX()
    {
        FMODAudioManager.instance.UIButtonClickedPositive();
    }

    public void HandleButtonClickNegativeSFX()
    {
        FMODAudioManager.instance.UIButtonClickedNegative();
    }

    protected void SetButtonsFont()
    {
        if (fontManager == null)
            return;

        var buttons = screen.Query<Button>().ToList();

        // Get the current font from the FontManager
        var fontDefinition = fontManager.GetCurrentUIStyleFont();


        // Set the font for each button
        foreach (var button in buttons)
        {
            button.style.unityFontDefinition = fontDefinition;
        }
    }


    protected void SetLabelsFont()
    {
        if (fontManager == null)
            return;

        var labels = screen.Query<Label>().ToList();

        // Get the current font from the FontManager
        var fontDefinition = fontManager.GetCurrentUIStyleFont();

        // Set the font for each label
        foreach (var label in labels)
        {
            label.style.unityFontDefinition = fontDefinition;
        }
    }


    /// <summary>
    /// Isn't called in the base class, but can be.
    /// </summary>
    protected void SetBarsFont()
    {
        var bars = screen.Query<ProgressBar>().ToList();

        // Get the current font from the FontManager
        var fontDefinition = fontManager.GetCurrentUIStyleFont();

        // Set the font for each label
        foreach (var bar in bars)
        {
            bar.style.unityFontDefinition = fontDefinition;
        }
    }

    protected virtual void SetLocalizedText()
    {
        // Override this method in derived classes to set localized text
    }

    protected string GetLocalizedString(string key)
    {
        return LocalizationSettings.StringDatabase.GetLocalizedString(LocalizationTableName.TABLE_UI_TEXT, key);
    }
}