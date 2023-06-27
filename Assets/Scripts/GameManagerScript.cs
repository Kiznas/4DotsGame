using EventHandler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour
{
    [Range(2, 4)]
    [SerializeField] private int _numberOfPlayers;
    [SerializeField] private CellInstance.Cell[] _cells;
    [SerializeField] private GridGenerator gridGenerator;
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
    [SerializeField] private Button InitializeButton;
    [SerializeField] private Button CellsButton;

    private int _cellIndex;
    private int _gridSize;

    private void Start()
    {
        InitializeButton.onClick.AddListener(InitializeComponents);
        CellsButton.onClick.AddListener(AddCells);
    }

    [ContextMenu("Intialize Components")]
    private void InitializeComponents()
    {
        _gridSize = gridGenerator._gridSize;
        _cells = new CellInstance.Cell[_gridSize * _gridSize];
        EventAggregator.Subscribe<CellAdded>(AddCellToArray);
        EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);

        _1PlayerColor = _colorizer.AddGlowing(_1PlayerColor);
        _2PlayerColor = _colorizer.AddGlowing(_2PlayerColor);
        _3PlayerColor = _colorizer.AddGlowing(_3PlayerColor);
        _4PlayerColor = _colorizer.AddGlowing(_4PlayerColor);
    }

    private void AddCellToArray(object arg1, CellAdded _cell)
    {
        _cells[_cellIndex++] = _cell.CellInstance;
    }

    [ContextMenu("AddCells")]
    private void AddCells()
    {
        if (_numberOfPlayers >= 2)
        {
            AddDotsToCells(1, 1, _1PlayerColor, _1PlayerMaterial, CellInstance.Cell.Team.Team1);
            AddDotsToCells(_gridSize - 2, _gridSize - 2, _2PlayerColor, _2PlayerMaterial, CellInstance.Cell.Team.Team2);
            if (_numberOfPlayers >= 3)
            {
                AddDotsToCells(1, _gridSize - 2, _3PlayerColor, _3PlayerMaterial, CellInstance.Cell.Team.Team3);
                if (_numberOfPlayers >= 4)
                {
                    AddDotsToCells(_gridSize - 2, 1, _4PlayerColor, _4PlayerMaterial, CellInstance.Cell.Team.Team4);
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

    private void AddDotsToCells(int posColumn, int posRow, Color teamColor, Material material, CellInstance.Cell.Team team)
    {
        CellInstance.Cell cell = GetCellAtPos(posColumn, posRow);
        cell.SetTeam(teamColor, material, team);
        cell.AddDot();
    }

    private void AddToNearbyCells(object arg1, AddToNearbyCells cellData)
    {
        List<CellInstance.Cell> cells = new List<CellInstance.Cell>
        {
            cellData.neighbours.Item1,
            cellData.neighbours.Item2,
            cellData.neighbours.Item3,
            cellData.neighbours.Item4
        };
        foreach (var cell in cells)
        {
            if (cell?.NumberOfDots == 0)
            {
                cell.SetTeam(cellData.teamColor, cellData.material, cellData.team);
                cell.AddDot();
            }
            else if (cell?.TeamColor == cellData.teamColor)
            {
                cell.AddDot();
            }
            else if (cell != null)
            {
                cell.SetTeam(cellData.teamColor, cellData.material, cellData.team);
                cell.AddDot();
            }
        }
    }

    [ContextMenu("AddNeighboursToCells")]
    private void AddNeighboursToCells()
    {
        foreach (var cell in _cells)
        {
            AddNeighbours(cell);
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
