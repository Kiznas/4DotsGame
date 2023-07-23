using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    private float deltaTime = 0.0f;
    [SerializeField] TextMeshProUGUI _textOfFps;

    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        float fps = 1.0f / deltaTime;
        string text = string.Format("FPS : {0:0.00}", fps);

        _textOfFps.text = text;
    }

}
