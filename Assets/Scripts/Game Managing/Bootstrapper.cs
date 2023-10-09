using UI;
using TMPro;
using Assets;
using Services;
using Utilities;
using CellLogic;
using BotScripts;
using Events.EventsManager;
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
        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton; 
        [SerializeField] private TMP_Text winText;
        [SerializeField] private Toggle[] playerBotToggles;
        
        public static Bootstrapper Instance;
        public ImageCombine ImageCombiner;

        private BotLogic _bot;
        private UIManager _uiManager;
        private Color[] _playersColors;
        private InputFields _inputFields;
        private CellManager _cellsManager;
        private InitializeTurnSystem _turnSystem;

        private AllServices _services;

        public int NumberOfPlayer { get; private set; }
        
        private void Awake() {
            if (Instance == null) { Instance = this; }
            else { Destroy(gameObject); }

            _inputFields = gameObject.GetComponent<InputFields>();
            _uiManager = gameObject.GetComponent<UIManager>();
        }

        private void OnDestroy() {
            _services?.Single<IEventsManager>().UnregisterAll();
        }

        public void InitializeComponents() {
            ImageCombiner = new ImageCombine();

            _services = AllServices.Container;
            _services.RegisterSingle<IAssetProvider>(new AssetProvider());
            _services.RegisterSingle<IEventsManager>(new EventsManager());
            
            var playersMaterials = Resources.LoadAll<Material>(AssetsPath.Materials);
            int rowsSize, columnsSize;
            NumberOfPlayer = (int)_uiManager.PlayersNumSlider.value;
            (rowsSize, columnsSize) = _inputFields.GetGridSize();
            
            var cells = new Cell[rowsSize * columnsSize];
            _cellsManager = new CellManager(cells);
            _services.Single<IEventsManager>().RegisterObject(_cellsManager);

            GridGenerator gridGenerator = new GridGenerator(backgroundImage.gameObject, gridGameObject, _services.Single<IAssetProvider>());
            gridGenerator.GenerateGrid(rowsSize, columnsSize);
            
            InitializeCells.AddCells(rowsSize, columnsSize, NumberOfPlayer, cells, Extensions.Utilities.RandomizePlayersColors(out _playersColors), playersMaterials);
            
            _turnSystem = new InitializeTurnSystem(backgroundImage, winPanel, this, winText, playerBotToggles, restartButton, startButton, _playersColors);
            _bot = new BotLogic(this, _cellsManager); 
            
            _services.Single<IEventsManager>().RegisterObject(_bot);
            _services.Single<IEventsManager>().RegisterObject(_turnSystem.TurnSystem);
            
            _uiManager.TurnOffUnneededUI();
        }
    }
}