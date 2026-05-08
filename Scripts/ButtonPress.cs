using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Local.Scripts.Extensions;

public class ButtonPress : MonoBehaviour
{
    [SerializeField]
    private EventChannelSO returnButtonEC;

    public UnityEvent<Vector3> OnButtonPressed;

    [SerializeField]
    private float pushAmount = 0.1f;
    [SerializeField]
    private AudioClip buttonSound;
    [SerializeField]
    private float lerpSpeed = 0.3f;

    private const float pressDetectRange = 0.4f;
    private const float moveCompleteThreshold = 0.01f;

    private bool isPressed = false;
    private Vector3 initialPosition;
    private bool isMoving = false;  // Flag to check if the button is currently moving
    private bool returnButton = false;
    private void Awake()
    {
        initialPosition = transform.localPosition;
    }

    private void OnEnable()
    {
        transform.localPosition = initialPosition;
        isPressed = false;
        isMoving = false;
        returnButtonEC?.Subscribe(ReturnButton);
    }

    private void OnDisable()
    {
        returnButtonEC?.Unsubscribe(ReturnButton);
    }

    private void FixedUpdate()
    {
        if (isMoving) return;  // Skip checking for player if the button is moving

        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.up, out hit, pressDetectRange, LayerMaskCash.OnlyPlayer))
        {
            if (!isPressed)
            {
                isPressed = true;
                OnButtonPressed?.Invoke(transform.position + Vector3.up);
                StartCoroutine(SmoothMove(transform.localPosition - new Vector3(0, pushAmount, 0), true));
            }
        }
        else
        {
            if (returnButton)
            {
                returnButton = false;
                StartCoroutine(SmoothMove(initialPosition));
            }
        }
    }

    private IEnumerator SmoothMove(Vector3 endPos, bool returning = false)
    {
        isMoving = true;
        Vector3 startPos = transform.localPosition;

        float journeyLength = Vector3.Distance(startPos, endPos);
        float startTime = Time.time;

        while (Vector3.Distance(transform.localPosition, endPos) > moveCompleteThreshold)
        {
            float distanceCovered = (Time.time - startTime) * lerpSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            transform.localPosition = Vector3.Lerp(startPos, endPos, fractionOfJourney);
            yield return null;
        }

        transform.localPosition = endPos;

        if (!returning)
        {
            // Once the button is pressed, start returning to the initial position
            isPressed = false;
        }

        isMoving = false;
    }

    public void ReturnButton()
    {
        returnButton = true;
    }
}
