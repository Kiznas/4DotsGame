using EventHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellInstance : MonoBehaviour
{
    [SerializeField] public Cell _cell;
    [SerializeField] private ImageCombiner _imageCombiner;
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;

    public class Cell
    {
        public event Action DotAdded;
        public event Action TeamColorChanged;
        private int _posRow;
        private int _posColumn;
        private bool _filled;
        private int _numberOfDots;
        private Color _teamColor;
        private Material _material;
        private (bool, bool, bool, bool) _neighbours;

        public int PosRow
        {
            get { return _posRow; }
            set { _posRow = value; }
        }
        public int PosColumn
        {
            get { return _posColumn; }
            set { _posColumn = value; }
        }
        public bool Filled
        {
            get { return _filled; }
            set { _filled = value; }
        }
        public int NumberOfDots
        {
            get { return _numberOfDots; }
        }
        public (bool top, bool right, bool bottom, bool left) Neighbours
        {
            get { return _neighbours; }
            set { _neighbours = value; }
        }
        public Color TeamColor
        {
            get { return _teamColor; }
        }
        public Material Material
        {
            get { return _material; }
        }
        public Cell(int posX, int posY)
        {
            _posRow = posX;
            _posColumn = posY;
        }

        public void AddDot()
        {
            _numberOfDots++;
            DotAdded.Invoke();
        }

        public void SetTeam(Color teamColor, Material material)
        {
            _teamColor = teamColor;
            _material = material;
            _material.color = _teamColor;
            TeamColorChanged.Invoke();
        }

        public void ClearCell()
        {
            _teamColor = Color.white;
            _material = null;
            _numberOfDots = 0;
        }
    }

    private void Start()
    {
        _button.onClick.AddListener(OnClick);
    }
    public void CreateCellInstance(int row, int column)
    {
        _cell = new Cell(row, column);
        _cell.DotAdded += DotAdded;
        _cell.TeamColorChanged += TeamChanged;
        EventAggregator.Post(this, new CellAdded { CellInstance = _cell });
    }

    private void OnClick()
    {
        _cell.AddDot();
        Debug.Log($"Row{_cell.PosRow + 1}, Collumn {_cell.PosColumn + 1}");
    }
    private void DotAdded()
    {
        if (_cell.NumberOfDots >= 4)
        {
            EventAggregator.Post(this, new AddToNearbyCells { CellInstance = _cell });
            _cell.ClearCell();
            _imageCombiner.ClearImage((Texture2D)_image.mainTexture);
        }
        else
        {
            EventAggregator.Post(this, new CheckNearbyCells { CellInstance = _cell });
            _imageCombiner.CombineImages(_cell.NumberOfDots, _cell.Neighbours.top, _cell.Neighbours.right, _cell.Neighbours.bottom, _cell.Neighbours.left);
        }
    }

    private void TeamChanged()
    {
        _image.material = _cell.Material;
    }
}


