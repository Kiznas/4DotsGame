using Game_Managing;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Buttons/Sliders/InputFields")]
        [SerializeField] private Button initializeButton;
        [SerializeField] private Button restart;
        [SerializeField] private Button tutorial;
        [SerializeField] private Slider playersNumberSlider;
        [Header("Background")]
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject gridSetting;
        [Header("Game Manager")]
        [SerializeField] private Bootstrapper gameManager;
        [SerializeField] private InputFields inputFields;

        public Slider PlayersNumSlider => playersNumberSlider;

        public void TurnOffUnneededUI() {
            background.SetActive(true);
            tutorial.gameObject.SetActive(false);
            initializeButton.gameObject.SetActive(false);
            playersNumberSlider.gameObject.SetActive(false);
            inputFields.customGridButton.gameObject.SetActive(false);
            gridSetting.SetActive(false);
        }

        private void Start() {
            initializeButton.onClick.AddListener(gameManager.InitializeComponents);
            restart.onClick.AddListener(RestartGame);
        }
        private void OnDestroy() {
            initializeButton.onClick.RemoveAllListeners();
            restart.onClick.RemoveAllListeners();

        }
        private static void RestartGame() {
            SceneManager.LoadScene(0);
        }
    }
}
