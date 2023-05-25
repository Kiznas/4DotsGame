using UnityEngine;
using UnityEngine.UI;

public class ImageCombiner : MonoBehaviour
{
    public Texture2D[] sourceImages; // Array of source images to combine
    public Image _image;

    [ContextMenu("Combine")]
    private void CombineImages()
    {
        Texture2D combinedImage = new Texture2D(sourceImages[0].width, sourceImages[0].height);

        // Loop through each pixel of the combined image
        for (int y = 0; y < combinedImage.height; y++)
        {
            for (int x = 0; x < combinedImage.width; x++)
            {
                Color pixelColor = Color.black; // Default color if no source image has pixel at (x, y)

                // Loop through each source image
                for (int i = 0; i < sourceImages.Length; i++)
                {
                    // Get the color of the pixel from the current source image
                    Color sourceColor = sourceImages[i].GetPixel(x, y);

                    // If the current source image has a non-transparent pixel at (x, y), use that color
                    if (sourceColor.a > 0)
                    {
                        pixelColor = sourceColor;
                        break; // Exit the loop once a non-transparent pixel is found
                    }
                }

                // Set the color of the pixel in the combined image
                combinedImage.SetPixel(x, y, pixelColor);
            }
        }

        combinedImage.Apply();

        _image.sprite = Sprite.Create(combinedImage, new Rect(0, 0, combinedImage.width, combinedImage.height), Vector2.zero);
    }
}
