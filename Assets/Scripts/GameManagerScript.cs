using System;
using System.Linq;
using UnityEngine;
using EventHandler;
using System.Threading.Tasks;
using System.Collections.Generic;

public class GameManagerScript : MonoBehaviour
{
    [Header("Grid Size & Player Num")]
    public int gridSize;
    public int numberOfPlayers;

    [Header("Essentials")]
    [SerializeField] private GridGenerator _gridGenerator;
    [SerializeField] private GameManagerUI _gameManagerUI;

    [Header("Materials")]
    [SerializeField] private Material[] _playersMaterials = new Material[4];

    [Header("Colors")]
    [SerializeField] private Color[] _playersColors = new Color[4];

    public Cell[] Cells;
    private Queue<Cell> _cellQueue;
    private Stack<Cell> _stackToChange = new();
    private HashSet<Team> _previouslyAliveTeams = new();

    private int _cellIndex;
    private bool _isProceeding;

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

        EventAggregator.Subscribe<CellAdded>(AddCellToArray);
        EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);
        EventAggregator.Post(this, new Initialization { teamsColorList = _playersColors });

        _cellQueue = new Queue<Cell>();

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

        int[] playerPosX = { 1, rows - 2, 1, rows - 2 };
        int[] playerPosY = { 1, columns - 2, columns - 2, 1 };

        for (int i = 0; i < numberOfPlayers; i++)
        {
            int posX = playerPosX[i % 4];
            int posY = playerPosY[i % 4];

            AddDotsToCells(posX, posY, _playersColors[i], _playersMaterials[i], (Team)i+1);
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
        cell.NumberOfDots = 2;
        cell.AddDot();
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
            
            await Task.Delay(TimeSpan.FromSeconds(Constants.SpeedOfGame));
        }

        var aliveTeams = new HashSet<Team>(Cells.Select(cell => cell.CellTeam));
        foreach (var lostTeam in _previouslyAliveTeams.Except(aliveTeams))
        {
            var playerName = "PLAYER" + (int)lostTeam;
            EventAggregator.Post(this, new PlayerLost { PlayerName = playerName });
        }

        _previouslyAliveTeams = aliveTeams;
        _isProceeding = false;

        await UpdateImages();

        EventAggregator.Post(this, new NextTurn { cellTeam = team });
    }

    async Task UpdateImages()
    {
        int cellsDone = 0;
        foreach (var item in _stackToChange)
        {
            await item.UpdateImageAsync();
            cellsDone++;
        }
        Debug.Log($"CellsDone{cellsDone}");
        _stackToChange.Clear();
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
                if (item.NumberOfDots == 3)
                {
                    item.SetTeam(cell.TeamColor, cell.Material, cell.CellTeam);
                    item.NumberOfDots++;
                    item.UpdateImage();
                    _cellQueue.Enqueue(item);
                    StartCoroutine(item.CellInstance.SpreadAnimation(true, item.TeamColor));
                }
                else
                {
                    item.SetTeam(cell.TeamColor, cell.Material, cell.CellTeam);
                    item.AddDot();
                }
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
        if(cell != null)
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