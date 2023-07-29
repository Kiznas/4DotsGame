using System.Threading.Tasks;
using UnityEngine;

public class Cell
{
    private CellInstance _cellInstance;
    private readonly int _posRow;
    private readonly int _posColumn;
    private int _numberOfDots;
    private Color _teamColor = Color.white;
    private Material _material;
    private (Cell, Cell, Cell, Cell) _neighbours;
    private Team _team;

    public int PosRow { get { return _posRow; } }
    public int PosColumn { get { return _posColumn; } }
    public int NumberOfDots { get { return _numberOfDots; } }
    public Team CellTeam { get { return _team; } }
    public Color TeamColor { get { return _teamColor; } }
    public Material Material { get { return _material; } }
    public CellInstance CellInstance { get { return _cellInstance; } }
    
    public (Cell top, Cell right, Cell bottom, Cell left) Neighbours
    {
        get { return _neighbours; }
        set { _neighbours = value; }
    }

    public Cell(int posX, int posY, CellInstance cellInstance)
    {
        _posRow = posX;
        _posColumn = posY;
        _cellInstance = cellInstance;
    }

    public void AddDot()
    {
        _numberOfDots++;
        if (_numberOfDots >= 4)
        {
            UpdateImage();
            _cellInstance.StartCoroutine(_cellInstance.AddToNearby());
        }
        else
        {
            UpdateImage();
        }
    }
    public void SetTeam(Color teamColor, Material material, Team team)
    {
        _team = team;
        _teamColor = teamColor;
        _material = material;
        if (teamColor != null && _material != null)
        {
            _material.color = teamColor;
        }
        _cellInstance.Image.material = _material;
    }
    public void ClearCell()
    {
        _team = Team.None;
        _teamColor = Color.white;
        _numberOfDots = 0;
        _cellInstance.ImageCombiner.ClearImage((Texture2D)_cellInstance.Image.mainTexture);
    }
    public void UpdateImage()
    {
        if (_numberOfDots != 0 && _team != Team.None)
        {
            _cellInstance.ImageCombiner.CombineImages(_numberOfDots,
                                     Neighbours.top?.CellTeam == _team,
                                     Neighbours.right?.CellTeam == _team,
                                     Neighbours.bottom?.CellTeam == _team,
                                     Neighbours.left?.CellTeam == _team);
        }
    }
    public async Task UpdateImageAsync()
    {
        if (_numberOfDots != 0 && _team != Team.None)
        {
            await _cellInstance.ImageCombiner.CombineImagesAsync(_numberOfDots,
                                     Neighbours.top?.CellTeam == _team,
                                     Neighbours.right?.CellTeam == _team,
                                     Neighbours.bottom?.CellTeam == _team,
                                     Neighbours.left?.CellTeam == _team);
        }
    }
}