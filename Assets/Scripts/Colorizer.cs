using UnityEngine;
using UnityEngine.UI;

public class Colorizer : MonoBehaviour
{
    [Range(0, 10)]
    [SerializeField] private float _colorBrightness;
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
