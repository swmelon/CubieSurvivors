using System;
using UnityEngine;

public class InputCanvasController : MonoBehaviour
{
    [SerializeField]
    private BooleanEventChannelSO inputCanvasControlChannel;
    
    [SerializeField]
    private BooleanEventChannelSO joyStickPositionControlChannel;

    [SerializeField]
    private Transform joystick, jumpButton;
    
    private Canvas canvas;

    private void Awake()
    { 
        canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        joyStickPositionControlChannel.Subscribe(SetJoystickPosition);
        inputCanvasControlChannel.Subscribe(SetCanvas);
    }

    private void OnDisable()
    {
        joyStickPositionControlChannel.Unsubscribe(SetJoystickPosition);
        inputCanvasControlChannel.Unsubscribe(SetCanvas);
    }

    private void Start()
    {
        canvas.enabled = false;
    }

    private void SetJoystickPosition(bool val)
    {
        if (val)
        {
            // joystick goes to right
            jumpButton.SetSiblingIndex(0);
        }
        else
        {
            // joystick goes to left
            joystick.transform.SetSiblingIndex(0);
        }
    }
    
    private void SetCanvas(bool val)
    {
        canvas.enabled = val;
    }
}
