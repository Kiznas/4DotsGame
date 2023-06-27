using System;
using UnityEngine;
using EventHandler;
using UnityEngine.UI;
using System.Collections;

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
        public event Action UpdatedImage;
        private int _posRow;
        private int _posColumn;
        private bool _filled;
        private int _numberOfDots;
        private Color _teamColor = Color.white;
        private Material _material;
        private (Cell, Cell, Cell, Cell) _neighbours;
        private Team _team;

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
        public Team CellTeam
        {
           get { return _team; }
           set { _team = value; }
        }
        public int NumberOfDots
        {
            get { return _numberOfDots; }
        }
        public (Cell top, Cell right, Cell bottom, Cell left) Neighbours
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
        public void SetTeam(Color teamColor, Material material, Team team)
        {
            _team = team;
            _teamColor = teamColor;
            _material = material;
            _material.color = _teamColor;
            TeamColorChanged.Invoke();
        }
        public void ClearCell()
        {
            _team = Team.None;
            _teamColor = Color.white; 
            _material = null;
            _numberOfDots = 0;
        }
        public void UpdateImage()
        {
            UpdatedImage.Invoke();
        }

        public enum Team
        {
            None,
            Team1,
            Team2,
            Team3,
            Team4
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
        _cell.UpdatedImage += UpdateImage;
        EventAggregator.Post(this, new CellAdded { CellInstance = _cell });
    }

    private void OnClick()
    {
        if (_cell.TeamColor != Color.white)
        {
            _cell.AddDot();
            Debug.Log($"Row{_cell.PosRow + 1}, Collumn {_cell.PosColumn + 1}, Team {_cell.TeamColor}, neighbours {_cell.Neighbours}");
        }

        Debug.Log($"Row{_cell.PosRow + 1}, Collumn {_cell.PosColumn + 1}, Team {_cell.TeamColor}, neighbours {_cell.Neighbours}");

    }
    private void DotAdded()
    {
        if (_cell.NumberOfDots >= 4)
        {
            UpdateImage();
            StartCoroutine(AddCells());
        }
        else
        {
            UpdateImage();
        }
    }

    public void UpdateImage()
    {
        if(_cell.NumberOfDots != 0) 
        {
            _imageCombiner.CombineImages(_cell.NumberOfDots,
                                     _cell.Neighbours.top?.CellTeam == _cell.CellTeam,
                                     _cell.Neighbours.right?.CellTeam == _cell.CellTeam,
                                     _cell.Neighbours.bottom?.CellTeam == _cell.CellTeam,
                                     _cell.Neighbours.left?.CellTeam == _cell.CellTeam);
        }
    }

    private void TeamChanged()
    {
        _image.material = _cell.Material;
    }

    IEnumerator AddCells()
    {
        yield return new WaitForSeconds(0.3f);

        Cell.Team cellTeam = _cell.CellTeam;
        int posCol = _cell.PosColumn;
        int posRow = _cell.PosRow;
        Color color = _cell.TeamColor;
        Material cellMat = _cell.Material;

        _cell.ClearCell();

        yield return new WaitForSeconds(0.1f);

        EventAggregator.Post(this, new AddToNearbyCells { posColumn = posCol, posRow = posRow, teamColor = color, material = cellMat, team = cellTeam, neighbours = _cell.Neighbours });
        _imageCombiner.ClearImage((Texture2D)_image.mainTexture);
    }
}