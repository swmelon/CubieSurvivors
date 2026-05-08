using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSourcePrefab;
    
    private static SoundManager instance;
    private Queue<AudioSource> availableAudioSources = new Queue<AudioSource>();
    private Queue<AudioSource> availableSpatialAudioSources = new Queue<AudioSource>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public static SoundManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("SoundManager is not initialized.");
            }
            return instance;
        }
    }

    private void Start()
    {
        // Initialize the available audio sources pool
        for (int i = 0; i < 100; i++) // Adjust the initial pool size as needed
        {
            CreateNewAudioSource();
        }
        
        // Initialize the available spatial audio sources pool
        for (int i = 0; i < 100; i++) // Adjust the initial pool size as needed
        {
            CreateNewSpatialAudioSource();
        }
    }

    private void CreateNewAudioSource()
    {
        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        newAudioSource.playOnAwake = false;
        gameObject.SetActive(false);
        gameObject.SetActive(true);
        availableAudioSources.Enqueue(newAudioSource);
    }
    
    private void CreateNewSpatialAudioSource()
    {
        AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
        newAudioSource.spatialBlend = 1f;
        newAudioSource.playOnAwake = false;
        availableSpatialAudioSources.Enqueue(newAudioSource);
    }

    public void PlaySound(AudioClip audioClip, float volume = 1.0f)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("Attempting to play a null audio clip.");
            return;
        }

        AudioSource audioSource = GetAvailableAudioSource();
        
        if (audioSource != null)
        {
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
            StartCoroutine(ReturnAudioSourceWhenDone(audioSource));
        }
    }
    
    IEnumerator ReturnAudioSourceWhenDone(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        availableAudioSources.Enqueue(source);
    }
    
    public void PlaySpatialSound(AudioClip audioClip, Vector3 position, float volume = 1.0f)
    {
        if (audioClip == null)
        {
            Debug.LogWarning("Attempting to play a null audio clip.");
            return;
        }

        AudioSource audioSource = GetAvailableSpatialAudioSource();
        
        if (audioSource != null)
        {
            audioSource.transform.position = position;
            audioSource.clip = audioClip;
            audioSource.volume = volume;
            audioSource.Play();
            StartCoroutine(ReturnSpatialAudioSourceWhenDone(audioSource));
        }
    }

    IEnumerator ReturnSpatialAudioSourceWhenDone(AudioSource source)
    {
        yield return new WaitWhile(() => source.isPlaying);
        availableSpatialAudioSources.Enqueue(source);
    }
    
    private AudioSource GetAvailableAudioSource()
    {
        if (availableAudioSources.Count > 0)
        {
            return availableAudioSources.Dequeue();
        }

        // If no available audio source is found, create a new one
        CreateNewAudioSource();
        return availableAudioSources.Dequeue();
    }

    private AudioSource GetAvailableSpatialAudioSource()
    {
        if (availableSpatialAudioSources.Count > 0)
        {
            return availableSpatialAudioSources.Dequeue();
        }

        // If no available audio source is found, create a new one
        CreateNewSpatialAudioSource();
        return availableSpatialAudioSources.Dequeue();
    }
}