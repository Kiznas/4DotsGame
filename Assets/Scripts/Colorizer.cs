using UnityEngine;
using UnityEngine.UI;

public class Colorizer : MonoBehaviour
{
    [Range(0, 10)]
    [SerializeField] private float _colorBrightness;
    private void GenerateNewColor(Material material)
    {
        Color randomColor;
        int h = Random.Range(0, 361);
        float s = 1f;
        float v = 1f;
        randomColor = Color.HSVToRGB(h / 360f, s, v, true);

        Color color = randomColor;
        color = new Color(
            color.r * Mathf.Pow(2, _colorBrightness),
            color.g * Mathf.Pow(2, _colorBrightness),
            color.b * Mathf.Pow(2, _colorBrightness),
            color.a);
        material.color = color;
    }
    public Color AddGlowing(Color color)
    {
        color = new Color(
            color.r * Mathf.Pow(2, _colorBrightness),
            color.g * Mathf.Pow(2, _colorBrightness),
            color.b * Mathf.Pow(2, _colorBrightness),
            color.a);
        return color;
    }
}
