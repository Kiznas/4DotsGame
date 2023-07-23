using EventHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotLogic : MonoBehaviour
{
    [SerializeField] private GameManagerScript _gameManager;
    [SerializeField] private PlayersTurnSystem _turnSysten;
    private List<Team> _botTeams = new List<Team>();

    readonly Dictionary<Team, GameStates> TeamDictionary = new()
    {
        { Team.Team1, GameStates.PLAYER1TURN },
        { Team.Team2, GameStates.PLAYER2TURN },
        { Team.Team3, GameStates.PLAYER3TURN },
        { Team.Team4, GameStates.PLAYER4TURN }
    };

    private void Start()
    {
        EventAggregator.Subscribe<GetTurn>(OnGetTurn);
        EventAggregator.Subscribe<AddBots>(AddBotsToList);
    }

    private void AddBotsToList(object arg1, AddBots data)
    {
        _botTeams = data.teams;
    }

    private void OnDestroy()
    {
        EventAggregator.Unsubscribe<GetTurn>(OnGetTurn);
        EventAggregator.Unsubscribe<AddBots>(AddBotsToList);
    }

    private void OnGetTurn(object sender, GetTurn turnData)
    {
        foreach (Team botTeam in _botTeams)
        {
            if (turnData.gameState == TeamDictionary[botTeam])
            {
                StartCoroutine(BotDelay(botTeam));
                return; 
            }
        }
    }

    private IEnumerator BotDelay(Team team)
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame);
        TakeBotTurn(team);
    }

    private void TakeBotTurn(Team _botTeam)
    {
        var opponentCells = _gameManager.Cells.Where(cell => cell.CellTeam != _botTeam).ToList();
        var botCells = _gameManager.Cells.Where(cell => cell.CellTeam == _botTeam).ToList();

        var botAndOpponentCellWith3Dots = botCells.FirstOrDefault(botCell => botCell.NumberOfDots == 3 && opponentCells.Any(opponentCell => botCell.HasAdjacentCellWith3Dots(opponentCell) && opponentCell.NumberOfDots == 3));
        if (botAndOpponentCellWith3Dots != null)
        {
            GiveTurnIfNotThreeDots(botAndOpponentCellWith3Dots, _botTeam);
            botAndOpponentCellWith3Dots.AddDot();
            return;
        }

        var botCellWith2DotsAndAdjacentToOpponentCell = botCells.FirstOrDefault(botCell => botCell.NumberOfDots == 2 && opponentCells.Any(opponentCell => botCell.HasAdjacentCellWith3Dots(opponentCell)));
        if (botCellWith2DotsAndAdjacentToOpponentCell != null)
        {
            GiveTurnIfNotThreeDots(botCellWith2DotsAndAdjacentToOpponentCell, _botTeam);
            botCellWith2DotsAndAdjacentToOpponentCell.AddDot();
            return;
        }

        var opponentCellWith3Dots = opponentCells.FirstOrDefault(opponentCell => opponentCell.NumberOfDots == 3);
        if (opponentCellWith3Dots != null)
        {
            var botCellToAttack = botCells.FirstOrDefault(botCell => botCell.HasAdjacentCellWith3Dots(opponentCellWith3Dots));
            if (botCellToAttack != null)
            {
                GiveTurnIfNotThreeDots(botCellToAttack, _botTeam);
                botCellToAttack.AddDot();
                return;
            }
        }

        var botCellWith2DotsAdjacentToOpponentCellWith2Dots = botCells.FirstOrDefault(botCell => botCell.NumberOfDots == 2 && opponentCells.Any(opponentCell => botCell.HasAdjacentCellWith3Dots(opponentCell) && opponentCell.NumberOfDots == 2));
        if (botCellWith2DotsAdjacentToOpponentCellWith2Dots != null)
        {
            GiveTurnIfNotThreeDots(botCellWith2DotsAdjacentToOpponentCellWith2Dots, _botTeam);
            botCellWith2DotsAdjacentToOpponentCellWith2Dots.AddDot();
            return;
        }

        var closestOpponentCell = opponentCells.OrderBy(opponentCell => botCells.Min(botCell => Vector2.Distance(new Vector2(botCell.PosColumn, botCell.PosRow),
            new Vector2(opponentCell.PosColumn, opponentCell.PosRow)))).FirstOrDefault();
        if (closestOpponentCell != null)
        {
            var botCellToMove = botCells.OrderBy(botCell => Vector2.Distance(new Vector2(botCell.PosColumn, botCell.PosRow), new Vector2(closestOpponentCell.PosColumn, closestOpponentCell.PosRow))).FirstOrDefault();
            if (botCellToMove != null)
            {
                GiveTurnIfNotThreeDots(botCellToMove, _botTeam);
                botCellToMove.AddDot();
                return;
            }
        }
    }

    private void GiveTurnIfNotThreeDots(Cell botCell, Team team)
    {
        if (botCell.NumberOfDots != 3)
        {
            StartCoroutine(GiveNextTurn(team));
        }
        _gameManager.Cells.FirstOrDefault(botCell => botCell.CellTeam == team)?.Material.SetInt("_IsGlowing", 0);
    }

    private IEnumerator GiveNextTurn(Team team)
    {
        _gameManager.Cells.FirstOrDefault(botCell => botCell.CellTeam == team)?.Material.SetInt("_IsGlowing", 0);
        yield return new WaitForSeconds(Constants.SpeedOfGame);
        EventAggregator.Post(this, new NextTurn { cellTeam = team });
    }
}