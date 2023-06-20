using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageCombiner : MonoBehaviour
{
    public Texture2D[] _sourceImages;
    public Image _image;
    public void CombineImages(int numberOfDots, bool teamUp, bool teamRight, bool teamBottom, bool teamLeft)
    {
        List<Texture2D> images = new List<Texture2D>
        {
            _sourceImages[numberOfDots - 1]
        };

        if (!teamUp)images.Add(_sourceImages[(int)ImagesIndexes.TopLine]);
        if (!teamRight) images.Add(_sourceImages[(int)ImagesIndexes.RightLine]);
        if (!teamBottom) images.Add(_sourceImages[(int)ImagesIndexes.BottomLine]);
        if (!teamLeft) images.Add(_sourceImages[(int)ImagesIndexes.LeftLine]);

        Texture2D combinedImage = new Texture2D(_sourceImages[0].width, _sourceImages[0].height);

        for (int y = 0; y < combinedImage.height; y++)
        {
            for (int x = 0; x < combinedImage.width; x++)
            {
                Color pixelColor = Color.black; 

                for (int i = 0; i < images.Count; i++)
                {
                    Color sourceColor = images[i].GetPixel(x, y);

                    if (sourceColor.a > 0)
                    {
                        pixelColor = sourceColor;
                        break;
                    }
                }
                combinedImage.SetPixel(x, y, pixelColor);
            }
        }

        combinedImage.Apply();

        _image.sprite = Sprite.Create(combinedImage, new Rect(0, 0, combinedImage.width, combinedImage.height), Vector2.zero);
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
