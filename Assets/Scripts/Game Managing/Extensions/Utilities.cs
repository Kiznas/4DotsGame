using UnityEngine;

namespace Game_Managing.Extensions
{
    public static class Utilities
    {
        public static Color[] RandomizePlayersColors(out Color[] playersColors)
        {
            playersColors = new Color[4];
            float randomHue = Random.Range(0f, 1f);
            for (int i = 0; i < playersColors.Length; i++)
            {
                playersColors[i] = Color.HSVToRGB(randomHue, 1, 1);
                randomHue += 0.25f;
                if (randomHue >= 1f)
                    randomHue -= 1f;
            }

            return playersColors;
        }
    }
}