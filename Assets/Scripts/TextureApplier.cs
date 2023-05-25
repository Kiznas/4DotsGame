using UnityEngine;
using UnityEngine.UI;

public class TextureApplier : MonoBehaviour
{
    [SerializeField] private Sprite _texture1;
    [SerializeField] private Sprite _texture2;
    [SerializeField] private Sprite _texture3;
    [SerializeField] private Sprite _texture4;
    [SerializeField] private Sprite _texture5;
    [SerializeField] private Image _image;

    [ContextMenu("Apply Texture 1")]
    private void ApplyTexture1()
    {
        if (_image != null && _texture1 != null)
        {
            _image.sprite = _texture1;
        }
    }

    [ContextMenu("Apply Texture 2")]
    private void ApplyTexture2()
    {
        if (_image != null && _texture2 != null)
        {
            _image.sprite = _texture2;
        }
    }

    [ContextMenu("Apply Texture 3")]
    private void ApplyTexture3()
    {
        if (_image != null && _texture3 != null)
        {
            _image.sprite = _texture3;
        }
    }

    [ContextMenu("Apply Texture 4")]
    private void ApplyTexture4()
    {
        if (_image != null && _texture4 != null)
        {
            _image.sprite = _texture4;
        }
    }

    private void ApplyTexture5()
    {
        if (_image != null && _texture5 != null)
        {
            _image.sprite = _texture5;
        }
    }
}
