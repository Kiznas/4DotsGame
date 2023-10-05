using UI;
using TMPro;
using System;
using Utilities;
using CellLogic;
using Constants;
using BotScripts;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using Infrastructure;
using System.Threading.Tasks;
using System.Collections.Generic;
using Events;
using Game_Managing.Initialization;

namespace Game_Managing
{
    public class GameManage : MonoBehaviour, ICoroutineRunner
    {
        [Header("Essentials")] 
        [SerializeField] private GridGenerator gridGenerator;
        [SerializeField] private InputFields inputFields;
        [SerializeField] private UIManager uiManager ;
        [Header("Materials")]
        [SerializeField] private Material[] playersMaterials = new Material[4];
        [Header("Colors")]
        [SerializeField] private Color[] playersColors = new Color[4];
        
        [Header("UI Elements")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Toggle[] playerBotToggles;
        [SerializeField] private TMP_Text winText;
        [Header("Essentials")]
        [SerializeField] private GameObject winPanel;
        [SerializeField] private Image backgroundImage;
        
        public static GameManage Instance;

        public ImageCombine ImageCombiner;

        public Cell[] Cells;
        private Queue<Cell> _cellQueue;
        private readonly Stack<Cell> _stackToChange = new();
        private HashSet<Enums.Team> _previouslyAliveTeams = new();

        private int _cellIndex;
        private bool _isProceeding;
        public int NumberOfPlayer { get; private set; }
        
        private void Awake()
        {
            // Ensure there's only one instance of GameManager
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Keep this object alive between scenes if needed
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy() {
            EventAggregator.Unsubscribe<CellAdded>(AddCellToArray);
            EventAggregator.Unsubscribe<AddToNearbyCells>(AddToNearbyCells);
        }
        public void InitializeComponents() {
            ImageCombiner = new ImageCombine();
            int rowsSize, columnsSize;
            NumberOfPlayer = (int)uiManager.PlayersNumSlider.value;

            (rowsSize, columnsSize) = inputFields.GetCurrentMode();

            Cells = new Cell[rowsSize * columnsSize];
            _cellQueue = new Queue<Cell>();

            EventAggregator.Subscribe<CellAdded>(AddCellToArray);
            EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);
            
            gridGenerator.GenerateGrid(rowsSize, columnsSize);
            InitializeCells.AddCells(rowsSize, columnsSize, NumberOfPlayer, Cells, playersColors, playersMaterials);

            uiManager.TurnOnOffGameObjects();

            InitializeTurnSystem turnSystem = new InitializeTurnSystem(backgroundImage, winPanel, this, winText, playerBotToggles, restartButton, startButton, playersColors);
            BotLogic bot = new BotLogic(this, this); 
        }
        
        private void AddCellToArray(object arg1, CellAdded cell)
        {
            Cells[_cellIndex++] = cell.Cell;
        }
        private async void AddToNearbyCells(object arg1, AddToNearbyCells cellData)
        {
            if (cellData != null) {
                Cell cell = cellData.Cell;
                _cellQueue.Enqueue(cell);
            }

            if (_isProceeding == false) {
                _isProceeding = true;
                if (cellData != null) await ProcessQueue(cellData.Cell.CellTeam);
            }
        }

        private async Task ProcessQueue(Enums.Team team) {
            while (_cellQueue.Count > 0) {
                List<Cell> cellsWaves = new();

                while (_cellQueue.Count > 0)
                {
                    var cellData = _cellQueue.Dequeue();
                    cellsWaves.Add(cellData);
                }
                foreach (var cellData in cellsWaves)
                {
                    ProcessCell(cellData);
                }

                await Task.Delay(TimeSpan.FromSeconds(Constants.Constants.SpeedOfGame));
            }

            var aliveTeams = new HashSet<Enums.Team>(Cells.Select(cell => cell.CellTeam));
            foreach (var lostTeam in _previouslyAliveTeams.Except(aliveTeams)) {
                var playerName = Constants.Constants.Player + (int)lostTeam;
                EventAggregator.Post(this, new PlayerLost { PlayerName = playerName });
            }

            _previouslyAliveTeams = aliveTeams;
            _isProceeding = false;

            UpdateImages();

            EventAggregator.Post(this, new NextTurn { CellTeam = team });
        }

        private void UpdateImages()
        {
            foreach (var item in _stackToChange.ToList())
            {
                item?.UpdateImage();
            }
            _stackToChange.Clear();
            Resources.UnloadUnusedAssets();
        }

        private void ProcessCell(Cell cell)
        {
            Cell[] cells = cell.Neighbours;

            foreach (var item in cells) {
                if (item != null && cell.TeamColor != Color.white) 
                {
                    item.SetTeam(cell.TeamColor, cell.Material, cell.CellTeam);
                    item.AddDot();
                }
                StackAdd(item);
            }
            cell.ClearCell();
        }

        private void StackAdd(Cell cell)
        {
            if (cell != null){
                var cells = new List<Cell> { cell };
                cells.AddRange(cell.Neighbours.Where(neighbour => neighbour != null));

                foreach (var item in cells.Where(item =>
                             item != null && !_stackToChange.Contains(item))){
                    _stackToChange.Push(item);
                }
            }
        }

    }
}