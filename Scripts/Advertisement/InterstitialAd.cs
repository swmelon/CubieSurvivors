using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAd : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string _androidAdUnitId = "Interstitial_Android";
    [SerializeField] private string _iOsAdUnitId = "Interstitial_iOS";
    [SerializeField] private SaveLoadManagerSO saveLoadManager;
    [SerializeField] private PopupScreenController popupScreenController;
    private string _adUnitId;

    private float adCooldown = 300f; // 5 minutes expressed in seconds
    private float timeSinceLastAd = float.MaxValue; // Start with a max value to allow first ad immediately

    public event Action AdsShowCompleted;
    public event Action AdsShowFailed;

    private SaveFile saveFile;
    private DateTime lastAdShowTime;
    private DateTime lastAdDate;
    private const int maxAdsPerDay = 25;

    void Awake()
    {
        _adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOsAdUnitId
            : _androidAdUnitId;

        saveFile = saveLoadManager.SaveFile; // Assuming there's a method to load the save file

        // Parse LastAdShowTime from string to DateTime
        if (DateTime.TryParse(saveFile.LastAdShowTime, out lastAdShowTime))
        {
            lastAdShowTime = lastAdShowTime.ToUniversalTime();
            Debug.Log(name + " : " + "Last ad show time loaded successfully.");
        }
        else
        {
            Debug.Log(name + " : " + "Failed to load last ad show time, setting to current time.");
            lastAdShowTime = DateTime.UtcNow.AddHours(-24);
            saveFile.LastAdShowTime = lastAdShowTime.ToString("o"); 
            saveLoadManager.Save(); // Save the current time to the save file
        }   

        // Parse LastAdDate from string to DateTime
        if (DateTime.TryParse(saveFile.LastAdDate, out lastAdDate))
        {
            lastAdDate = lastAdDate.ToUniversalTime();
            Debug.Log(name + " : " + "Last ad date loaded successfully.");
        }
        else
        {
            Debug.Log(name + " : " + "Failed to load last ad date, setting to today's date.");
            lastAdDate = DateTime.UtcNow.Date;
            saveFile.LastAdDate = lastAdDate.ToString("o");
            saveLoadManager.Save(); // Save the current date to the save file
        }

        // Check and reset ads shown today if it's a new day
        if (lastAdDate.Date != DateTime.UtcNow.Date)
        {
            saveFile.AdsShownToday = 0; // Reset daily ads count
            lastAdDate = DateTime.UtcNow.Date;
            saveFile.LastAdDate = lastAdDate.ToString("o"); // Save new date
            saveLoadManager.Save(); // Save the reset to the save file
        }

        // Calculate time since last ad
        timeSinceLastAd = (float)(DateTime.UtcNow - lastAdShowTime).TotalSeconds;
    }

    void Update()
    {
        if (timeSinceLastAd < adCooldown)
        {
            timeSinceLastAd += Time.unscaledDeltaTime; // Increment the cooldown timer
        }
    }

    public bool Loadable()
    {
        bool result = timeSinceLastAd >= adCooldown && saveFile.AdsShownToday < maxAdsPerDay;

        if (!result)
        {
            Debug.Log(name + " : " + "Ad cooldown in effect or daily limit reached. Cannot load ad yet.");
        }

        return timeSinceLastAd >= adCooldown && saveFile.AdsShownToday < maxAdsPerDay;
    }

    public bool TryLoadAndShow(Action onFinishCallback, Action onFailCallback)
    {
        if (Loadable())
        {
            Debug.Log(name + " : " + "Attempting to load Ad: " + _adUnitId);

            AdsShowCompleted += onFinishCallback;
            AdsShowFailed += onFailCallback;

            Advertisement.Load(_adUnitId, this);
            return true;
        }
        else
        {
            Debug.Log(name + " : " + "Ad cooldown in effect or daily limit reached. Cannot load ad yet.");
            return false;
        }
    }

    private void ShowAd()
    {
        Debug.Log(name + " : " + "Showing Ad: " + _adUnitId);
        Advertisement.Show(_adUnitId, this);
        timeSinceLastAd = 0; // Reset the timer after showing an ad
        lastAdShowTime = DateTime.UtcNow; // Update last ad show time
        saveFile.LastAdShowTime = lastAdShowTime.ToString("o"); // Save the current time as an ISO 8601 string
        saveFile.AdsShownToday++; // Increment the count of ads shown today
        if (lastAdDate.Date != DateTime.UtcNow.Date)
        {
            lastAdDate = DateTime.UtcNow.Date;
            saveFile.LastAdDate = lastAdDate.ToString("o"); // Update the date if it has changed
            saveFile.AdsShownToday = 1; // Reset daily ads count for the new day
        }
        saveLoadManager.Save(); // Save the changes to the save file
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        // Optionally execute code if the Ad Unit successfully loads content.
        ShowAd();
    }

    public void OnUnityAdsFailedToLoad(string _adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log(name + " : " + $"Error loading Ad Unit: {_adUnitId} - {error.ToString()} - {message}");
        popupScreenController.ShowPopupScreen(UIText.FAILED_TO_LOAD_ADS, UIText.OK, InvokeAddFailed);
    }

    public void OnUnityAdsShowFailure(string _adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log(name + " : " + $"Error showing Ad Unit {_adUnitId}: {error.ToString()} - {message}");
        popupScreenController.ShowPopupScreen(UIText.FAILED_TO_SHOW_ADS, UIText.OK, InvokeAddFailed);
    }

    public void OnUnityAdsShowStart(string _adUnitId) { }
    public void OnUnityAdsShowClick(string _adUnitId) { }
    public void OnUnityAdsShowComplete(string _adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        AdsShowCompleted?.Invoke();
        AdsShowCompleted = null;
    }

    private void InvokeAddFailed()
    {
        AdsShowFailed?.Invoke();
        AdsShowFailed = null;
    }
}
