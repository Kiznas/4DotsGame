using EventHandler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CellInstance : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private ParticleSystem _particle;
    public ImageCombiner ImageCombiner;
    public Image Image;

    private List<Team> _botTeams;

    private Cell _cell;
    private bool _teamTurn;

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
            _teamTurn = false;
            _cell.Material.SetInt("_IsGlowing", 0);
        }
    }

    private void SetTurn(object arg1, GetTurn turn)
    {
        if (_cell.CellTeam != Team.None)
        {
            var reversed = Constants.TeamsDictionary.ToDictionary(x => x.Value, x => x.Key);
            if (turn.gameState == Constants.TeamsDictionary[_cell.CellTeam] && !_botTeams.Contains(reversed[turn.gameState]))
            {
                _cell.Material.SetInt("_IsGlowing", 1);
                _cell.Material.SetFloat("_Current_Time", Time.time);
                _teamTurn = true;
            }
        }
    }

    private void OnClick()
    {
        if (_cell.TeamColor != Color.white && _teamTurn)
        {
            EventAggregator.Post(this, new PrepareForNextTurn { cellTeam = _cell.CellTeam });
            if (_cell.NumberOfDots != 3) { StartCoroutine(NextTurnWithDelay()); }
            _cell.AddDot();
        }
    }

    private IEnumerator NextTurnWithDelay()
    {
        yield return new WaitForSeconds(Constants.SPEEDOFGAME);
        EventAggregator.Post(this, new NextTurn { cellTeam = _cell.CellTeam });
    }

    public IEnumerator AddToNearby()
    {
        yield return new WaitForSeconds(Constants.SPEEDOFGAME);

        StartCoroutine(SpreadAnimation(false, _cell.TeamColor));
        EventAggregator.Post(this, new AddToNearbyCells { cell = _cell });
    }

    public IEnumerator SpreadAnimation(bool withDelay, Color teamColor)
    {
        if (withDelay)
        {
            yield return new WaitForSeconds(Constants.SPEEDOFGAME);
        }
        ParticleSystem particle = Instantiate(_particle, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform);
        var particleMain = particle.main;
        particleMain.startColor = teamColor;
        Destroy(particle.gameObject, 3f);
        yield return null;
    }
}