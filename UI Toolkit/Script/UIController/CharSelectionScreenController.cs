
using System;
using UnityEngine;

public class CharSelectionScreenController : MonoBehaviour
{
    [SerializeField]
    private CharSelectionScreen charSelectionScreen;

    [SerializeField]
    private StageSelectionScreenController stageSelectionScreenController;



    private void OnEnable()
    {
        CharSelectionScreen.CharSelectButtonClicked += OnCharSelectButtonClicked;
    }
    
    private void OnDisable()
    {
        CharSelectionScreen.CharSelectButtonClicked -= OnCharSelectButtonClicked;
    }
    

    private void OnCharSelectButtonClicked()
    {
        charSelectionScreen.HideScreen();
        stageSelectionScreenController.ShowStageSelectionScreen();
    }
  

    public void ShowCharSelectionScreen()
    {
        charSelectionScreen.ShowScreen();
    }
}
