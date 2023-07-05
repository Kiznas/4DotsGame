using System;
using UnityEngine;
using EventHandler;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Team { None, Team1, Team2, Team3, Team4 }

public class CellInstance : MonoBehaviour
{
    [SerializeField] private ImageCombiner _imageCombiner;
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;
    private Cell _cell;
    private bool _teamTurn;

    Dictionary<Team, GameStates> TeamDictionary = new Dictionary<Team, GameStates>()
{
    { Team.Team1, GameStates.PLAYER1TURN },
    { Team.Team2, GameStates.PLAYER2TURN },
    { Team.Team3, GameStates.PLAYER3TURN },
    { Team.Team4, GameStates.PLAYER4TURN }
};


    public class Cell
    {
        public event Action DotAdded;
        public event Action TeamColorChanged;
        public event Action UpdatedImage;
        private int _posRow;
        private int _posColumn;
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
            _material.color = _teamColor != null ? _teamColor : _material.color;
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
        EventAggregator.Subscribe<GetTurn>(SetTurn);
    }

    private void SetTurn(object arg1, GetTurn turn)
    {
        if(_cell.CellTeam != Team.None)
        {
            if (turn.gameState == TeamDictionary[_cell.CellTeam])
            {
                StartCoroutine(SetTurn());
            }
            else
            {
                _teamTurn = false;
            }
        }
    }

    private void OnClick()
    {
        if (_cell.TeamColor != Color.white && _teamTurn)
        {
            if(_cell.NumberOfDots != 3)
            {
                EventAggregator.Post(this, new NextTurn { cellTeam = _cell.CellTeam });
            }
            _cell.AddDot();
            _cell.Material.SetInt("_IsGlowing", 0);
        }
    }

    private IEnumerator SetTurn()
    {
        yield return new WaitForSeconds(0.3f);
        _cell.Material.SetInt("_IsGlowing", 1);
        _cell.Material.SetFloat("_Current_Time", Time.time);
        _teamTurn = true;
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
        if (_cell.NumberOfDots != 0)
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

    public bool CheckForThreeInNeighbours()
    {
        if (_cell.Neighbours.top?.NumberOfDots == 3 ||
            _cell.Neighbours.bottom?.NumberOfDots == 3 ||
            _cell.Neighbours.right?.NumberOfDots == 3 ||
            _cell.Neighbours.left?.NumberOfDots == 3)
        {
            return true;
        }
        return false;
    }

    IEnumerator AddCells()
    {
        yield return new WaitForSeconds(0.2f);

        Team cellTeam = _cell.CellTeam;
        int cellPosColumn = _cell.PosColumn;
        int cellPosRow = _cell.PosRow;
        Color cellTeamColor = _cell.TeamColor;
        Material cellMat = _cell.Material;

        _cell.ClearCell();

        yield return new WaitForSeconds(0.2f);

        EventAggregator.Post(this, new AddToNearbyCells { posColumn = cellPosColumn, posRow = cellPosRow, teamColor = cellTeamColor, material = cellMat, team = cellTeam, neighbours = _cell.Neighbours });
        _imageCombiner.ClearImage((Texture2D)_image.mainTexture);
    }
}