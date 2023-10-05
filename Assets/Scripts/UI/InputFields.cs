using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class InputFields : MonoBehaviour
    {
        [SerializeField] public Button customGridButton;
        [SerializeField] private TMP_InputField gridSizeInput;
        [SerializeField] private TMP_InputField rowInput;
        [SerializeField] private TMP_InputField columnInput;
        [SerializeField] private GameObject customInputFields;
        [SerializeField] private int minSize;
        [SerializeField] private int maxSize;

        private void Start() {
            customGridButton.onClick.AddListener(CustomGridSettings);
            gridSizeInput.onEndEdit.AddListener(delegate { MyValidate(gridSizeInput); });
            rowInput.onEndEdit.AddListener(delegate { MyValidate(rowInput); });
            columnInput.onEndEdit.AddListener(delegate { MyValidate(columnInput); });
        }

        private void OnDestroy() {
            customGridButton.onClick.RemoveAllListeners();
            gridSizeInput.onEndEdit.RemoveAllListeners();
            rowInput.onEndEdit.RemoveAllListeners();
            columnInput.onEndEdit.RemoveAllListeners();
        }
    
        private void CustomGridSettings()
        {
            if (customGridButton.CompareTag(Constants.Constants.Regular)) {
                gridSizeInput.gameObject.SetActive(false);
                customInputFields.SetActive(true);
                customGridButton.tag = Constants.Constants.Custom;
                customGridButton.GetComponentInChildren<TMP_Text>().text = Constants.Constants.Regular;
            }
            else if (customGridButton.CompareTag(Constants.Constants.Custom)) {
                gridSizeInput.gameObject.SetActive(true);
                customInputFields.SetActive(false);
                customGridButton.tag = Constants.Constants.Regular;
                customGridButton.GetComponentInChildren<TMP_Text>().text = Constants.Constants.Custom;
            }
        }
    
        public (int, int) GetCurrentMode() => 
            customGridButton.CompareTag(Constants.Constants.Regular) ? 
                (int.Parse(gridSizeInput.text), int.Parse(gridSizeInput.text)) :
                (int.Parse(rowInput.text), int.Parse(columnInput.text));
    
        private void MyValidate(TMP_InputField inputField) {
            var text = inputField.text;

            if (string.IsNullOrEmpty(text) || text == "-")
                return;
            if (int.TryParse(text, out var number)) {
                if (number >= minSize && number <= maxSize)
                    return;
            }
            inputField.text = GetValidValue(text, minSize, maxSize);
        }

        private static string GetValidValue(string text, int minValue, int maxValue) {
            if (int.TryParse(text, out var number)) {
                if (number < minValue)
                    return minValue.ToString();
                if (number > maxValue)
                    return maxValue.ToString();
            }
            return minValue.ToString();
        }
    }
}