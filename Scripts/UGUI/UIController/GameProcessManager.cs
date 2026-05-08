using UnityEngine;
using UnityEngine.UI;


public class GameProcessManager : MonoBehaviour
{
    [SerializeField] 
    private BooleanEventChannelSO pauseMenuUIControlChannel, upgradeCanvasControlChannel, ingameUIControlChannel, pauseButtonUIControlChannel;
    [SerializeField] 
    private BooleanEventChannelSO userInputControlChannel;

    [SerializeField]
    private EventChannelSO gameStartEventChannel;

    [SerializeField]
    private EventChannelSO eventStagePortalActivatedEC;

    [SerializeField]
    private EventChannelSO exitEventStageEC;

    [SerializeField] 
    private SaveLoadManagerSO saveLoadManagerSO;


    [SerializeField]
    MainMenuUIManager mainMenuUIManager;

    [SerializeField]
    private AccessoryStatsSelectionScreenController accStatsSelectionScreenController;

    [SerializeField] 
    private Canvas worldCanvas, controlCanvas;
    
    // The button prefab to use for each menu item
    public Button buttonPrefab;

    private Button[] buttons;

    // flag use to restart game when exit event stage (if game was started)
    private bool gameStarted = false;
    private bool forceQuit = true;

    private void OnEnable()
    {
        eventStagePortalActivatedEC.Subscribe(OnPortalActivated);
    }

    private void OnDisable()
    {
        eventStagePortalActivatedEC.Unsubscribe(OnPortalActivated);
    }

    private void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        SetupInitialSettings();
    }
    public void StartGame()
    {
        // enable world canvas

        worldCanvas.enabled = true;
        
        // enable player control
        
    

        // move camera to game mode

        
        controlCanvas.enabled = true;
        userInputControlChannel.Raise(true);
        pauseMenuUIControlChannel.Raise(true);
        ingameUIControlChannel.Raise(true);
        pauseButtonUIControlChannel.Raise(true);
        gameStartEventChannel.Raise();
        gameStarted = true;
    }

    private void SetupInitialSettings()
    {
        userInputControlChannel.Raise(false);
        upgradeCanvasControlChannel.Raise(false);
        ingameUIControlChannel.Raise(false);
        worldCanvas.enabled = false;
        controlCanvas.enabled = false;
        
        if (saveLoadManagerSO.HasPendingCardSelection())
        {
            accStatsSelectionScreenController.ShowPendingSelectionScreen();
        }
        else
        {
            mainMenuUIManager.ShowHomeScreen();
        }

        pauseButtonUIControlChannel.Raise(false);
    }

    public void RestartGame()
    {
        saveLoadManagerSO.Save();
        // Restart the current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        forceQuit = false;
        saveLoadManagerSO.Save();
        // Quit the game
        Application.Quit();
    }

    private void OnPortalActivated()
    {
        if (gameStarted)
        {
            RestartGame();
        }
        else
        {
            exitEventStageEC.Raise();
            SetupInitialSettings();
        }
    }


    private void MultiplayButtonClicked()
    {
        
    }
    
    private void RankingButtonClicked()
    {
        
    }
    
    private void CollectionButtonClicked()
    {
        
    }

}