
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ToggleController : MonoBehaviour
{
    [SerializeField] 
    private bool changeImage;
    
    [SerializeField]
    private Sprite onHandleImage, offHandleImage;
    
    [SerializeField]
    private Sprite onBackgroundImage, offBackgroundImage;

    [SerializeField]
    private GameObject handle, background;
    
    [SerializeField]
    private AnimationCurve handleAnimationCurve;
    
    [SerializeField]
    private float handleAnimationSpeed = 1f;


    private Image handleImage, backgroundImage;
    public UnityEvent OnToggleOn, OnToggleOff;


    private RectTransform handleRectTransform;
    private bool isOn;
    private float movement;
    
    private void Awake()
    {
        if (changeImage)
        {
            handleImage = handle.GetComponent<Image>();
            backgroundImage = background.GetComponent<Image>();
        }
        
        handleRectTransform = handle.GetComponent<RectTransform>();
        movement = handleRectTransform.localPosition.x;
        
        if (movement > 0)
        {
            isOn = true;
        }
        else
        {
            isOn = false;
        }
        
        movement = Math.Abs(movement);
    }

    public void SetToggle()
    {
        isOn = !isOn;
        
        if (changeImage)
        {
            if (isOn)
            {
                handleImage.sprite = onHandleImage;
                backgroundImage.sprite = onBackgroundImage;
            }
            else
            {
                handleImage.sprite = offHandleImage;
                backgroundImage.sprite = offBackgroundImage;
            }
            
        }
        
        if (isOn)
        {
            OnToggleOn?.Invoke();
        }
        else
        {
            OnToggleOff?.Invoke();
        }
        
        StopAllCoroutines();
        StartCoroutine(MoveHandle(isOn));
    }    
    
    private IEnumerator MoveHandle(bool val)
    {
        float target = movement;
        if (!val)
        {
            target = -target;
        }
        
        Vector3 startPosition = handleRectTransform.localPosition;
        Vector3 targetPosition = new Vector3(target, startPosition.y, startPosition.z);
        float time = 0f;
        
        while (Math.Abs(handleRectTransform.localPosition.x - target) > 0.01f)
        {
            time += Time.unscaledDeltaTime * handleAnimationSpeed;
            handleRectTransform.localPosition = Vector3.Lerp(startPosition, targetPosition, handleAnimationCurve.Evaluate(time));
            yield return null;
        }

        handleRectTransform.localPosition = targetPosition;
    }
}
