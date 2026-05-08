using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FourButtonsPuzzle : MonoBehaviour
{
    [SerializeField]
    private ButtonPress[] buttons; // Assign this in the Unity Editor with your button instances

    [SerializeField]
    private UnityEvent onPuzzleCompleted; // Event triggered when the puzzle is solved

    private int[] correctOrder = { 0, 1, 2, 3 }; // Correct order of button presses
    private List<int> currentPressOrder = new List<int>();

    private void Awake()
    {
        // Subscribe to each button's OnButtonPressed event
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Local copy for the closure below
            buttons[i].OnButtonPressed.AddListener((position) => ButtonPressed(index));
        }
    }

    private void ButtonPressed(int buttonIndex)
    {
        currentPressOrder.Add(buttonIndex);
        CheckPuzzleCompletion();
    }

    private void CheckPuzzleCompletion()
    {
        if (currentPressOrder.Count == correctOrder.Length)
        {
            for (int i = 0; i < correctOrder.Length; i++)
            {
                if (currentPressOrder[i] != correctOrder[i])
                {
                    ResetPuzzle();
                    return;
                }
            }

            // If all checks out
            onPuzzleCompleted.Invoke();
        }
    }

    private void ResetPuzzle()
    {
        // Optionally add some feedback for the player here
        currentPressOrder.Clear();
        // Reset buttons if necessary

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].ReturnButton();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].OnButtonPressed.RemoveAllListeners();
        }
    }
}
