using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableLoadingManager : MonoBehaviour
{
    // Singleton instance
    public static AddressableLoadingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    // Load an asset asynchronously by its addressable key
    public void LoadAsset<T>(string key, System.Action<T> onSuccess, System.Action onFailure = null)
    {
        Addressables.LoadAssetAsync<T>(key).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                T asset = handle.Result;
                onSuccess?.Invoke(asset);
            }
            else
            {
                onFailure?.Invoke();
            }
        };
    }

    public void LoadAsset<T>(AddressableKey key, System.Action<T> onSuccess, System.Action onFailure = null)
    {
        LoadAsset<T>(key.key, onSuccess, onFailure);
    }

    // Example method to unload an asset
    public void ReleaseAsset<T>(T asset)
    {
        Addressables.Release(asset);
    }
    // You can extend this class further to handle more specific cases,
    // like loading multiple assets, handling asset bundles, etc.
}