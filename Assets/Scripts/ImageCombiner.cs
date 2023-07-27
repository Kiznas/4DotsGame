using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ImageCombiner : MonoBehaviour
{
    public Texture2D[] _sourceImages;
    public Image _image;

    private readonly int combinedWidth = 64;
    private readonly int combinedHeight = 64;

    public void CombineImages(int numberOfDots, bool teamUp, bool teamRight, bool teamBottom, bool teamLeft)
    {
        if (numberOfDots == 0) { return; }
        Texture2D sourceImage = _sourceImages[numberOfDots - 1];

        List<Texture2D> images = new() { sourceImage };

        if (!teamUp) images.Add(_sourceImages[(int)ImagesIndexes.TopLine]);
        if (!teamRight) images.Add(_sourceImages[(int)ImagesIndexes.RightLine]);
        if (!teamBottom) images.Add(_sourceImages[(int)ImagesIndexes.BottomLine]);
        if (!teamLeft) images.Add(_sourceImages[(int)ImagesIndexes.LeftLine]);

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

        Texture2D combinedImage = new(combinedWidth, combinedHeight);
        combinedImage.SetPixels32(combinedPixels);
        combinedImage.Apply();

        Sprite sprite = Sprite.Create(combinedImage, new Rect(0, 0, combinedWidth, combinedHeight), Vector2.zero);
        _image.sprite = sprite;
    }

    public async Task CombineImagesAsync(int numberOfDots, bool teamUp, bool teamRight, bool teamBottom, bool teamLeft)
    {
        if (numberOfDots == 0) { return; }
        Texture2D sourceImage = _sourceImages[numberOfDots - 1];

        List<Texture2D> images = new() { sourceImage };

        if (!teamUp) images.Add(_sourceImages[(int)ImagesIndexes.TopLine]);
        if (!teamRight) images.Add(_sourceImages[(int)ImagesIndexes.RightLine]);
        if (!teamBottom) images.Add(_sourceImages[(int)ImagesIndexes.BottomLine]);
        if (!teamLeft) images.Add(_sourceImages[(int)ImagesIndexes.LeftLine]);

        Color32[] combinedPixels = new Color32[combinedWidth * combinedHeight];

        Color32[][] pixelColors = new Color32[images.Count][];
        for (int i = 0; i < images.Count; i++)
        {
            pixelColors[i] = images[i].GetPixels32();
        }

        int maxThreads = System.Environment.ProcessorCount;
        int rowsPerThread = combinedHeight / maxThreads;

        List<Task> tasks = new();

        for (int i = 0; i < maxThreads; i++)
        {
            int startY = i * rowsPerThread;
            int endY = (i == maxThreads - 1) ? combinedHeight : (i + 1) * rowsPerThread;

            tasks.Add(Task.Run(() => CombineChunk(pixelColors, combinedWidth, combinedPixels, startY, endY)));
        }

        await Task.WhenAll(tasks);

        Texture2D combinedImage = new(combinedWidth, combinedHeight);
        combinedImage.SetPixels32(combinedPixels);
        combinedImage.Apply();

        Sprite sprite = Sprite.Create(combinedImage, new Rect(0, 0, combinedWidth, combinedHeight), Vector2.zero);
        _image.sprite = sprite;
    }

    private void CombineChunk(Color32[][] pixelColors, int combinedWidth, Color32[] combinedPixels, int startY, int endY)
    {
        for (int y = startY; y < endY; y++)
        {
            for (int x = 0; x < combinedWidth; x++)
            {
                Color pixelColor = Color.black;
                for (int i = 0; i < pixelColors.Length; i++)
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
    }
    public void ClearImage(Texture2D sourceImage)
    {
        Texture2D clearedImage = new(sourceImage.width, sourceImage.height);

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
