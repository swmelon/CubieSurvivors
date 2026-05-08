
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;


[RequireComponent(typeof(UIDocument))]
public class MainMenuUIManager : MonoBehaviour
{
    [Header("Modal Menu Screens")]
    [Tooltip("Only one modal interface can appear on-screen at a time.")]
    [SerializeField] HomeScreen homeModalScreen;
    [SerializeField] CharSelectionScreen charModalScreen;
    [SerializeField] UpgradeScreen collectionModalScreen;
    // [SerializeField] InfoScreen m_InfoModalScreen;
    // [SerializeField] ShopScreen m_ShopModalScreen;
    // [SerializeField] MailScreen m_MailModalScreen;

    // [Header("Toolbars")]
    // [Tooltip("Toolbars remain active at all times unless explicitly disabled.")]
    // [SerializeField] OptionsBar m_OptionsToolbar;
    // [SerializeField] MenuBar m_MenuToolbar;

    [Header("Full-screen overlays")]
    [Tooltip("Full-screen overlays block other controls until dismissed.")]
    // [SerializeField] MenuScreen m_InventoryScreen;
    [SerializeField] SettingsScreen settingsModalScreen;

    [SerializeField] PauseMenuScreen pauseMenuScreen;

    [SerializeField] AccessoryInventoryScreen accessoryInventoryScreen;

    private List<MenuScreen> allModalScreens = new List<MenuScreen>();
    private UIDocument mainMenuDocument;
    public UIDocument MainMenuDocument => mainMenuDocument;

    void OnEnable()
    {
        mainMenuDocument = GetComponent<UIDocument>();
        SetupModalScreens();
        //ShowHomeScreen();
    }

    void Start()
    {
        Time.timeScale = 1f;
    }

    void SetupModalScreens()
    {
        if (homeModalScreen != null)
            allModalScreens.Add(homeModalScreen);

        if (charModalScreen != null)
            allModalScreens.Add(charModalScreen);

        if (collectionModalScreen != null)
            allModalScreens.Add(collectionModalScreen);

        if (settingsModalScreen != null)
            allModalScreens.Add(settingsModalScreen);

        if (pauseMenuScreen != null)
            allModalScreens.Add(pauseMenuScreen);

        if (accessoryInventoryScreen != null)
            allModalScreens.Add(accessoryInventoryScreen);
        
        // if (m_ShopModalScreen != null)
        //     allModalScreens.Add(m_ShopModalScreen);
        //
        // if (m_MailModalScreen != null)
        //     allModalScreens.Add(m_MailModalScreen);
    }

    // shows one screen at a time
    void ShowModalScreen(MenuScreen modalScreen)
    {
        foreach (MenuScreen m in allModalScreens)
        {
            if (m == modalScreen)
            {
                m?.ShowScreen();
            }
            else
            {
                m?.HideScreen();
            }
        }
    }

    // methods to toggle screens on/off

    // modal screen methods 
    public void ShowHomeScreen()
    {
        ShowModalScreen(homeModalScreen);
    }

    // note: screens with tabbed menus default to showing the first tab
    public void ShowCharScreen()
    {
        ShowModalScreen(charModalScreen);
    }
    
    public void ShowCollectionScreen()
    {
        ShowModalScreen(collectionModalScreen);
    }

    // public void ShowInfoScreen()
    // {
    //     ShowModalScreen(m_InfoModalScreen);
    // }
    //
    // public void ShowShopScreen()
    // {
    //     ShowModalScreen(m_ShopModalScreen);
    // }
    //
    // // opens the Shop Screen directly to a specific tab (e.g. to gold or gem shop) from the Options Bar
    // public void ShowShopScreen(string tabName)
    // {
    //     m_MenuToolbar?.ShowShopScreen();
    //     m_ShopModalScreen?.SelectTab(tabName);
    // }

    // public void ShowMailScreen()
    // {
    //     ShowModalScreen(m_MailModalScreen);
    // }

    // overlay screen methods
    public void ShowSettingsScreen(bool shownByHomeScreen= true)
    {
        ShowModalScreen(settingsModalScreen);
        settingsModalScreen.SetShownByHomeScreen(shownByHomeScreen);
        //settingsScreen?.ShowScreen();
    }

    public void ShowPauseMenuScreen()
    {
        ShowModalScreen(pauseMenuScreen);
    }

    // public void ShowInventoryScreen()
    // {
    //     m_InventoryScreen?.ShowScreen();
    // }
}
