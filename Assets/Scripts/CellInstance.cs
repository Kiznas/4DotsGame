using System.Linq;
using UnityEngine;
using EventHandler;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum Team { None, Team1, Team2, Team3, Team4 }

public class CellInstance : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private ParticleSystem _particle;
    public ImageCombiner ImageCombiner;
    public Image Image;

    private List<Team> _teams;

    private Cell _cell;
    public bool TeamTurn;

    readonly Dictionary<Team, GameStates> TeamDictionary = new()
    {
        { Team.Team1, GameStates.PLAYER1TURN },
        { Team.Team2, GameStates.PLAYER2TURN },
        { Team.Team3, GameStates.PLAYER3TURN },
        { Team.Team4, GameStates.PLAYER4TURN }
    };

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
        _cell = new Cell(row, column);
        _cell.CellInstance = this;
        EventAggregator.Post(this, new CellAdded { Cell = _cell });
        EventAggregator.Subscribe<GetTurn>(SetTurn);
        EventAggregator.Subscribe<PrepareForNextTurn>(PrepareTurn);
        EventAggregator.Subscribe<AddBots>(AddBotsTeams);
    }

    private void AddBotsTeams(object arg1, AddBots data)
    {
        _teams = data.teams;
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
            var reversed = TeamDictionary.ToDictionary(x => x.Value, x => x.Key);
            if (turn.gameState == TeamDictionary[_cell.CellTeam] && !_teams.Contains(reversed[turn.gameState]))
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
            TeamTurn = false;
            _cell.Material.SetInt("_IsGlowing", 0);
            EventAggregator.Post(this, new PrepareForNextTurn { cellTeam = _cell.CellTeam });
            if(_cell.NumberOfDots != 3) { StartCoroutine(NextTurnDelay()); }
            _cell.AddDot();
        }
    }

    private IEnumerator NextTurnDelay()
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame);
        EventAggregator.Post(this, new NextTurn { cellTeam = _cell.CellTeam });
    }

    public IEnumerator AddCells()
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame);
        if(_cell.TeamColor != Color.white)
        {
            StartCoroutine(Animation());
            EventAggregator.Post(this, new AddToNearbyCells { cell = _cell });
        }
    }

    public IEnumerator SpreadAnimation()
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame);
        if(_cell.TeamColor != Color.white)
        {
            StartCoroutine(Animation());
        }
    }

    private IEnumerator Animation()
    {
        ParticleSystem particle = Instantiate(_particle, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform);
        var particleMain = particle.main;
        particleMain.startColor = _cell.TeamColor;
        Destroy(particle.gameObject, 3f);
        yield return null;
    }
}
public class Cell
{
    public CellInstance CellInstance;
    private readonly int _posRow;
    private readonly int _posColumn;
    private int _numberOfDots;
    private Color _teamColor = Color.white;
    private Material _material;
    private (Cell, Cell, Cell, Cell) _neighbours;
    private Team _team;

    public int PosRow
    {
        get { return _posRow; }
    }
    public int PosColumn
    {
        get { return _posColumn; }
    }
    public Team CellTeam
    {
        get { return _team; }
        set { _team = value; }
    }
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
        if (_numberOfDots >= 4)
        {
            UpdateImage();
            CellInstance.StartCoroutine(CellInstance.AddCells());
        }
        else
        {
            UpdateImage();
        }
        CellInstance.TeamTurn = false;
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
        CellInstance.Image.material = _material;
    }
    public void ClearCell()
    {
        _team = Team.None;
        _teamColor = Color.white;
        _numberOfDots = 0;
        CellInstance.ImageCombiner.ClearImage((Texture2D)CellInstance.Image.mainTexture);
    }
    public void UpdateImage()
    {
        if (_numberOfDots != 0 && _team != Team.None)
        {
            CellInstance.ImageCombiner.CombineImages(_numberOfDots,
                                     _neighbours.Item1?.CellTeam == _team,
                                     Neighbours.right?.CellTeam == _team,
                                     Neighbours.bottom?.CellTeam == _team,
                                     Neighbours.left?.CellTeam == _team);
        }
    }

    public bool NeighboursHasLessThan3Dots()
    {
        return (Neighbours.top?.NumberOfDots < 3 ||
                Neighbours.bottom?.NumberOfDots < 3 ||
                Neighbours.right?.NumberOfDots < 3 ||
                Neighbours.left?.NumberOfDots < 3);
    }

    public bool HasAdjacentCellWith3Dots(Cell targetCell)
    {
        return (Neighbours.top == targetCell && Neighbours.top?.NumberOfDots == 3) ||
               (Neighbours.bottom == targetCell && Neighbours.bottom?.NumberOfDots == 3) ||
               (Neighbours.right == targetCell && Neighbours.right?.NumberOfDots == 3) ||
               (Neighbours.left == targetCell && Neighbours.left?.NumberOfDots == 3);
    }

    public float GetDistanceToOpponent(List<Cell> opponentCells)
    {
        return opponentCells.Min(opponentCell => Vector2.Distance(new Vector2(PosColumn, PosRow), new Vector2(opponentCell.PosColumn, opponentCell.PosRow)));
    }

}