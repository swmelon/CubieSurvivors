using FMODUnity;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Collections.Generic;
using System.Linq;



[CreateAssetMenu(fileName = "FMODEvents", menuName = "ScriptableObjects/FMODEvents", order = 1)]

public class FMODEvents : ScriptableObject, IDependentInitialization
{
    [field: Header("Ambience")]
    [field: SerializeField] public EventReference ambience { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference music { get; private set; }

    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerFootsteps { get; private set; }

    [field: Header("Coin SFX")]
    [field: SerializeField] public EventReference coinCollected { get; private set; }
    [field: SerializeField] public EventReference coinIdle { get; private set; }

    [SerializedDictionary("SFX tag", "Event Reference")]
    public SerializedDictionary<SFXTags, EventReference> SFXEvents;

    private Dictionary<SFXTags, EventReference> sfxEvents;
    public void Initialize()
    {
        sfxEvents = SFXEvents.ToDictionary(x => x.Key, x => x.Value);
    }

    // TryGet гр╠Н?
    public EventReference GetSFXEventRefernce(SFXTags tag)
    {
        return sfxEvents[tag];
    }
}