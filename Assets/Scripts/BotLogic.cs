using EventHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

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

    private void TakeBotTurn(Team botTeam)
    {
        var opponentCells = _gameManager.Cells.Where(cell => cell.CellTeam != botTeam).ToList();
        var botCells = _gameManager.Cells.Where(cell => cell.CellTeam == botTeam).ToList();

        // Variant 1: Check if there is a bot cell with 3 dots and adjacent to an opponent cell with 3 dots
        var botCellWith3DotsAndAdjacentToOpponentCellWith3Dots = GetBotCellWithDotsAndAdjacentOpponentCellWithDots(botCells, opponentCells, 3);
        if (botCellWith3DotsAndAdjacentToOpponentCellWith3Dots != null)
        {
            ProcessBotCellTurn(botCellWith3DotsAndAdjacentToOpponentCellWith3Dots, botTeam);
            botCellWith3DotsAndAdjacentToOpponentCellWith3Dots.AddDot();
            return;
        }

        // Variant 2: Check if there is a bot cell with 2 dots and adjacent to an opponent cell with 3 dots
        var botCellWith2DotsAndAdjacentToOpponentCellWith3Dots = GetBotCellWithDotsAndAdjacentOpponentCellWithDots(botCells, opponentCells, 2);
        if (botCellWith2DotsAndAdjacentToOpponentCellWith3Dots != null)
        {
            ProcessBotCellTurn(botCellWith2DotsAndAdjacentToOpponentCellWith3Dots, botTeam);
            botCellWith2DotsAndAdjacentToOpponentCellWith3Dots.AddDot();
            return;
        }

        // Variant 3: Check if there is an opponent cell with 3 dots and attack it if there is a bot cell adjacent to it
        var opponentCellWith3Dots = opponentCells.FirstOrDefault(cell => cell.NumberOfDots == 3);
        if (opponentCellWith3Dots != null)
        {
            var botCellToAttack = botCells.FirstOrDefault(cell => cell.HasAdjacentCellWith3Dots(opponentCellWith3Dots));
            if (botCellToAttack != null)
            {
                ProcessBotCellTurn(botCellToAttack, botTeam);
                botCellToAttack.AddDot();
                return;
            }
        }

        // Variant 4: Check if there is a bot cell with 2 dots and adjacent to an opponent cell with 2 dots
        var botCellWith2DotsAndAdjacentToOpponentCellWith2Dots = GetBotCellWithDotsAndAdjacentOpponentCellWithDots(botCells, opponentCells, 2);
        if (botCellWith2DotsAndAdjacentToOpponentCellWith2Dots != null)
        {
            ProcessBotCellTurn(botCellWith2DotsAndAdjacentToOpponentCellWith2Dots, botTeam);
            botCellWith2DotsAndAdjacentToOpponentCellWith2Dots.AddDot();
            return;
        }

        // Variant 5: Find the closest bot cell to an opponent cell and attack it
        var closestBotCellToOpponentCell = FindClosestBotCellToOpponentCell(botCells, opponentCells);
        if (closestBotCellToOpponentCell != null)
        {
            ProcessBotCellTurn(closestBotCellToOpponentCell, botTeam);
            closestBotCellToOpponentCell.AddDot();
            return;
        }
    }

    private Cell FindClosestBotCellToOpponentCell(List<Cell> botCells, List<Cell> opponentCells)
    {
        Cell closestBotCell = null;
        float minDistance = float.MaxValue;

        foreach (var botCell in botCells)
        {
            float distanceToOpponent = botCell.GetDistanceToOpponent(opponentCells);
            if (distanceToOpponent < minDistance)
            {
                minDistance = distanceToOpponent;
                closestBotCell = botCell;
            }
        }

        return closestBotCell;
    }

    private Cell GetBotCellWithDotsAndAdjacentOpponentCellWithDots(List<Cell> botCells, List<Cell> opponentCells, int dots)
    {
        return botCells.FirstOrDefault(botCell =>
            botCell.NumberOfDots == dots &&
            opponentCells.Any(opponentCell =>
                botCell.HasAdjacentCellWith3Dots(opponentCell) &&
                opponentCell.NumberOfDots == dots
            )
        );
    }

    private void ProcessBotCellTurn(Cell botCell, Team botTeam)
    {
        GiveTurnIfNotThreeDots(botCell, botTeam);
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