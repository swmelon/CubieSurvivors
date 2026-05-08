using System.Collections.Generic;
using UnityEngine;

public class CardFXController : MonoBehaviour
{
    [SerializeField]
    private List<FXCameraController> fxCamerasAlwaysOn;

    [SerializeField]
    private List<FXCameraController> fxCameras;

    public void TurnOnFx()
    {
        foreach (var fx in fxCamerasAlwaysOn)
        {
            fx.TurnOnFx();
        }
    }

    public void TurnOffFx()
    {
        foreach (var fx in fxCameras)
        {
            fx.TurnOffFx();
        }
    }
}