using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ImageCombiner : MonoBehaviour
{
    public Texture2D[] sourceImages;
    public Image image;

    private const int CombinedWidth = 64;
    private const int CombinedHeight = 64;

    public void CombineImages(int numberOfDots, (Cell.Cell, Cell.Cell, Cell.Cell, Cell.Cell) neighbours, Enums.Team team)
    {
        if (numberOfDots == 0) { return; }
        Texture2D sourceImage = sourceImages[numberOfDots - 1];
        List<Texture2D> images = new() { sourceImage };

        if (neighbours.Item1 != null && neighbours.Item1.CellTeam != team)
            images.Add(sourceImages[(int)ImagesIndexes.TopLine]);
        if (neighbours.Item2 != null && neighbours.Item2.CellTeam != team)
            images.Add(sourceImages[(int)ImagesIndexes.RightLine]);
        if (neighbours.Item3 != null && neighbours.Item3.CellTeam != team)
            images.Add(sourceImages[(int)ImagesIndexes.BottomLine]);
        if (neighbours.Item4 != null && neighbours.Item4.CellTeam != team) 
            images.Add(sourceImages[(int)ImagesIndexes.LeftLine]);

        var combinedPixels = new Color32[CombinedWidth * CombinedHeight];

        var pixelColors = new Color32[images.Count][];
        for (int i = 0; i < images.Count; i++)
        {
            pixelColors[i] = images[i].GetPixels32();
        }

        for (int y = 0; y < CombinedHeight; y++)
        {
            for (int x = 0; x < CombinedWidth; x++)
            {
                Color pixelColor = Color.black;
                for (int i = 0; i < images.Count; i++)
                {
                    Color sourceColor = pixelColors[i][x + y * CombinedWidth];
                    if (sourceColor.a > 0)
                    {
                        pixelColor = sourceColor;
                        break;
                    }
                }
                combinedPixels[x + y * CombinedWidth] = pixelColor;
            }
        }

        Texture2D combinedImage = new(CombinedWidth, CombinedHeight);
        combinedImage.SetPixels32(combinedPixels);
        combinedImage.Apply();

        Sprite sprite = Sprite.Create(combinedImage, new Rect(0, 0, CombinedWidth, CombinedHeight), Vector2.zero);
        image.sprite = sprite;
    }

    public async Task CombineImagesAsync(int numberOfDots, (Cell.Cell, Cell.Cell, Cell.Cell, Cell.Cell) neighbours, Enums.Team team)
    {
        if (numberOfDots == 0) { return; }
        Texture2D sourceImage = sourceImages[numberOfDots - 1];

        List<Texture2D> images = new() { sourceImage };

        if (neighbours.Item1 != null && neighbours.Item1.CellTeam != team)
            images.Add(sourceImages[(int)ImagesIndexes.TopLine]);
        if (neighbours.Item2 != null && neighbours.Item2.CellTeam != team)
            images.Add(sourceImages[(int)ImagesIndexes.RightLine]);
        if (neighbours.Item3 != null && neighbours.Item3.CellTeam != team)
            images.Add(sourceImages[(int)ImagesIndexes.BottomLine]);
        if (neighbours.Item4 != null && neighbours.Item4.CellTeam != team)
            images.Add(sourceImages[(int)ImagesIndexes.LeftLine]);

        Color32[] combinedPixels = new Color32[CombinedWidth * CombinedHeight];

        Color32[][] pixelColors = new Color32[images.Count][];
        for (int i = 0; i < images.Count; i++)
        {
            pixelColors[i] = images[i].GetPixels32();
        }

        int maxThreads = System.Environment.ProcessorCount;
        int rowsPerThread = CombinedHeight / maxThreads;

        List<Task> tasks = new();

        for (int i = 0; i < maxThreads; i++)
        {
            int startY = i * rowsPerThread;
            int endY = (i == maxThreads - 1) ? CombinedHeight : (i + 1) * rowsPerThread;

            tasks.Add(Task.Run(() => CombineChunk(pixelColors, CombinedWidth, combinedPixels, startY, endY)));
        }

        await Task.WhenAll(tasks);

        Texture2D combinedImage = new(CombinedWidth, CombinedHeight);
        combinedImage.SetPixels32(combinedPixels);
        combinedImage.Apply();

        Sprite sprite = Sprite.Create(combinedImage, new Rect(0, 0, CombinedWidth, CombinedHeight), Vector2.zero);
        image.sprite = sprite;
    }

    private void CombineChunk(Color32[][] pixelColors, int combinedWidth, Color32[] combinedPixels, int startY, int endY)
    {
        for (var y = startY; y < endY; y++)
        {
            for (var x = 0; x < combinedWidth; x++)
            {
                var pixelColor = Color.black;
                foreach (var t in pixelColors)
                {
                    Color sourceColor = t[x + y * combinedWidth];
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

        image.sprite = Sprite.Create(clearedImage, new Rect(0, 0, clearedImage.width, clearedImage.height), Vector2.zero);
    }

    private enum ImagesIndexes
    {
        TopLine = 4,
        RightLine = 5,
        BottomLine = 6,
        LeftLine = 7
    }
}
