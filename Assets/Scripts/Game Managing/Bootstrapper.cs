using UI;
using TMPro;
using Utilities;
using CellLogic;
using BotScripts;
using ConstantValues;
using UnityEngine;
using UnityEngine.UI;
using Infrastructure;
using Game_Managing.CellsManager;
using Game_Managing.Initialization;

namespace Game_Managing
{
    public class Bootstrapper : MonoBehaviour, ICoroutineRunner
    {
        [Header("Essentials")] 
        [SerializeField] private GameObject gridGameObject;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private InputFields inputFields;
        [SerializeField] private UIManager uiManager ;
        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private TMP_Text winText;
        [SerializeField] private Toggle[] playerBotToggles;
        
        public static Bootstrapper Instance;
        public ImageCombine ImageCombiner;

        private Color[] _playersColors;
        private InitializeTurnSystem _turnSystem;
        private BotLogic _bot;
        private CellManager _cellsManager;

        public int NumberOfPlayer { get; private set; }
        
        private void Awake()
        {
            // Ensure there's only one instance of GameManager
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }
        }

        private void OnDestroy()
        {
            _cellsManager?.Unsubscribe();
            _bot?.Unsubscribe();
            _turnSystem?.Unsubscribe();
        }

        public void InitializeComponents() {
            ImageCombiner = new ImageCombine();
            int rowsSize, columnsSize;
            NumberOfPlayer = (int)uiManager.PlayersNumSlider.value;

            (rowsSize, columnsSize) = inputFields.GetCurrentMode();

            RandomizePlayersColors();

            var cells = new Cell[rowsSize * columnsSize];

            _cellsManager = new CellManager(cells);

            GridGenerator gridGenerator = new GridGenerator(backgroundImage.gameObject, gridGameObject);
            gridGenerator.GenerateGrid(rowsSize, columnsSize);

            var playersMaterials = Resources.LoadAll<Material>(Constants.Materials);
            InitializeCells.AddCells(rowsSize, columnsSize, NumberOfPlayer, cells, _playersColors, playersMaterials);
            
            _turnSystem = new InitializeTurnSystem(backgroundImage, winPanel, this, winText, playerBotToggles, restartButton, startButton, _playersColors);
            _bot = new BotLogic(this, _cellsManager); 
            
            uiManager.TurnOffUnneededUI();
        }

        private void RandomizePlayersColors() // TODO move from Bootstrapper
        {
            _playersColors = new Color[4];
            float randomHue = Random.Range(0f, 1f);
            for (int i = 0; i < _playersColors.Length; i++)
            {
                _playersColors[i] = Color.HSVToRGB(randomHue, 1, 1);
                randomHue += 0.25f;
                if (randomHue >= 1f)
                    randomHue -= 1f;
            }
        }
    }
}