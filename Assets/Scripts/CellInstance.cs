using System.Linq;
using UnityEngine;
using EventHandler;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CellInstance : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private ParticleSystem _particle;
    public ImageCombiner ImageCombiner;
    public Image Image;

    private List<Team> _botTeams;

    private Cell _cell;
    public bool TeamTurn;

    private void Start()
    {
        _button.onClick.AddListener(OnClick);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveAllListeners();
        EventAggregator.Unsubscribe<GetTurn>(SetTurn);
        EventAggregator.Unsubscribe<PrepareForNextTurn>(PrepareTurn);
        EventAggregator.Unsubscribe<AddBots>(AddBotsTeams);
        if (_cell != null && _cell.Material != null)
        {
            _cell.Material.SetInt("_IsGlowing", 0);
        }
    }
    public void CreateCellInstance(int row, int column)
    {
        _cell = new Cell(row, column, this);
        EventAggregator.Post(this, new CellAdded { Cell = _cell });
        EventAggregator.Subscribe<GetTurn>(SetTurn);
        EventAggregator.Subscribe<PrepareForNextTurn>(PrepareTurn);
        EventAggregator.Subscribe<AddBots>(AddBotsTeams);
    }

    private void AddBotsTeams(object arg1, AddBots data)
    {
        _botTeams = data.teams;
    }

    private void PrepareTurn(object arg1, PrepareForNextTurn team)
    {
        if (team.cellTeam == _cell.CellTeam)
        {
            TeamTurn = false;
            _cell.Material.SetInt("_IsGlowing", 0);
        }
    }

    private void SetTurn(object arg1, GetTurn turn)
    {
        if(_cell.CellTeam != Team.None)
        {
            var reversed = Constants.TeamsDictionary.ToDictionary(x => x.Value, x => x.Key);
            if (turn.gameState == Constants.TeamsDictionary[_cell.CellTeam] && !_botTeams.Contains(reversed[turn.gameState]))
            {
                _cell.Material.SetInt("_IsGlowing", 1);
                _cell.Material.SetFloat("_Current_Time", Time.time);
                TeamTurn = true;
            }
        }
    }

    private void OnClick()
    {
        if (_cell.TeamColor != Color.white && TeamTurn)
        {
            EventAggregator.Post(this, new PrepareForNextTurn { cellTeam = _cell.CellTeam });
            if(_cell.NumberOfDots != 3) { StartCoroutine(NextTurnWithDelay()); }
            _cell.AddDot();
        }
    }

    private IEnumerator NextTurnWithDelay()
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame);
        EventAggregator.Post(this, new NextTurn { cellTeam = _cell.CellTeam });
    }

    public IEnumerator AddToNearby()
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame);

        StartCoroutine(SpreadAnimation(false, _cell.TeamColor));
        EventAggregator.Post(this, new AddToNearbyCells { cell = _cell });
    }

    public IEnumerator SpreadAnimation(bool withDelay, Color teamColor)
    {
        if(withDelay)
        {
            yield return new WaitForSeconds(Constants.SpeedOfGame);
        }
        ParticleSystem particle = Instantiate(_particle, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform);
        var particleMain = particle.main;
        particleMain.startColor = teamColor;
        Destroy(particle.gameObject, 3f);
        yield return null;
    }
}
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

    public int PosRow{ get { return _posRow; } }
    public int PosColumn { get { return _posColumn; } }
    public Team CellTeam { get { return _team; } }
    public Color TeamColor { get { return _teamColor; } }
    public Material Material{ get { return _material; } }
    public CellInstance CellInstance { get { return _cellInstance; } }

    public int NumberOfDots
    {
        get { return _numberOfDots; }
        set { _numberOfDots = value; }
    }
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