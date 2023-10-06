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
        [SerializeField] private GridGenerator gridGenerator;
        [SerializeField] private InputFields inputFields;
        [SerializeField] private UIManager uiManager ;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Image backgroundImage;
        
        [Header("Colors")]
        [SerializeField] private Color[] playersColors = new Color[4];
        
        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Toggle[] playerBotToggles;
        [SerializeField] private TMP_Text winText;
        
        
        public static Bootstrapper Instance;

        public ImageCombine ImageCombiner;

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

            var cells = new Cell[rowsSize * columnsSize];

            _cellsManager = new CellManager(cells);
            
            gridGenerator.GenerateGrid(rowsSize, columnsSize);

            var playersMaterials = Resources.LoadAll<Material>(Constants.Materials);
            InitializeCells.AddCells(rowsSize, columnsSize, NumberOfPlayer, cells, playersColors, playersMaterials);
            
            _turnSystem = new InitializeTurnSystem(backgroundImage, winPanel, this, winText, playerBotToggles, restartButton, startButton, playersColors);
            _bot = new BotLogic(this, _cellsManager); 
            
            uiManager.TurnOffUnneededUI();
        }
    }
}