using UnityEngine;
using UnityEngine.UI;

public class RandomColorizer : MonoBehaviour
{
    [SerializeField] private Image _fullColorImage;
    [SerializeField] private Image _darkerColorImage;
    [SerializeField] private Material _material;

    [ContextMenu("GenerateNewColor")]
    private void GenerateNewColor()
    {
        Color randomColor = GetRandomColor();
        Color darkerColor = randomColor * 0.8f;

        _fullColorImage.color = randomColor;
        _material.color = randomColor;
        _darkerColorImage.color = darkerColor;
    }

    private Color GetRandomColor()
    {
        int r;
        int g;
        int b;

        do
        {
            r = (int)Random.Range(0, 2);
            g = (int)Random.Range(0, 2);
            b = (int)Random.Range(0, 2);
        } while (r + g + b == 3 || r + g + b == 0);

        return new Color(r, g, b);
    }
}
