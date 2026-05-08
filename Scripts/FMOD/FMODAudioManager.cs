using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using Local.Scripts.Extensions;
using System;
using AOT;

public class FMODAudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float ambienceVolume = 1;
    [Range(0, 1)]
    public float SFXVolume = 1;

    [SerializeField]
    private float pitchIncrement = 0.1f;

    [SerializeField]
    private FMODEvents FMODEvents;

    [RequireInterface(typeof(IEasyListener))]
    public UnityEngine.Object[] myEventListeners;

    [RequireInterface(typeof(IEasyPitchListener))]
    public UnityEngine.Object[] myPitchListeners;

    [SerializeField]
    private EventChannelSO defaultStageTransitionEC;

    [SerializeField]
    private ThemeDataChannelSO themeDataChannel;

    private Bus musicBus;
    private Bus sfxBus;

    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    private EventInstance ambienceEventInstance;
    private EventInstance musicEventInstance;
    private EasyEvent musicEasyEvent;
    private EventReference[] playlist;
    private bool playingPlaylist;
    private bool musicPlaying;


    public static FMODAudioManager instance { get; private set; }

    private bool musicSet;
    private EventReference musicToPlayOnNextDefaultTransition;
    private ThemeData currentThemeData; 
    private FMOD.Studio.EVENT_CALLBACK musicCallback;

    public bool PlayingPlaylist => playingPlaylist;
    public EventReference[] Playlist => playlist;
    public float BeatLength => musicEasyEvent.BeatLength() / currentPitch;
    public EventInstance MusicEventInstance => musicEventInstance;

    private float minimunPlayerHitInterval = 0.1f;
    private float lastPlayerHitTime;
    private float currentPitch = 1f;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one Audio Manager in the scene.");
            Destroy(gameObject);
            return;
        }

        FMODEvents.Initialize();

        instance = this;

        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        musicBus = RuntimeManager.GetBus("bus:/Music");
        sfxBus = RuntimeManager.GetBus("bus:/SFX");

        themeDataChannel.Subscribe(SetThemeData);
        defaultStageTransitionEC.Subscribe(OnDefaultTransition);
    }

    private void OnDestroy()
    {
        CleanUp();
        instance = null;
        themeDataChannel.Unsubscribe(SetThemeData);
        defaultStageTransitionEC.Unsubscribe(OnDefaultTransition);
        StopMusic();

    }


    private void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambienceEventInstance = CreateInstance(ambienceEventReference);
        ambienceEventInstance.start();
    }

    private void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = CreateInstance(musicEventReference);
        musicEventInstance.start();
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue)
    {
        ambienceEventInstance.setParameterByName(parameterName, parameterValue);
    }

    public void PlayOneShot(SFXTags tag)
    {
        RuntimeManager.PlayOneShot(FMODEvents.GetSFXEventRefernce(tag));
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public void PlayOneShot(SFXTags tag, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(FMODEvents.GetSFXEventRefernce(tag), worldPos);
    }

    public void PlayOneShot(SFXTags tag, GameObject gameObject)
    {
        RuntimeManager.PlayOneShotAttached(FMODEvents.GetSFXEventRefernce(tag), gameObject);
    }

    public void StopSound(SFXTags tag)
    {
        EventReference soundEventReference = FMODEvents.GetSFXEventRefernce(tag);

        // Convert the EventReference to a string for comparison.
        // This assumes you have a way to uniquely identify EventInstances by their EventReference.
        string eventRefString = soundEventReference.Guid.ToString();

        // Find the EventInstance with the matching EventReference
        foreach (var eventInstance in eventInstances)
        {
            // Getting the path of the event instance for comparison
            eventInstance.getDescription(out var eventDescription);
            eventDescription.getPath(out var eventPath);

            if (eventPath.Contains(eventRefString))
            {
                // Stop the sound immediately
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

                // Optional: Release the instance if you're done with it
                eventInstance.release();

                // Remove the instance from the list to prevent memory leaks
                eventInstances.Remove(eventInstance);

                // Break out of the loop once the sound is found and stopped
                break;
            }
        }
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    public void SetMusicAndPlay(EventReference music)
    {
        StopMusic();
        PlayMusic(music);
    }

    public void SetMusicAndPlayOnNextStage(EventReference music)
    {
        StopMusic();

        musicToPlayOnNextDefaultTransition = music;
        musicSet = true;
    }


    /// <summary>
    /// Set playlist of music to play.
    /// </summary>
    /// <param name="newPlaylist"></param>
    public void SetMusicPlaylist(EventReference[] newPlaylist)
    {
        playlist = newPlaylist;
        PlayMusicInPlayList();
    }

    public void UIButtonClickedPositive()
    {
        PlayOneShot(SFXTags.UISelectPositive);
    }
    public void UIButtonClickedNegative()
    {
        PlayOneShot(SFXTags.UISelectNegative);
    }

    public void BuyItem()
    {
        PlayOneShot(SFXTags.Buy2);
    }

    public void UpgradeStat()
    {
        PlayOneShot(SFXTags.UpgradeStat);
    }

    public void StopMusic()
    {
        if (musicEventInstance.isValid())
        {
            // Attempt to stop the event
            FMOD.RESULT stopResult = musicEasyEvent.stop();
            if (stopResult != FMOD.RESULT.OK)
            {
                Debug.LogError($"Failed to stop the music event: {stopResult}");
            }

            // Attempt to release the event instance
            FMOD.RESULT releaseResult = musicEventInstance.release();
            if (releaseResult != FMOD.RESULT.OK)
            {
                Debug.LogError($"Failed to release the music event: {releaseResult}");
            }
        }
        else
        {
            Debug.LogWarning("Music event instance was not valid when attempting to stop.");
        }

        playingPlaylist = false;
        musicPlaying = false;

    }

    public void PlayMusicInPlayList()
    {
        PlayMusic(playlist.PickRandom());
        playingPlaylist = true;
    }

    private void PlayMusic(FMODUnity.EventReference music)
    {
        musicEasyEvent = new EasyEvent(music, myEventListeners);
        musicEventInstance = musicEasyEvent.EventInstance;
        currentPitch = 1f;

        // Verify the creation was successful
        if (musicEventInstance.isValid())
        {
            // Correct delegate setup for the callback

            // caching  안하면 crash 터짐 or static 아니면 터짐
            FMOD.RESULT startResult = musicEasyEvent.start();

            if (startResult != FMOD.RESULT.OK)
            {
                Debug.LogError($"Failed to start the new music event: {startResult}");
            }
        }
        else
        {
            Debug.LogError("Failed to create a valid music event instance.");
        }

        musicPlaying = true;
    }


    //이게 필요한 이유? 멈추고 다시 재생하려고
    //정 필요하면 플레이리스트곡에만 콜백 추가.

  


    private void CleanUp()
    {
        // stop and release any created instances
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
        // stop all of the event emitters, because if we don't they may hang around in other scenes
        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0, 1);
        musicVolume = volume;
        musicBus.setVolume(musicVolume);
    }

    public void SetSFXVolume(float volume)
    {
        volume = Mathf.Clamp(volume, 0, 1);
        SFXVolume = volume;
        sfxBus.setVolume(SFXVolume);
    }
    public void PlayerHit()
    {
        float time = Time.time;
        if (time - lastPlayerHitTime < minimunPlayerHitInterval)
        {
            return;
        }

        PlayOneShot(SFXTags.PlayerHit);
        lastPlayerHitTime = time;
    }

    private void OnDefaultTransition()
    {
        if (musicSet)
        {
            StopMusic();

            musicSet = false;
            PlayMusic(musicToPlayOnNextDefaultTransition);
        }
        else
        {
            PitchUp();
        }

        SendPitchUpdateMessage();
    }

    private void PitchUp()
    {
        musicEventInstance.getPitch(out currentPitch);
        currentPitch += pitchIncrement;
        musicEventInstance.setPitch(currentPitch);

    }

    private void SendPitchUpdateMessage()
    {
        for (int i = 0; i < myPitchListeners.Length; i++)
        {
            IEasyPitchListener listener = myPitchListeners[i] as IEasyPitchListener;
            listener.OnPitchChanged();
        }
    }

    private void SetThemeData(ThemeData newThemeData)
    {
        currentThemeData = newThemeData;

        if (newThemeData.nonCombatTheme)
        {
            // set playlist
            SetMusicPlaylist(newThemeData.playlist);
        }
        else
        {
            SetMusicAndPlayOnNextStage(newThemeData.bgm);
        }

    }
}