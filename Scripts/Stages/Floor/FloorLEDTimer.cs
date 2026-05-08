using UnityEngine;
using System.Collections;

public class FloorLEDTimer : MonoBehaviour
{
    [SerializeField]
    private float timerValueWhenDefeatBoss = 5.0f;

    [SerializeField]
    private FloatEventChannelSO setFloorTimerEC;

    [SerializeField]
    private EventChannelSO floorTimerEndEC;

    [SerializeField]
    private EventChannelSO playerDeadEC, playerReviveEC;

    [SerializeField]
    private EventChannelSO defeatBossEC;

    [SerializeField]
    private EventChannelSO playerFallEC;

    private FloorLEDBuilder floorBuilder;
    private bool stop;


    private void Awake()
    {
        floorBuilder = GetComponent<FloorLEDBuilder>();
        stop = false;
    }

    private void OnEnable()
    {
        setFloorTimerEC.Subscribe(StartFloorTimer);
        defeatBossEC.Subscribe(StartTimerWhenDefeatBoss);
        playerDeadEC.Subscribe(OnPlayerDead);    
        playerReviveEC.Subscribe(OnPlayerRevive);
        playerFallEC.Subscribe(OnPlayerFall);
    }

    private void OnDisable()
    {
        setFloorTimerEC.Unsubscribe(StartFloorTimer);
        defeatBossEC.Unsubscribe(StartTimerWhenDefeatBoss);
        playerDeadEC.Unsubscribe(OnPlayerDead);
        playerReviveEC.Unsubscribe(OnPlayerRevive);
        playerFallEC.Unsubscribe(OnPlayerFall);
    }

    private void OnPlayerDead()
    {
        stop = true;    
    }

    private void OnPlayerRevive()
    {
        stop= false;
    }

    private void OnPlayerFall()
    {
        StopAllCoroutines();
    }

    public void StartFloorTimer(float seconds)
    {
        // ���������� �� -> ���ϴ� �ð��� �����ϱ� ���� �� ƽ�� ���
        int floorSize = floorBuilder.Size;
        int outterLayer = floorBuilder.GetOutterLayer();
        float tickCount = floorSize * (floorSize + 1) * 0.5f;
        WaitForSeconds tick = new WaitForSeconds(seconds / tickCount);

        StartCoroutine(StartTimer(tick, maxLayer: outterLayer));
    }

    private void StartTimerWhenDefeatBoss()
    {
        StartFloorTimer(timerValueWhenDefeatBoss);
    }

    private IEnumerator StartTimer(WaitForSeconds tick, int maxLayer)
    {
        int currentLayer = 0;

        while (maxLayer >= 0)
        {
            currentLayer++;

            while (currentLayer <= maxLayer)
            {
                yield return tick;

                while (stop)
                {
                    yield return null;
                }

                currentLayer++;
            }

            yield return tick;

            floorBuilder.TurnOnLayerWorkTrigger();

            currentLayer = 0;
            maxLayer--;

            while (stop)
            {
                yield return null;
            }
        }
    }
}