using StarterAssets;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutSceneDirector : MonoBehaviour
{
    [SerializeField]
    private ScoreScreenController scoreScreenController;

    [SerializeField]
    private StageRewardScreenController stageRewardScreenController;

    [Header("dependency")]
    [SerializeField]
    private TimelineAsset[] ta;

    [SerializeField]
    private ScreenFilter screenFilter;

    [SerializeField]
    private CameraController cameraController;

    [SerializeField]
    private GameObject megaExplosion;

    [Header("Channels")]
    [SerializeField]
    private EventChannelSO defeatFinalBossEC;

    [SerializeField]
    private EventChannelSO endBossDefeatCutSceneEC;

    [SerializeField]
    private PlayerChannelSO playerChannel;

    [SerializeField]
    private EventChannelSO getCharacterUnlockItemEC, startCharacterUnlockCutSceneEC, endCharacterUnlockCutSceneEC;

    [SerializeField]
    private BooleanEventChannelSO accessoryInventoryScreenControlChannel;

    [SerializeField]
    private EventChannelSO enterShelfEC, exitShelfEC;

    [SerializeField]
    private EventChannelSO enterShopEC, exitShopEC;

    [SerializeField]
    private FloatEventChannelSO vignetteValueControlChannel;

    [SerializeField]
    private BossChannelSO bossChannel;

    [SerializeField]
    private BooleanEventChannelSO shopUIControlChannel; 


    [SerializeField]
    private EventStageChannelSO eventStageChannel;

    [SerializeField]
    private EventChannelSO enterEventStageEC;

    [SerializeField]
    private GameProcessManager gameProcessManager;

    private PlayableDirector pd;
    private Player player;
    private CustomThirdPersonController playerController;
    private Boss boss;    
    private EventStage eventStage;
    private GameObject megaExplosionInstance;

    private bool isPlaying = false;

    private bool completeScenarioFirstTimeTriggered = false; 
    private void Awake()
    {
        pd = GetComponent<PlayableDirector>();
    }


    private void OnEnable()
    {
        defeatFinalBossEC.Subscribe(OnDefeatFinalBoss);
        playerChannel.Subscribe(SetPlayer);
        bossChannel.Subscribe(SetBoss);
        endBossDefeatCutSceneEC.Subscribe(OnEnterEventStage);
        getCharacterUnlockItemEC.Subscribe(StartCharacterUnlockCutScene);
        endCharacterUnlockCutSceneEC.Subscribe(OnCharacterUnlockCutSceneEnd);
        enterShelfEC.Subscribe(PlayEnterShelfCutScene);
        exitShelfEC.Subscribe(PlayExitShelfCutScene);
        eventStageChannel.Subscribe(SetEventStage);
        enterShopEC.Subscribe(PlayEnterShopCutScene);
        exitShopEC.Subscribe(PlayExitShopCutScene);
        enterEventStageEC.Subscribe(OnEnterEventStage);
    }

    private void OnDisable()
    {
        defeatFinalBossEC.Unsubscribe(OnDefeatFinalBoss);
        playerChannel.Unsubscribe(SetPlayer);
        bossChannel.Unsubscribe(SetBoss);
        endBossDefeatCutSceneEC.Unsubscribe(OnEnterEventStage);
        getCharacterUnlockItemEC.Unsubscribe(StartCharacterUnlockCutScene);
        endCharacterUnlockCutSceneEC.Unsubscribe(OnCharacterUnlockCutSceneEnd);
        enterShelfEC.Unsubscribe(PlayEnterShelfCutScene);
        exitShelfEC.Unsubscribe(PlayExitShelfCutScene);
        eventStageChannel.Unsubscribe(SetEventStage);
        enterShopEC.Unsubscribe(PlayEnterShopCutScene);
        exitShopEC.Unsubscribe(PlayExitShopCutScene);
        enterEventStageEC.Unsubscribe(OnEnterEventStage);
    }

    private void OnDefeatFinalBoss()
    {
        if (isPlaying)
        {
            return;
        }

        isPlaying = true;
        StartCoroutine(FadeOutAndShowScore());
    }

    private IEnumerator FadeOutAndShowScore()
    {
        screenFilter.FadeOut();
        yield return new WaitForSeconds(1.5f);

        // move player to somewhere
        // move boss to center

        cameraController.enabled = false;
        player.gameObject.SetActive(false);

        ShowScoreScreen();
    }

    private void AfterShowScoreScreen()
    {
        stageRewardScreenController.ShowStageRewardScreen();
    }

    public void StartDefeatFinalBossCutScene()
    {
        StartCoroutine(DefeatBossCutScene());
    }

    private IEnumerator DefeatBossCutScene(){ 
        yield return new WaitForSeconds(1.5f);
        cameraController.Camera.clearFlags = CameraClearFlags.Skybox;

        boss.transform.position = new Vector3(0, 1, 0);
        boss.transform.rotation = Quaternion.Euler(0, 180, 0);
        pd.Play(ta[0]);
        megaExplosionInstance = Instantiate(megaExplosion, new Vector3(0, 1.2f, 0), Quaternion.identity);

        yield return new WaitForSeconds((float)ta[0].duration);
        // wait for timeline to finish

        pd.Stop();
        Destroy(boss.gameObject);
        player.gameObject.SetActive(true);

        isPlaying = false;
        Destroy(megaExplosionInstance);
        endBossDefeatCutSceneEC.Raise();
        cameraController.Camera.clearFlags = CameraClearFlags.SolidColor;
    }

    private void SetPlayer(Player currentPlayer)
    {
        if (currentPlayer == null)
        {
            return;
        }

        player = currentPlayer;
        playerController = player.GetComponent<CustomThirdPersonController>();
    }

    private void SetBoss(Boss currentBoss)
    {
        boss = currentBoss;
    }

    public void OnEnterEventStage()
    {
        cameraController.enabled = true;
        cameraController.FixedOthograpicMode();
        screenFilter.FadeInWhite();
    }

    public void StartCharacterUnlockCutScene()
    {
        if (isPlaying)
        {
            return;
        }

        isPlaying = true;
        StartCoroutine(CharacterUnlockCutScene());
    }

    private IEnumerator CharacterUnlockCutScene()
    {
        screenFilter.FadeOut();
        playerController.IgnoreInput();

        yield return new WaitForSeconds(1.5f);

        cameraController.enabled = false;

        yield return new WaitForSeconds(1.5f);

        startCharacterUnlockCutSceneEC.Raise();
        pd.Play(ta[1]);

        yield return new WaitForSeconds((float)ta[1].duration);
        // wait for timeline to finish

        pd.Stop();
        playerController.ListenInput();

        endCharacterUnlockCutSceneEC.Raise();
        isPlaying = false;
    }

    private void OnCharacterUnlockCutSceneEnd()
    {
        cameraController.enabled = true;
        screenFilter.FadeInWhite();
    }

    private void PlayEnterShelfCutScene()
    {
        if (isPlaying)
        {
            return;
        }

        isPlaying = true;
        StartCoroutine(EnterShelfCutSceneCoroutine());
    }

    private IEnumerator EnterShelfCutSceneCoroutine()
    {
        screenFilter.FadeOut();
        playerController.IgnoreInput();

        yield return new WaitForSeconds(1.5f);


        playerController.PutAwayCharacter();
        cameraController.SplitScreen();
        screenFilter.FadeIn();
        vignetteValueControlChannel.Raise(0.2f);
        
        accessoryInventoryScreenControlChannel.Raise(true);
        isPlaying = false;
    }

    private void PlayExitShelfCutScene()
    {
        if (isPlaying)
        {
            return;
        }

        isPlaying = true;
        StartCoroutine(ExitShelfCutSceneCoroutine());
    }

    private IEnumerator ExitShelfCutSceneCoroutine() 
    {    
        screenFilter.FadeOut();
        playerController.IgnoreInput();

        yield return new WaitForSeconds(1f);

        playerController.MoveOnlyCharacterTo(eventStage.initialCharacterPosition);
        eventStage.ExitFocusZone();
        accessoryInventoryScreenControlChannel.Raise(false);
        cameraController.MergeScreen();

        yield return new WaitForSeconds(1f);

        playerController.IgnoreInputUntillHitGround();
        
        screenFilter.FadeIn();
        vignetteValueControlChannel.Raise(0.0f);
        isPlaying = false;
    }

    private void SetEventStage(EventStage currentEventStage)
    {
        eventStage = currentEventStage;
    }

    private void PlayEnterShopCutScene()
    {
        if (isPlaying)
        {
            return;
        }

        isPlaying = true;
        StartCoroutine(EnterShopCoroutine());
    }

    private IEnumerator EnterShopCoroutine()
    {
        screenFilter.FadeOut();
        playerController.IgnoreInput();

        yield return new WaitForSeconds(1.5f);

        playerController.PutAwayCharacter();
        screenFilter.FadeIn();
        vignetteValueControlChannel.Raise(0.2f);
        shopUIControlChannel.Raise(true);

        isPlaying = false;
    }

    private void PlayExitShopCutScene()
    {
        if (isPlaying)
        {
            return;
        }

        isPlaying = true;
        StartCoroutine(ExitShopCoroutine());
    }

    private IEnumerator ExitShopCoroutine()
    {
        screenFilter.FadeOut();
        playerController.IgnoreInput();

        yield return new WaitForSeconds(1f);

        playerController.MoveOnlyCharacterTo(eventStage.initialCharacterPosition);
        eventStage.ExitFocusZone();
        shopUIControlChannel.Raise(false);

        yield return new WaitForSeconds(1f);

        playerController.IgnoreInputUntillHitGround();

        screenFilter.FadeIn();
        vignetteValueControlChannel.Raise(0.0f);
        isPlaying = false;
    }

    private void ShowScoreScreen()
    {
        scoreScreenController.ShowScoreScreen(UIText.CONTINUE, AfterShowScoreScreen);
    }

    private void OnCompleteScenarioFirstTime()
    {
        completeScenarioFirstTimeTriggered = true;
    }
}