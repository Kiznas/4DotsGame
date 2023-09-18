using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Events;
using UI;
using UnityEngine;

namespace Game_Managing
{
    public class GameManage : MonoBehaviour
    {
        [Header("Essentials")]
        [SerializeField] private GridGenerator gridGenerator;
        [SerializeField] private GameManagerUI gameManagerUI;
        [SerializeField] private InputFields inputFields;
        [Header("Materials")]
        [SerializeField] private Material[] playersMaterials = new Material[4];
        [Header("Colors")]
        [SerializeField] private Color[] playersColors = new Color[4];

        public Cell.Cell[] Cells;
        private Queue<Cell.Cell> _cellQueue;
        private readonly Stack<Cell.Cell> _stackToChange = new();
        private HashSet<Enums.Team> _previouslyAliveTeams = new();

        private int _cellIndex;
        private bool _isProceeding;
        public int NumberOfPlayer { get; private set; }

        private void OnDestroy() {
            EventAggregator.Unsubscribe<CellAdded>(AddCellToArray);
            EventAggregator.Unsubscribe<AddToNearbyCells>(AddToNearbyCells);
        }

        public void InitializeComponents() {
            int rowsSize, columnsSize;
            NumberOfPlayer = (int)gameManagerUI.PlayersNumSlider.value;

            (rowsSize, columnsSize) = inputFields.GetCurrentMode();

            Cells = new Cell.Cell[rowsSize * columnsSize];
            _cellQueue = new Queue<Cell.Cell>();

            EventAggregator.Subscribe<CellAdded>(AddCellToArray);
            EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);
            EventAggregator.Post(this, new Initialization { TeamsColorList = playersColors });

            gridGenerator.GenerateGrid(rowsSize, columnsSize);
            AddCells(rowsSize, columnsSize, NumberOfPlayer);

            gameManagerUI.TurnOnOffGameObjects();
        }

        private void AddCellToArray(object arg1, CellAdded cell) {
            Cells[_cellIndex++] = cell.Cell;
        }

        private void AddCells(int rows, int columns, int numberOfPlayers) {
            foreach (var cell in Cells) {
                AddNeighbours(cell);
            }

            int[] playersPosX = { 1, rows - 2, 1, rows - 2 };
            int[] playersPosY = { 1, columns - 2, columns - 2, 1 };

            for (int i = 0; i < numberOfPlayers; i++) {
                int posX = playersPosX[i % 4];
                int posY = playersPosY[i % 4];

                AddDotsToCells(posX, posY, playersColors[i], playersMaterials[i], (Enums.Team)i + 1);
            }
        }

        private Cell.Cell GetCellAtPos(int posColumn, int posRow) {
            return Cells.FirstOrDefault(cell => cell.PosColumn == posColumn && cell.PosRow == posRow);
        }

        private void AddDotsToCells(int posColumn, int posRow, Color teamColor, Material material, Enums.Team team) {
            Cell.Cell cell = GetCellAtPos(posColumn, posRow);
            cell.SetTeam(teamColor, material, team);
            for (int i = 0; i < 3; i++) { cell.AddDot(); }
        }

        private async void AddToNearbyCells(object arg1, AddToNearbyCells cellData)
        {
            if (cellData != null) {
                Cell.Cell cell = cellData.Cell;
                _cellQueue.Enqueue(cell);
            }

            if (_isProceeding == false) {
                _isProceeding = true;
                if (cellData != null) await ProcessQueue(cellData.Cell.CellTeam);
            }
        }

        private async Task ProcessQueue(Enums.Team team) {
            while (_cellQueue.Count > 0) {
                List<Cell.Cell> cellsWaves = new();

                while (_cellQueue.Count > 0)
                {
                    var cellData = _cellQueue.Dequeue();
                    cellsWaves.Add(cellData);
                }
                foreach (var cellData in cellsWaves)
                {
                    ProcessCell(cellData);
                }

                await Task.Delay(TimeSpan.FromSeconds(Constants.SpeedOfGame));
            }

            var aliveTeams = new HashSet<Enums.Team>(Cells.Select(cell => cell.CellTeam));
            foreach (var lostTeam in _previouslyAliveTeams.Except(aliveTeams)) {
                var playerName = Constants.Player + (int)lostTeam;
                EventAggregator.Post(this, new PlayerLost { PlayerName = playerName });
            }

            _previouslyAliveTeams = aliveTeams;
            _isProceeding = false;

            await UpdateImages();

            await Task.WhenAll(UpdateImages());

            EventAggregator.Post(this, new NextTurn { CellTeam = team });
        }

        private async Task UpdateImages() {
            foreach (var item in _stackToChange.ToList())
            {
                await item.UpdateImageAsync();
            }
            _stackToChange.Clear();
            Resources.UnloadUnusedAssets();
            await Task.Delay(2);
        }

        private void ProcessCell(Cell.Cell cell) {
            List<Cell.Cell> cells = new()
            {
                cell.Neighbours.top,
                cell.Neighbours.right,
                cell.Neighbours.bottom,
                cell.Neighbours.left
            };

            foreach (var item in cells) {
                if (item != null && cell.TeamColor != Color.white) {
                    item.SetTeam(cell.TeamColor, cell.Material, cell.CellTeam);
                    item.AddDot();
                }
                StackAdd(item);
            }
            cell.ClearCell();
        }

        private void AddNeighbours(Cell.Cell cell) {
            int posX = cell.PosColumn;
            int posY = cell.PosRow;

            cell.Neighbours = (
                GetCellAtPos(posX, posY - 1),
                GetCellAtPos(posX + 1, posY),
                GetCellAtPos(posX, posY + 1),
                GetCellAtPos(posX - 1, posY)
            );
        }

        private void StackAdd(Cell.Cell cell)
        {
            if (cell != null)
            {
                List<Cell.Cell> cells = new(){
                    cell,
                    cell.Neighbours.top,
                    cell.Neighbours.right,
                    cell.Neighbours.bottom,
                    cell.Neighbours.left
                };

                foreach (var item in cells) {
                    if (item != null && !_stackToChange.Contains(item))
                    {
                        _stackToChange.Push(item);
                    }
                }
            }
        }
    }
}