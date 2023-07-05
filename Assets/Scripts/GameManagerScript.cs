using EventHandler;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    [Header("Grid Size & Player Num")]
    [Range(5, 9)]
    [SerializeField] public int GridSize; 
    [Range(2, 4)]
    [SerializeField] public int NumberOfPlayers;

    [Header("Essentials")]
    [SerializeField] private GridGenerator _gridGenerator;
    [SerializeField] private Colorizer _colorizer;

    [Header("Materials")]
    [SerializeField] private Material _1PlayerMaterial;
    [SerializeField] private Material _2PlayerMaterial;
    [SerializeField] private Material _3PlayerMaterial;
    [SerializeField] private Material _4PlayerMaterial;

    [Header("Materials")]
    [SerializeField] private Color _1PlayerColor;
    [SerializeField] private Color _2PlayerColor;
    [SerializeField] private Color _3PlayerColor;
    [SerializeField] private Color _4PlayerColor;

    [Header("Buttons")]
    [SerializeField] private Button _initializeButton;

    private CellInstance.Cell[] _cells;

    private int _cellIndex;
    private void Start()
    {
        _initializeButton.onClick.AddListener(InitializeComponents);
    }

    [ContextMenu("Intialize Components")]
    private void InitializeComponents()
    {
        _cells = new CellInstance.Cell[GridSize * GridSize];
        EventAggregator.Subscribe<CellAdded>(AddCellToArray);
        EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);

        _gridGenerator.GenerateGrid();

        AddCells();
    }

    private void AddCellToArray(object arg1, CellAdded _cell)
    {
        _cells[_cellIndex++] = _cell.CellInstance;
    }

    private void AddCells()
    {
        foreach (var cell in _cells)
        {
            AddNeighbours(cell);
        }

        if (NumberOfPlayers >= 2)
        {
            AddDotsToCells(1, 1, _1PlayerColor, _1PlayerMaterial, Team.Team1);
            AddDotsToCells(GridSize - 2, GridSize - 2, _2PlayerColor, _2PlayerMaterial, Team.Team2);
            if (NumberOfPlayers >= 3)
            {
                AddDotsToCells(1, GridSize - 2, _3PlayerColor, _3PlayerMaterial, Team.Team3);
                if (NumberOfPlayers >= 4)
                {
                    AddDotsToCells(GridSize - 2, 1, _4PlayerColor, _4PlayerMaterial, Team.Team4);
                }
            }
        }
    }

    private CellInstance.Cell GetCellAtPos(int posColumn, int posRow)
    {
        foreach (var cell in _cells)
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
        CellInstance.Cell cell = GetCellAtPos(posColumn, posRow);
        cell.SetTeam(teamColor, material, team);
        cell.AddDot();
        cell.AddDot();
        cell.AddDot();
    }

    private void AddToNearbyCells(object arg1, AddToNearbyCells cellData)
    {
        bool setNextTurn = true;
        List<CellInstance.Cell> cells = new List<CellInstance.Cell>
        {
            cellData.neighbours.Item1,
            cellData.neighbours.Item2,
            cellData.neighbours.Item3,
            cellData.neighbours.Item4
        };
        foreach (var cell in cells)
        {
            if(cell != null)
            {
                if(cell?.NumberOfDots == 3)
                {
                    setNextTurn = false;
                }
                if (cell?.NumberOfDots == 0)
                {
                    cell.SetTeam(cellData.teamColor, cellData.material, cellData.team);
                    cell.AddDot();
                }
                else if (cell?.CellTeam == cellData.team)
                {
                    cell.AddDot();
                }
                else
                {
                    cell.SetTeam(cellData.teamColor, cellData.material, cellData.team);
                    cell.AddDot();
                }
            }
        }

        var aliveTeams = new HashSet<Team>();

        foreach (var cell in _cells)
        {
            cell.UpdateImage();
            aliveTeams.Add(cell.CellTeam);
        }

        for (int i = 1; i <= 4; i++)
        {
            var playerName = "PLAYER" + i;
            if (!aliveTeams.Contains((Team)i))
            {
                EventAggregator.Post(this, new PlayerLost { PlayerName = playerName });
            }
        }

        if (setNextTurn)
        {
            EventAggregator.Post(this, new NextTurn { cellTeam = cellData.team });
        }
    }

    private void AddNeighbours(CellInstance.Cell cell)
    {
        (int posX, int posY) topPos = (cell.PosColumn, cell.PosRow - 1);
        (int posX, int posY) rightPos = (cell.PosColumn + 1, cell.PosRow);
        (int posX, int posY) bottomPos = (cell.PosColumn, cell.PosRow + 1);
        (int posX, int posY) leftPos = (cell.PosColumn - 1, cell.PosRow);
        foreach (var neighbourCell in _cells)
        {
            if (neighbourCell.PosColumn == topPos.posX && neighbourCell.PosRow == topPos.posY)
            {
                cell.Neighbours = (neighbourCell, cell.Neighbours.right, cell.Neighbours.bottom, cell.Neighbours.left);
            }
            if (neighbourCell.PosColumn == rightPos.posX && neighbourCell.PosRow == rightPos.posY)
            {
                cell.Neighbours = (cell.Neighbours.top, neighbourCell, cell.Neighbours.bottom, cell.Neighbours.left);
            }
            if (neighbourCell.PosColumn == bottomPos.posX && neighbourCell.PosRow == bottomPos.posY)
            {
                cell.Neighbours = (cell.Neighbours.top, cell.Neighbours.right, neighbourCell, cell.Neighbours.left);
            }
            if (neighbourCell.PosColumn == leftPos.posX && neighbourCell.PosRow == leftPos.posY)
            {
                cell.Neighbours = (cell.Neighbours.top, cell.Neighbours.right, cell.Neighbours.bottom, neighbourCell);
            }
        }
    }
}
