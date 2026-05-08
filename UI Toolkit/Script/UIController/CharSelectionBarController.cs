using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class CharSelectionBarController : MonoBehaviour
{
    [FormerlySerializedAs("CharacterManager")] [SerializeField]
    private CharacterManagerSO characterManager;


    private void OnEnable()
    {
        CharSelectionBar.CharPortraitClicked += OnCharPortraitClicked;
    }
    
    private void OnDisable()
    {
        CharSelectionBar.CharPortraitClicked -= OnCharPortraitClicked;
    }
    
    private void OnCharPortraitClicked(int index)
    {
        Debug.Log("Char portrait clicked: " + index);
        
        if (characterManager.ChangeChar(index))
        {
            Debug.Log("Char changed to: " + index);
        }
        else
        {
            Debug.Log("Char change failed");
        }
    }
}
