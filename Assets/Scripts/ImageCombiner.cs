using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ImageCombiner : MonoBehaviour
{
    public Texture2D[] _sourceImages;
    public Image _image;

    public async void CombineImages(int numberOfDots, bool teamUp, bool teamRight, bool teamBottom, bool teamLeft)
    {
        await CombineImagesAsync(numberOfDots, teamUp, teamRight, teamBottom, teamLeft);
    }

    private async Task CombineImagesAsync(int numberOfDots, bool teamUp, bool teamRight, bool teamBottom, bool teamLeft)
    {
        if (numberOfDots == 0) { return; }
        Texture2D sourceImage = _sourceImages[numberOfDots - 1];

        List<Texture2D> images = new List<Texture2D> { sourceImage };

        if (!teamUp) images.Add(_sourceImages[(int)ImagesIndexes.TopLine]);
        if (!teamRight) images.Add(_sourceImages[(int)ImagesIndexes.RightLine]);
        if (!teamBottom) images.Add(_sourceImages[(int)ImagesIndexes.BottomLine]);
        if (!teamLeft) images.Add(_sourceImages[(int)ImagesIndexes.LeftLine]);

        int combinedWidth = sourceImage.width;
        int combinedHeight = sourceImage.height;

        Color32[] combinedPixels = new Color32[combinedWidth * combinedHeight];

        Color32[][] pixelColors = new Color32[images.Count][];
        for (int i = 0; i < images.Count; i++)
        {
            pixelColors[i] = images[i].GetPixels32();
        }

        for (int y = 0; y < combinedHeight; y++)
        {
            for (int x = 0; x < combinedWidth; x++)
            {
                Color pixelColor = Color.black;
                for (int i = 0; i < images.Count; i++)
                {
                    Color sourceColor = pixelColors[i][x + y * combinedWidth];
                    if (sourceColor.a > 0)
                    {
                        pixelColor = sourceColor;
                        break;
                    }
                }
                combinedPixels[x + y * combinedWidth] = pixelColor;
            }
        }

        Texture2D combinedImage = new Texture2D(combinedWidth, combinedHeight);
        combinedImage.SetPixels32(combinedPixels);
        combinedImage.Apply();

        if (_image.sprite != null) 
        {
            Resources.UnloadUnusedAssets();
        }

        Sprite sprite = Sprite.Create(combinedImage, new Rect(0, 0, combinedWidth, combinedHeight), Vector2.zero);
        _image.sprite = sprite;
        await Task.Delay(1);
    }


    public void ClearImage(Texture2D sourceImage)
    {
        Texture2D clearedImage = new Texture2D(sourceImage.width, sourceImage.height);

        for (int y = 0; y < clearedImage.height; y++)
        {
            for (int x = 0; x < clearedImage.width; x++)
            {
                Color pixelColor = Color.black;
                clearedImage.SetPixel(x, y, pixelColor);
            }
        }

        clearedImage.Apply();

        _image.sprite = Sprite.Create(clearedImage, new Rect(0, 0, clearedImage.width, clearedImage.height), Vector2.zero);
    }

    private enum ImagesIndexes
    {
        OneDice = 0,
        TwoDice = 1,
        ThreeDice = 2,
        FourDice = 3,
        TopLine = 4,
        RightLine = 5,
        BottomLine = 6,
        LeftLine = 7
    }
}
