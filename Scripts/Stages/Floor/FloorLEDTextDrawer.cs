using System.Collections.Generic;
using UnityEngine;

public class FloorLEDTextDrawer : MonoBehaviour
{
    [SerializeField]
    private GridTextManagerSO gridTextManager;

    [SerializeField]
    private StringEventChannelSO drawTextOnStageEC;

    private FloorLEDBuilder floorBuilder;

    private void Awake()
    {
        floorBuilder = GetComponent<FloorLEDBuilder>();
    }

    private void OnEnable()
    {
        drawTextOnStageEC.Subscribe(DrawTextOnFloor);
    }

    private void OnDisable()
    {
        drawTextOnStageEC.Unsubscribe(DrawTextOnFloor);
    }

    public void DrawTextOnFloor(string text)
    {
        bool isNumeric = int.TryParse(text, out int n);

        floorBuilder.ClearTextOnFloor();

        List<GridTextSO> digitsToDraw = new List<GridTextSO>();

        if (isNumeric)
        {

            // Get GridTextSO for each digit
            foreach (char digit in text)
            {
                if (gridTextManager.TryGetGridText(digit.ToString(), out GridTextSO gridText))
                {
                    digitsToDraw.Add(gridText);
                }
                else
                {
                    Debug.LogWarning($"GridTextSO for digit '{digit}' not found.");
                }
            }

            int width = digitsToDraw[0].Width;

            // Calculate starting position for the first digit
            Vector2Int startPosition = new Vector2Int(-(width + 1) * (digitsToDraw.Count - 1) / 2 - 2, 3);

            foreach (var digitGrid in digitsToDraw)
            {
                floorBuilder.DrawText(digitGrid, startPosition);

                startPosition.x += width + 1; // Move to the next digit position
            }
        }
        else
        {
            if (gridTextManager.TryGetGridText(text, out GridTextSO gridText))
            {
                floorBuilder.DrawText(gridText, new Vector2Int(-gridText.Width / 2, gridText.Height / 2));
            }
        }
    }

    
}