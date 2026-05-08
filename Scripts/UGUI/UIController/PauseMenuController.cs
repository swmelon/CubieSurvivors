using System;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Canvas))]
public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private BooleanEventChannelSO pauseMenuUiControlChannel;
    [SerializeField] private GamePauser gamePauser; // Serialized reference to the GamePauser object
    [SerializeField] private Button resumeButton; // Serialized reference to the resume button in the pause menu UI
    [SerializeField] private Button restartButton; // Serialized reference to the restart button in the pause menu UI
    [SerializeField] private Button quitButton; // Serialized reference to the quit button in the pause menu UI
    [SerializeField] private SaveLoadManagerSO saveLoadManagerSO;
    
    private Canvas canvas;
    private bool locked;

    private void Awake()
    {
        pauseMenuUiControlChannel.Subscribe(SetSwitch);
        canvas = GetComponent<Canvas>();
        canvas.enabled = false;
    }

    private void Start()
    {
        // Add event listeners to the buttons in the pause menu UI
        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void Update()
    {
        if (locked)
        {
            return;
        }
        
        // Check if the player has pressed the pause button (default is "Escape")
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gamePauser.Pause)
            {
                // If the game is already paused, unpause the game and hide the pause menu UI
                ResumeGame();
            }
            else
            {
                // If the game is not already paused, pause the game and show the pause menu UI
                PauseGame();
            }
        }
    }

    private void SetSwitch(bool val)
    {
        if (!val)
        {
            locked = true;
            canvas.enabled = false;
        }
        else
        {
            locked = false;
        }
        
    }

    private void PauseGame()
    {
        if (locked)
        {
            return;
        }
        
        // Pause the game using the gamePauser object
        gamePauser.Pause = true;

        // Show the pause menu UI
        canvas.enabled = true;

        // Disable the cursor so the player can't click on anything outside of the pause menu
        // Cursor.lockState = CursorLockMode.Confined;
        // Cursor.visible = true;
    }

    private void ResumeGame()
    {
        // Unpause the game using the gamePauser object
        gamePauser.Pause = false;

        // Hide the pause menu UI
        canvas.enabled = false;

        // Enable the cursor again
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }

    private void RestartGame()
    {
        saveLoadManagerSO.Save();
        // Restart the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    private void QuitGame()
    {
        saveLoadManagerSO.Save();
        // Quit the game
        Application.Quit();
    }

    private void OnDestroy()
    {
        pauseMenuUiControlChannel.Unsubscribe(SetSwitch);
    }
}
