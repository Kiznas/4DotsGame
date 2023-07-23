using EventHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    [Header("Grid Size & Player Num")]
    [Range(5, 9)] public int gridSize;
    [Range(2, 4)] public int numberOfPlayers;

    [Header("Essentials")]
    [SerializeField] private GridGenerator _gridGenerator;
    [SerializeField] private Colorizer _colorizer;

    [Header("Materials")]
    public Material[] PlayerMaterials = new Material[4];

    [Header("Colors")]
    public Color[] PlayerColors = new Color[4];

    [Header("Buttons/Sliders")]
    [SerializeField] private Button _initializeButton;
    [SerializeField] private Button _restart;
    [SerializeField] private Slider _playersNumberSlider;
    [SerializeField] private Slider _gridSizeSlider;

    [SerializeField] private Toggle _performanceMod;

    [Header("Background")]
    [SerializeField] private GameObject _background;

    public Cell[] Cells;
    private Queue<Cell> cellQueue;
    private HashSet<Team> previouslyAliveTeams = new();

    private int _cellIndex;

    private bool IsProceeding;

    private void Start()
    {
        _initializeButton.onClick.AddListener(InitializeComponents);
        _playersNumberSlider.onValueChanged.AddListener(ChangePlayerNumber);
        _gridSizeSlider.onValueChanged.AddListener(ChangeGridSize);
        _restart.onClick.AddListener(RestartGame);
        Application.targetFrameRate = 60;
    }

    private void OnDestroy()
    {
        _initializeButton.onClick.RemoveAllListeners();
        _playersNumberSlider.onValueChanged.RemoveAllListeners();
        _gridSizeSlider.onValueChanged.RemoveAllListeners();
        _restart.onClick.RemoveAllListeners();
        EventAggregator.Unsubscribe<CellAdded>(AddCellToArray);
        EventAggregator.Unsubscribe<AddToNearbyCells>(AddToNearbyCells);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1.0f;
    }

    private void ChangeGridSize(float arg0)
    {
        gridSize = (int)arg0;
    }

    private void ChangePlayerNumber(float arg0)
    {
        numberOfPlayers = (int)arg0;
    }

    private void InitializeComponents()
    {
        Cells = new Cell[gridSize * gridSize];
        EventAggregator.Subscribe<CellAdded>(AddCellToArray);
        EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);
        EventAggregator.Post(this, new Initialization { });

        cellQueue = new Queue<Cell>();

        _gridGenerator.GenerateGrid();
        AddCells();
        _background.SetActive(true);
        _gridSizeSlider.gameObject.SetActive(false);
        _initializeButton.gameObject.SetActive(false);
        _playersNumberSlider.gameObject.SetActive(false);
    }

    private void AddCellToArray(object arg1, CellAdded _cell)
    {
        Cells[_cellIndex++] = _cell.CellInstance;
    }

    private void AddCells()
    {
        foreach (var cell in Cells)
        {
            AddNeighbours(cell);
        }

        for (int i = 0; i < numberOfPlayers; i++)
        {
            int posX, posY;
            switch (i)
            {
                case 0:
                    posX = 1;
                    posY = 1;
                    break;
                case 1:
                    posX = gridSize - 2;
                    posY = gridSize - 2;
                    break;
                case 2:
                    posX = 1;
                    posY = gridSize - 2;
                    break;
                case 3:
                    posX = gridSize - 2;
                    posY = 1;
                    break;
                default:
                    posX = posY = 1;
                    break;
            }
            AddDotsToCells(posX, posY, PlayerColors[i], PlayerMaterials[i], (Team)i + 1);
        }
    }

    private Cell GetCellAtPos(int posColumn, int posRow)
    {
        foreach (var cell in Cells)
        {
            if (cell.PosColumn == posColumn && cell.PosRow == posRow)
            {
                return cell;
            }
        }
        return null;
    }

    private void AddDotsToCells(int posColumn, int posRow, Color teamColor, Material material, Team team)
    {
        Cell cell = GetCellAtPos(posColumn, posRow);
        cell.SetTeam(teamColor, material, team);
        cell.AddDot();
        cell.AddDot();
        cell.AddDot();
    }

    private async void AddToNearbyCells(object arg1, AddToNearbyCells cellData)
    {
        if (cellData != null)
        {
            Cell cell = cellData.cell;
            cellQueue.Enqueue(cell);
        }

        if(IsProceeding == false)
        {
            IsProceeding = true;
            await ProcessQueue(cellData.cell.CellTeam);
        }
    }

    private async Task ProcessQueue(Team team)
    {
        while (cellQueue.Count > 0)
        {
            List<Cell> cellsWaves = new();
            while (cellQueue.Count > 0)
            {
                var cellData = cellQueue.Dequeue();
                cellsWaves.Add(cellData);
            }
            foreach (var cellData in cellsWaves)
            {
                ProcessCell(cellData);
            }
            if (!_performanceMod.isOn)
            {
                foreach (var cell in Cells)
                {
                    cell.UpdateImage();
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(Constants.SpeedOfGame));
        }
            
        var aliveTeams = new HashSet<Team>(Cells.Select(cell => cell.CellTeam));
        foreach (var lostTeam in previouslyAliveTeams.Except(aliveTeams))
        {
            var playerName = "PLAYER" + (int)lostTeam;
            EventAggregator.Post(this, new PlayerLost { PlayerName = playerName });
        }

        previouslyAliveTeams = aliveTeams;
        IsProceeding = false;

        if (_performanceMod.isOn)
        {
            foreach (var cell in Cells)
            {
                cell.UpdateImage();
            }
        }

        EventAggregator.Post(this, new NextTurn { cellTeam = team });
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
                if(item.NumberOfDots == 3)
                {
                    item.SetTeam(cell.TeamColor, cell.Material, cell.CellTeam);
                    item.NumberOfDots++;
                    item.UpdateImage();
                    cellQueue.Enqueue(item);
                    StartCoroutine(item.CellInstance.SpreadAnimation());
                }
                else
                {
                    item.SetTeam(cell.TeamColor, cell.Material, cell.CellTeam);
                    item.AddDot();
                }
            }
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
}

