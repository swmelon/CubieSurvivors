using System;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "GamePauser", menuName = "ScriptableObjects/GamePauser", order = 2)]
public class GamePauser : ScriptableObject, IDependentInitialization
{
    [FormerlySerializedAs("turnOnOffLightEventChannel")] [SerializeField]
    private BooleanEventChannelSO GamePausedEC;
    
    private bool pause = false;

    public bool Pause
    {
        get => pause;
        set => SetSwitch(value);
    }

    public void Initialize()
    {
        pause = false;
    }
    private void SetSwitch(bool val)
    {
        Time.timeScale = val ? 0 : 1;
        GamePausedEC.Raise(val);
        pause = val;
    }
}