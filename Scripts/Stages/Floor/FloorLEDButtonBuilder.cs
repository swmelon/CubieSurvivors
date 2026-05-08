using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class FloorLEDButtonBuilder : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> LEDButtonPrefabs;

    private FloorLEDBuilder floorLEDBuilder;
    private List<GameObject> activeLEDButtons, inactiveButtons;


    private void Awake()
    {
        floorLEDBuilder = GetComponent<FloorLEDBuilder>();
        activeLEDButtons = new List<GameObject>();
        inactiveButtons = new List<GameObject>();
    }

    public void AddButtons()
    {
        // Add weapon spinner button LED at random position

        foreach (GameObject buttonPrefab in LEDButtonPrefabs)
        {
            if (floorLEDBuilder.TryGetOnFloorObjectPosition(out Vector3 position))
            {
                position.y = buttonPrefab.transform.position.y + floorLEDBuilder.HalfCubeHeight;
                GameObject weaponSpinnerButtonLED = Instantiate(buttonPrefab, transform);
                weaponSpinnerButtonLED.transform.localPosition = position;
                weaponSpinnerButtonLED.SetActive(true);
                inactiveButtons.Add(weaponSpinnerButtonLED);
            }
            else
            {
                Debug.LogWarning("Failed to get a position for the weapon spinner button LED.");
            }
        }
    }

    public void ActivateLEDButtons()
    {
        for (int i = 0; i < inactiveButtons.Count; i++)
        {
            GameObject button = inactiveButtons[i];
            button.SetActive(true);
            activeLEDButtons.Add(button);
        }

        inactiveButtons.Clear();
    }

    public void DestoryLEDButtons()
    {
        for (int i = 0; i < activeLEDButtons.Count; i++)
        {
            GameObject button = activeLEDButtons[i];
            Destroy(button);
        }

        activeLEDButtons.Clear();
    }

}
