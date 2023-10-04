using System.Collections.Generic;
using System.IO;
using Constants;
using UnityEngine;
using UnityEngine.UI;

public class ImageCombiner : MonoBehaviour
{
    public Texture2D[] sourceImages;
    public Image image;

    public ImageCombiner(Texture2D[] sourceImages, Image image)
    {
        this.sourceImages = sourceImages;
        this.image = image;
    }
    
    public void CombineImages(int numberOfDots, CellLogic.Cell[] neighbours, Enums.Team team)
    {
        if (numberOfDots == 0)
            return;

        var width = sourceImages[0].width;
        var height = sourceImages[0].height;
        Texture2D sourceImage = sourceImages[numberOfDots - 1];
        List<Texture2D> images = new List<Texture2D> { sourceImage };

        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours[i] != null && neighbours[i].CellTeam != team)
            {
                images.Add(sourceImages[i + 4]);
            }
        }

        var combinedPixels = new Color32[width * height];

        foreach (var texture2D in images)
        {
            var pixels = texture2D.GetPixels32();

            for (var i = 0; i < pixels.Length; i++)
            {
                if (pixels[i].a > 1)
                {
                    combinedPixels[i] = pixels[i];
                }
            }
        }

        Texture2D combinedImage = new Texture2D(width, height);
        combinedImage.SetPixels32(combinedPixels);
        combinedImage.Apply();

        Sprite sprite = Sprite.Create(combinedImage, new Rect(0, 0, width, height), Vector2.zero);
        image.sprite = sprite;
    }

    public void ClearImage(Texture2D sourceImage)
    {
        Texture2D clearedImage = new(sourceImage.width, sourceImage.height);

        for (int y = 0; y < clearedImage.height; y++)
        {
            for (int x = 0; x < clearedImage.width; x++)
            {
                Color pixelColor = new Color(0, 0 , 0, 0);
                clearedImage.SetPixel(x, y, pixelColor);
            }
        }

        clearedImage.Apply();

        image.sprite = Sprite.Create(clearedImage, new Rect(0, 0, clearedImage.width, clearedImage.height),
            Vector2.zero);
    }
}
