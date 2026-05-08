using FMOD.Studio;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{
    [FMODUnity.BankRef]
    public List<string> Banks;

    [SerializeField]
    private string sceneToLoad = "MainMenuScene";
    
    private Slider loadingBar;  // Drag your Slider component here in the Inspector
    private TextMeshProUGUI loadingText;
    
    private void Awake()
    {
        loadingBar = GetComponent<Slider>();
        loadingText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        StartCoroutine(LoadGameAsync());
    }

    private IEnumerator LoadGameAsync()
    {
        // Start an asynchronous operation to load the scene
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneToLoad);

        // Don't lead the scene start until all Studio Banks have finished loading
        async.allowSceneActivation = false;

        // Iterate all the Studio Banks and start them loading in the background
        // including the audio sample data
        foreach (var bank in Banks)
        {
            FMODUnity.RuntimeManager.LoadBank(bank, true);
        }

        // Keep yielding the co-routine until all the Bank loading is done
        while (FMODUnity.RuntimeManager.AnySampleDataLoading())
        {
            loadingBar.value = async.progress / 0.9f;  // Dividing by 0.9f because progress can go up to 0.9
            loadingText.text = $"{(int)(loadingBar.value * 100)}%";
            yield return null;
        }

        // Allow the scene to be activated. This means that any OnActivated() or Start()
        // methods will be guaranteed that all FMOD Studio loading will be completed and
        // there will be no delay in starting events
        async.allowSceneActivation = true;

        // Keep yielding the co-routine until scene loading and activation is done.
        while (!async.isDone)
        {
            yield return null;
        }

    }
}