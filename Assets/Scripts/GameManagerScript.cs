using TMPro;
using System;
using System.Linq;
using UnityEngine;
using EventHandler;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class GameManagerScript : MonoBehaviour
{
    [Header("Grid Size & Player Num")]
    [Range(5, 14)] public int gridSize;
    [Range(2, 4)] public int numberOfPlayers;

    [Header("Essentials")]
    [SerializeField] private GridGenerator _gridGenerator;
    [SerializeField] private Colorizer _colorizer;

    [Header("Materials")]
    public Material[] PlayerMaterials = new Material[4];

    [Header("Colors")]
    public Color[] PlayerColors = new Color[4];

    [Header("Buttons/Sliders/InputFields")]
    [SerializeField] private Button _initializeButton;
    [SerializeField] private Button _restart;
    [SerializeField] private Button _customGridButton;

    [SerializeField] private Slider _playersNumberSlider;

    [SerializeField] private TMP_InputField _gridSizeInput;
    [SerializeField] private TMP_InputField _inputRow;
    [SerializeField] private TMP_InputField _inputCollumn;

    [Header("Background & InputFields")]
    [SerializeField] private GameObject _background;
    [SerializeField] private GameObject _inputFields;

    private const string REGULAR = "REGULAR";
    private const string CUSTOM = "CUSTOM";

    public Cell[] Cells;
    private Queue<Cell> cellQueue;
    private Stack<Cell> stackToChange = new();
    private HashSet<Team> previouslyAliveTeams = new();

    private int _cellIndex;
    private bool IsProceeding;

    private void Start()
    {
        _initializeButton.onClick.AddListener(InitializeComponents);
        _playersNumberSlider.onValueChanged.AddListener(ChangePlayerNumber);
        _restart.onClick.AddListener(RestartGame);
        _customGridButton.onClick.AddListener(CustomGridSettings);
        Application.targetFrameRate = 60;
    }

    private void OnDestroy()
    {
        _initializeButton.onClick.RemoveAllListeners();
        _playersNumberSlider.onValueChanged.RemoveAllListeners();
        _restart.onClick.RemoveAllListeners();
        EventAggregator.Unsubscribe<CellAdded>(AddCellToArray);
        EventAggregator.Unsubscribe<AddToNearbyCells>(AddToNearbyCells);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1.0f;
    }
    private void ChangePlayerNumber(float arg0)
    {
        numberOfPlayers = (int)arg0;
    }

    private void InitializeComponents()
    {
        int rowsSize, columnsSize;

        if (_customGridButton.CompareTag(REGULAR))
        {
            gridSize = int.Parse(_gridSizeInput.text);
            rowsSize = gridSize;
            columnsSize = gridSize;
        }
        else
        {
            rowsSize = int.Parse(_inputRow.text);
            columnsSize = int.Parse(_inputCollumn.text);
        }

        Cells = new Cell[rowsSize * columnsSize];

        EventAggregator.Subscribe<CellAdded>(AddCellToArray);
        EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);
        EventAggregator.Post(this, new Initialization { });

        cellQueue = new Queue<Cell>();

        _gridGenerator.GenerateGrid(rowsSize, columnsSize);
        AddCells(rowsSize, columnsSize, numberOfPlayers);

        TurnOnOffGameObjects();
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

        if (IsProceeding == false)
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

        StartCoroutine(UpdateImages());

        EventAggregator.Post(this, new NextTurn { cellTeam = team });
    }

    IEnumerator UpdateImages()
    {
        int cellsDone = 0;
        foreach (var item in stackToChange)
        {
            item.UpdateImage();
            cellsDone++;
        }
        Debug.Log($"CellsDone{cellsDone}");
        stackToChange.Clear();
        yield return null;
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
                    cellQueue.Enqueue(item);
                    StartCoroutine(item.CellInstance.SpreadAnimation());
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

    private void CustomGridSettings()
    {
        if (_customGridButton.CompareTag(REGULAR))
        {
            _gridSizeInput.gameObject.SetActive(false);
            _inputFields.SetActive(true);
            _customGridButton.tag = CUSTOM;
            _customGridButton.GetComponentInChildren<TMP_Text>().text = REGULAR;
        }
        else if (_customGridButton.CompareTag(CUSTOM))
        {
            _gridSizeInput.gameObject.SetActive(true);
            _inputFields.SetActive(false);
            _customGridButton.tag = REGULAR;
            _customGridButton.GetComponentInChildren<TMP_Text>().text = CUSTOM;
        }
    }

    private void TurnOnOffGameObjects()
    {
        _background.SetActive(true);
        _gridSizeInput.gameObject.SetActive(false);
        _initializeButton.gameObject.SetActive(false);
        _playersNumberSlider.gameObject.SetActive(false);
        _customGridButton.gameObject.SetActive(false);
        _inputFields.SetActive(false);
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
                if (item != null)
                {
                    stackToChange.Push(item);
                }
            }
        }
    }
}