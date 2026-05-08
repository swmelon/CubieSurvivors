using UnityEngine;

public class DeathCounter : NumberTrackingUI
{
    [SerializeField]
    private DeathManager deathManager;


    protected override void Awake()
    {
        base.Awake();
        deathManager.DeathCountChanged += SetNumber;
    }

    private void OnDestroy()
    {
        deathManager.DeathCountChanged -= SetNumber;
    }
}