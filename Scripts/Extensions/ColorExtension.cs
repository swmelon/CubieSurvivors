using UnityEngine;

public static class ColorExtension
{
    public static Color GenerateRandomPastelColor()
    {
        // Start with a base (white) color
        float baseRed = 1f;
        float baseGreen = 1f;
        float baseBlue = 1f;

        // Generate a random color
        float randomRed = Random.Range(0f, 1f);
        float randomGreen = Random.Range(0f, 1f);
        float randomBlue = Random.Range(0f, 1f);

        // Mix the base color with the random color
        float finalRed = (baseRed + randomRed) / 2;
        float finalGreen = (baseGreen + randomGreen) / 2;
        float finalBlue = (baseBlue + randomBlue) / 2;

        // Return the new pastel color
        return new Color(finalRed, finalGreen, finalBlue);
    }

    public static Color GenerateRandomVividColor()
    {
        // Generate a random hue from 0 to 1
        float hue = Random.Range(0f, 1f);
        // Set saturation to 1 for full vividness
        float saturation = 1f;
        // Adjust brightness to ensure it's vivid but not too close to white
        // You might choose a value that ensures the color is bright but not overly so.
        float brightness = Random.Range(0.5f, 1f);

        // Convert HSB to RGB
        Color vividColor = Color.HSVToRGB(hue, saturation, brightness);

        return vividColor;
    }
}
