using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using System.Collections.Generic;

public class CloudSaveManager : MonoBehaviour
{
    private async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void SaveData()
    {
        var data = new Dictionary<string, object> { { "MySaveKey", "HelloWorld" } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }
}