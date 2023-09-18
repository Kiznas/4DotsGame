using Game_Managing;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class GameManagerUI : MonoBehaviour
    {
        [Header("Buttons/Sliders/InputFields")]
        [SerializeField] private Button initializeButton;
        [SerializeField] private Button restart;
        [SerializeField] private Slider playersNumberSlider;
        [Header("Background")]
        [SerializeField] private GameObject background;
        [SerializeField] private GameObject gridSetting;
        [Header("Game Manager")]
        [SerializeField] private GameManage gameManager;
        [SerializeField] private InputFields inputFields;

        public Slider PlayersNumSlider => playersNumberSlider;

        private void Start() {
            initializeButton.onClick.AddListener(gameManager.InitializeComponents);
            restart.onClick.AddListener(RestartGame);
            Application.targetFrameRate = 144;
        }

        private void OnDestroy() {
            initializeButton.onClick.RemoveAllListeners();
            restart.onClick.RemoveAllListeners();

        }
        private static void RestartGame() {
            SceneManager.LoadScene(0);
        }

        public void TurnOnOffGameObjects() {
            background.SetActive(true);
            initializeButton.gameObject.SetActive(false);
            playersNumberSlider.gameObject.SetActive(false);
            inputFields.gameObject.SetActive(false);
            gridSetting.SetActive(false);
        }
    }
}
