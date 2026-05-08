using UnityEngine;

public class FocusTrigger : ZoneTrigger
{
    [SerializeField]
    private EventChannelSO enterFocusZoneEC, exitFocusZoneEC;

    public Vector3 focusPos;
    public Vector3 cameraPos;

    protected override void Awake()
    {
        base.Awake();

        PlayerEnter += OnPlayerEnter;
        PlayerExit += OnPlayerExit;
    }

    private void OnPlayerEnter(ZoneTrigger obj)
    {
        enterFocusZoneEC?.Raise();
    }

    private void OnPlayerExit(ZoneTrigger obj)
    {
        exitFocusZoneEC?.Raise();
    }

}