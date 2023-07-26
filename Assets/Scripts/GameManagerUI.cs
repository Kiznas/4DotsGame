using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerUI : MonoBehaviour
{
    [Header("Buttons/Sliders/InputFields")]
    [SerializeField] private Button _initializeButton;
    [SerializeField] private Button _restart;
    [SerializeField] private Button _customGridButton;

    [SerializeField] private Slider _playersNumberSlider;

    [SerializeField] private TMP_InputField _gridSizeInput;
    [SerializeField] private TMP_InputField _rowInput;
    [SerializeField] private TMP_InputField _columnInput;
    [SerializeField] private GameObject _customInputFields;

    [Header("Background")]
    [SerializeField] private GameObject _background;
    [SerializeField] private GameObject _gridSetting;

    [Header("Game Manager")]
    [SerializeField] private GameManagerScript _gameManager;

    public Slider PlayersNumSlider { get { return _playersNumberSlider; } }

    private void Start()
    {
        _initializeButton.onClick.AddListener(_gameManager.InitializeComponents);
        _restart.onClick.AddListener(RestartGame);
        _customGridButton.onClick.AddListener(CustomGridSettings);

        _gridSizeInput.onEndEdit.AddListener(delegate { MyValidate(_gridSizeInput); });
        _rowInput.onEndEdit.AddListener(delegate { MyValidate(_rowInput); });
        _columnInput.onEndEdit.AddListener(delegate { MyValidate(_columnInput); });

        Application.targetFrameRate = 60;
    }

    private void OnDestroy()
    {
        _initializeButton.onClick.RemoveAllListeners();
        _restart.onClick.RemoveAllListeners();
        _customGridButton.onClick.RemoveAllListeners();

        _gridSizeInput.onEndEdit.RemoveAllListeners();
        _rowInput.onEndEdit.RemoveAllListeners();
        _columnInput.onEndEdit.RemoveAllListeners();

    }
    private void CustomGridSettings()
    {
        if (_customGridButton.CompareTag(Constants.REGULAR))
        {
            _gridSizeInput.gameObject.SetActive(false);
            _customInputFields.SetActive(true);
            _customGridButton.tag = Constants.CUSTOM;
            _customGridButton.GetComponentInChildren<TMP_Text>().text = Constants.REGULAR;
        }
        else if (_customGridButton.CompareTag(Constants.CUSTOM))
        {
            _gridSizeInput.gameObject.SetActive(true);
            _customInputFields.SetActive(false);
            _customGridButton.tag = Constants.REGULAR;
            _customGridButton.GetComponentInChildren<TMP_Text>().text = Constants.CUSTOM;
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(0);
    }

    public void TurnOnOffGameObjects()
    {
        _background.SetActive(true);
        _initializeButton.gameObject.SetActive(false);
        _playersNumberSlider.gameObject.SetActive(false);
        _customGridButton.gameObject.SetActive(false);
        _gridSetting.SetActive(false);
    }

    public (int, int) GetCurrentMode()
    {
        if (_customGridButton.CompareTag(Constants.REGULAR))
        {
            return (int.Parse(_gridSizeInput.text), int.Parse(_gridSizeInput.text));
        }
        else
        {
            return (int.Parse(_rowInput.text), int.Parse(_columnInput.text));
        }
    }

    private void MyValidate(TMP_InputField inputField)
    {
        string text = inputField.text;

        if (string.IsNullOrEmpty(text) || text == "-")
            return;

        int minValue = 4;
        int maxValue = 20;

        if (int.TryParse(text, out int number))
        {
            if (number >= minValue && number <= maxValue)
                return;
        }
        inputField.text = GetValidValue(text, minValue, maxValue);

    }

    private string GetValidValue(string text, int minValue, int maxValue)
    {
        if (int.TryParse(text, out int number))
        {
            if (number < minValue)
                return minValue.ToString();
            if (number > maxValue)
                return maxValue.ToString();
        }
        return minValue.ToString();
    }
}
