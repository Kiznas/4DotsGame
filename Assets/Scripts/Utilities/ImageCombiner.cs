using System.Collections.Generic;
using ConstantValues;
using UnityEngine;

namespace Utilities
{
    public class ImageCombine
    {
        private readonly Texture2D[] _sourceImages = Resources.LoadAll<Texture2D>(Constants.Images);
        private static readonly Sprite TransperentTexture = Resources.Load<Sprite>(Constants.Transperent);

        public Sprite CombineImages(int numberOfDots, CellLogic.Cell[] neighbours, Enums.Team team)
        {
            if (numberOfDots == 0)
                return null;

            var width = _sourceImages[0].width;
            var height = _sourceImages[0].height;
            Texture2D sourceImage = _sourceImages[numberOfDots - 1];
            List<Texture2D> images = new List<Texture2D> { sourceImage };

            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] != null && neighbours[i].CellTeam != team)
                {
                    images.Add(_sourceImages[i + 4]);
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

            return Sprite.Create(combinedImage, new Rect(0, 0, width, height), Vector2.zero);
        }

        public static Sprite ClearImage()
        {
            return TransperentTexture;
        }
    }
}
