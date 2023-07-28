using EventHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    [Header("Essentials")]
    [SerializeField] private GridGenerator _gridGenerator;
    [SerializeField] private GameManagerUI _gameManagerUI;

    [Header("Materials")]
    [SerializeField] private Material[] _playersMaterials = new Material[4];

    [Header("Colors")]
    [SerializeField] private Color[] _playersColors = new Color[4];

    public Cell[] Cells;
    private Queue<Cell> _cellQueue;
    private readonly Stack<Cell> _stackToChange = new();
    private HashSet<Team> _previouslyAliveTeams = new();

    private int _cellIndex;
    private int numberOfPlayers;

    private bool _isProceeding;
    public int NumberOfPlayer { get { return numberOfPlayers; } }

    private void OnDestroy()
    {
        EventAggregator.Unsubscribe<CellAdded>(AddCellToArray);
        EventAggregator.Unsubscribe<AddToNearbyCells>(AddToNearbyCells);
    }

    public void InitializeComponents()
    {
        int rowsSize, columnsSize;
        numberOfPlayers = (int)_gameManagerUI.PlayersNumSlider.value;

        (rowsSize, columnsSize) = _gameManagerUI.GetCurrentMode();

        Cells = new Cell[rowsSize * columnsSize];
        _cellQueue = new Queue<Cell>();

        EventAggregator.Subscribe<CellAdded>(AddCellToArray);
        EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);
        EventAggregator.Post(this, new Initialization { teamsColorList = _playersColors });

        _gridGenerator.GenerateGrid(rowsSize, columnsSize);
        AddCells(rowsSize, columnsSize, numberOfPlayers);

        _gameManagerUI.TurnOnOffGameObjects();
    }

    private void AddCellToArray(object arg1, CellAdded _cell)
    {
        Cells[_cellIndex++] = _cell.Cell;
    }

    private void AddCells(int rows, int columns, int numberOfPlayers)
    {
        foreach (var cell in Cells)
        {
            AddNeighbours(cell);
        }

        int[] playersPosX = { 1, rows - 2, 1, rows - 2 };
        int[] playersPosY = { 1, columns - 2, columns - 2, 1 };

        for (int i = 0; i < numberOfPlayers; i++)
        {
            int posX = playersPosX[i % 4];
            int posY = playersPosY[i % 4];

            AddDotsToCells(posX, posY, _playersColors[i], _playersMaterials[i], (Team)i + 1);
        }
    }

    private Cell GetCellAtPos(int posColumn, int posRow)
    {
        return Cells.FirstOrDefault(cell => cell.PosColumn == posColumn && cell.PosRow == posRow);
    }

    private void AddDotsToCells(int posColumn, int posRow, Color teamColor, Material material, Team team)
    {
        Cell cell = GetCellAtPos(posColumn, posRow);
        cell.SetTeam(teamColor, material, team);
        for (int i = 0; i < 3; i++) { cell.AddDot(); }
    }

    private async void AddToNearbyCells(object arg1, AddToNearbyCells cellData)
    {
        if (cellData != null)
        {
            Cell cell = cellData.cell;
            _cellQueue.Enqueue(cell);
        }

        if (_isProceeding == false)
        {
            _isProceeding = true;
            await ProcessQueue(cellData.cell.CellTeam);
        }
    }

    private async Task ProcessQueue(Team team)
    {
        while (_cellQueue.Count > 0)
        {
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

            await Task.Delay(TimeSpan.FromSeconds(Constants.SPEEDOFGAME));
        }

        var aliveTeams = new HashSet<Team>(Cells.Select(cell => cell.CellTeam));
        foreach (var lostTeam in _previouslyAliveTeams.Except(aliveTeams))
        {
            var playerName = Constants.PLAYER + (int)lostTeam;
            EventAggregator.Post(this, new PlayerLost { PlayerName = playerName });
        }

        _previouslyAliveTeams = aliveTeams;
        _isProceeding = false;

        await UpdateImages();

        await Task.WhenAll(UpdateImages());

        EventAggregator.Post(this, new NextTurn { cellTeam = team });
    }

    async Task UpdateImages()
    {
        int cellsDone = 0;
        foreach (var item in _stackToChange.ToList())
        {
            await item.UpdateImageAsync();
            cellsDone++;
        }
        _stackToChange.Clear();
        Resources.UnloadUnusedAssets();
        await Task.Delay(2);
    }

    private void ProcessCell(Cell cell)
    {
        List<Cell> cells = new()
        {
            cell.Neighbours.top,
            cell.Neighbours.right,
            cell.Neighbours.bottom,
            cell.Neighbours.left
        };

        foreach (var item in cells)
        {
            if (item != null && cell.TeamColor != Color.white)
            {
                item.SetTeam(cell.TeamColor, cell.Material, cell.CellTeam);
                item.AddDot();
            }

            StackAdd(item);
        }
        cell.ClearCell();
    }

    private void AddNeighbours(Cell cell)
    {
        int posX = cell.PosColumn;
        int posY = cell.PosRow;

        cell.Neighbours = (
            GetCellAtPos(posX, posY - 1),
            GetCellAtPos(posX + 1, posY),
            GetCellAtPos(posX, posY + 1),
            GetCellAtPos(posX - 1, posY)
        );
    }

    private void StackAdd(Cell cell)
    {
        if (cell != null)
        {
            List<Cell> cells = new()
            {
                cell,
                cell.Neighbours.top,
                cell.Neighbours.right,
                cell.Neighbours.bottom,
                cell.Neighbours.left
            };

            foreach (var item in cells)
            {
                if (item != null && !_stackToChange.Contains(item))
                {
                    _stackToChange.Push(item);
                }
            }
        }
    }
}