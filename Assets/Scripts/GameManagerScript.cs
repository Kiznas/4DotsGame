using EventHandler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

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

    private int _cellIndex;
    private int _gridSize;

    void Start()
    {
        _gridSize = gridGenerator._gridSize;
        _cells = new CellInstance.Cell[_gridSize * _gridSize];
        EventAggregator.Subscribe<CellAdded>(AddCellToArray);
        EventAggregator.Subscribe<AddToNearbyCells>(AddToNearbyCells);
        EventAggregator.Subscribe<CheckNearbyCells>(CheckNearbyCells);

        _1PlayerColor = _colorizer.AddGlowing(_1PlayerColor);
        _2PlayerColor = _colorizer.AddGlowing(_2PlayerColor);
        _3PlayerColor = _colorizer.AddGlowing(_3PlayerColor);
        _4PlayerColor = _colorizer.AddGlowing(_4PlayerColor);
    }

    private void AddCellToArray(object arg1, CellAdded _cell)
    {
        _cells[_cellIndex++] = _cell.CellInstance;
    }

    [ContextMenu("Method")]
    private void Method()
    {
        if (_numberOfPlayers >= 2)
        {
            AddDotsToCells(1, 1, _1PlayerColor, _1PlayerMaterial);
            AddDotsToCells(_gridSize - 2, _gridSize - 2, _2PlayerColor, _2PlayerMaterial);
            if(_numberOfPlayers >= 3)
            {
                AddDotsToCells(1, _gridSize - 2, _3PlayerColor, _3PlayerMaterial);
                if (_numberOfPlayers >= 4)
                {
                    AddDotsToCells(_gridSize - 2, 1, _4PlayerColor, _4PlayerMaterial);
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

    private void AddDotsToCells(int posColumn, int posRow, Color teamColor, Material material)
    {
        CellInstance.Cell cell = GetCellAtPos(posColumn, posRow);
        cell.SetTeam(teamColor, material);
        cell.AddDot();
    }

    private void AddToNearbyCells(object arg1, AddToNearbyCells cellData)
    {
        CellInstance.Cell cellInstance = cellData.CellInstance;
        foreach (var cell in _cells)
        {
            if (IsAdjacent(cell, cellInstance))
            {
                if (cell.NumberOfDots == 0)
                {
                    cell.SetTeam(cellInstance.TeamColor, cellInstance.Material);
                    cell.AddDot();
                }
                else if (cell.TeamColor == cellInstance.TeamColor)
                {
                    cell.AddDot();
                }
                else
                {
                    cell.SetTeam(cellInstance.TeamColor, cellInstance.Material);
                    cell.AddDot();
                }
            }
        }
    }

    private void CheckNearbyCells(object arg1, CheckNearbyCells cellInstance)
    {
        CellInstance.Cell cell = cellInstance.CellInstance;
        (int posX, int posY) topPos = (cell.PosColumn, cell.PosRow - 1);
        (int posX, int posY) rightPos = (cell.PosColumn + 1, cell.PosRow);
        (int posX, int posY) bottomPos = (cell.PosColumn, cell.PosRow + 1);
        (int posX, int posY) leftPos = (cell.PosColumn - 1, cell.PosRow);
        foreach (var neighbourCell in _cells)
        {
            if (neighbourCell.PosColumn == topPos.posX && neighbourCell.PosRow == topPos.posY)
            {
                if (neighbourCell.TeamColor == cell.TeamColor)
                {
                    cell.Neighbours = (true, cell.Neighbours.right, cell.Neighbours.bottom, cell.Neighbours.left);
                }
            }
            if (neighbourCell.PosColumn == rightPos.posX && neighbourCell.PosRow == rightPos.posY)
            {
                if (neighbourCell.TeamColor == cell.TeamColor)
                {
                    cell.Neighbours = (cell.Neighbours.top, true, cell.Neighbours.bottom, cell.Neighbours.left);
                }
            }
            if (neighbourCell.PosColumn == bottomPos.posX && neighbourCell.PosRow == bottomPos.posY)
            {
                if (neighbourCell.TeamColor == cell.TeamColor)
                {
                    cell.Neighbours = (cell.Neighbours.top, cell.Neighbours.right, true, cell.Neighbours.left);
                }
            }
            if (neighbourCell.PosColumn == leftPos.posX && neighbourCell.PosRow == leftPos.posY)
            {
                if (neighbourCell.TeamColor == cell.TeamColor)
                {
                    cell.Neighbours = (cell.Neighbours.top, cell.Neighbours.right, cell.Neighbours.bottom, true);
                }
            }
        }
    }

    private bool IsAdjacent(CellInstance.Cell cell, CellInstance.Cell cell2)
    {
        int columnDifference = Mathf.Abs(cell.PosColumn + 1);
        int rowDifference = Mathf.Abs(cell.PosRow - cell2.PosRow);
        return columnDifference == -1 && rowDifference == -1;
    }

}
