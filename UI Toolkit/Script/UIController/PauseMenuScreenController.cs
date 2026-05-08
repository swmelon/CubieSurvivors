
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuScreenController : MonoBehaviour
{
    [SerializeField] private PauseMenuScreen pauseMenuScreen;
    [SerializeField] private MainMenuUIManager mainMenuUIManager;
    [SerializeField] private BooleanEventChannelSO listenPauseActionControlChannel;
    [SerializeField] private GamePauser gamePauser;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private PopupScreenController popupScreenController;


    private InputAction pause;
    private GameProcessManager gameStartupManager;
    private bool locked;
    private bool showingPopupScreen;

    private void Start()
    {
        gameStartupManager = GetComponent<GameProcessManager>();
        pause = inputActions.FindAction("Pause");
        listenPauseActionControlChannel.Subscribe(ToggleState);

        pauseMenuScreen.ScreenStarted += UnlockListenChannel;
        pauseMenuScreen.ResumeButtonClicked += ResumeGame;
        pauseMenuScreen.SettingsButtonClicked += ShowSettingsScreen;
        pauseMenuScreen.RestartButtonClicked += ShowRestartPopupScreen;
        pauseMenuScreen.QuitButtonClicked += ShowQuitPopupScreen;
    }

    private void OnDestroy()
    {
        listenPauseActionControlChannel.Unsubscribe(ToggleState);

        pauseMenuScreen.ScreenStarted -= UnlockListenChannel;
        pauseMenuScreen.ResumeButtonClicked -= ResumeGame;
        pauseMenuScreen.SettingsButtonClicked -= ShowSettingsScreen;
        pauseMenuScreen.RestartButtonClicked -= ShowRestartPopupScreen;
        pauseMenuScreen.QuitButtonClicked -= ShowQuitPopupScreen;

    }

    private void ToggleState(bool val)
    {
        if (!val)
        {
            locked = true;
        }
        else
        {
            locked = false;
        }
    }

    private void Update()
    {
        if (locked || showingPopupScreen)
        {
            return;
        }

        if (pause.triggered)
        {
            if (gamePauser.Pause)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }



    public void PauseGame()
    {
        if (locked)
        {
            return;
        }

        // Pause the game using the gamePauser object
        gamePauser.Pause = true;

        
        // Show the pause menu UI
        pauseMenuScreen.ShowScreen();

        // Disable the cursor so the player can't click on anything outside of the pause menu
        // Cursor.lockState = CursorLockMode.Confined;
        // Cursor.visible = true;
    }

    private void ResumeGame()
    {
        // Unpause the game using the gamePauser object
        gamePauser.Pause = false;

        // Hide the pause menu UI
        pauseMenuScreen.HideScreen();

        // Enable the cursor again
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void ShowSettingsScreen()
    {
        mainMenuUIManager.ShowSettingsScreen(shownByHomeScreen: false);
        LockListenChannel();
    }

    private void RestartGame()
    {
      gameStartupManager.RestartGame();
    }

    private void ShowRestartPopupScreen()
    {
        //pauseMenuScreen.HideScreen();
        showingPopupScreen = true;
        popupScreenController.ShowPopupScreen(UIText.RESTART_MESSAGE, UIText.YES, UIText.NO, RestartGame, CancelPopup);
    }

    private void ShowQuitPopupScreen()
    {
        //pauseMenuScreen.HideScreen();
        showingPopupScreen = true;
        popupScreenController.ShowPopupScreen(UIText.QUIT_MESSAGE, UIText.YES, UIText.NO, QuitGame, CancelPopup);
    }

    private void QuitGame()
    {
        gameStartupManager.QuitGame();
    }

    private void CancelPopup()
    {
        showingPopupScreen = false;
    }

    private void LockListenChannel()
    {
        listenPauseActionControlChannel.Raise(false);
    }

    private void UnlockListenChannel()
    {
        listenPauseActionControlChannel.Raise(true);
    }
}
