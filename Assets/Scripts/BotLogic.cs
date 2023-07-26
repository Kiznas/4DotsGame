using System.Linq;
using UnityEngine;
using EventHandler;
using System.Collections;
using System.Collections.Generic;

public class BotLogic : MonoBehaviour
{
    [SerializeField] private GameManagerScript _gameManager;
    [SerializeField] private PlayersTurnSystem _turnSysten;
    private List<Team> _botTeams = new List<Team>();

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
            if (turnData.gameState == Constants.TeamsDictionary[botTeam])
            {
                StartCoroutine(BotDelay(botTeam));
                return; 
            }
        }
    }

    private IEnumerator BotDelay(Team team)
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame/2);
        TakeBotTurn(team);
    }

    private void TakeBotTurn(Team botTeam)
    {
        var opponentCells = _gameManager.Cells.Where(cell => cell.CellTeam != botTeam && cell.CellTeam != Team.None).ToList();
        var botCells = _gameManager.Cells.Where(cell => cell.CellTeam == botTeam).ToList();

        // Variant 1: Check if there is a bot cell with 3 dots and adjacent to an opponent cell with 3 dots
        var botCellWith3DotsAndAdjacentToOpponentCellWith3Dots = GetAdjacentCellWithNDots(botCells, opponentCells, 3);
        if (botCellWith3DotsAndAdjacentToOpponentCellWith3Dots != null)
        {
            ProcessBotCellTurn(botCellWith3DotsAndAdjacentToOpponentCellWith3Dots, botTeam);
            return;
        }

        // Variant 2: Check if there is a bot cell with 2 dots and adjacent to an opponent cell with 3 dots
        var botCellWith2DotsAndAdjacentToOpponentCellWith3Dots = GetAdjacentCellWithNDots(botCells, opponentCells, 2);
        if (botCellWith2DotsAndAdjacentToOpponentCellWith3Dots != null)
        {
            ProcessBotCellTurn(botCellWith2DotsAndAdjacentToOpponentCellWith3Dots, botTeam);
            return;
        }

        // Variant 3: Check if there is an opponent cell with 3 dots and attack it if there is a bot cell adjacent to it
        var opponentCellWith3Dots = opponentCells.FirstOrDefault(cell => cell.NumberOfDots == 3);
        if (opponentCellWith3Dots != null)
        {
            var botCellToAttack = botCells.FirstOrDefault(botCell => HasAdjacentCellWithNDots(botCell, opponentCellWith3Dots, 1));
            botCellToAttack ??= botCells.FirstOrDefault(botCell => HasAdjacentCellWithNDots(botCell, opponentCellWith3Dots, 2));
            if (botCellToAttack != null)
            {
                ProcessBotCellTurn(botCellToAttack, botTeam);
                return;
            }
        }

        // Variant 4: Check if there is a bot cell with 2 dots and adjacent to an opponent cell with 2 dots
        var botCellWith2DotsAndAdjacentToOpponentCellWith2Dots = GetAdjacentCellWithNDots(botCells, opponentCells, 2);
        if (botCellWith2DotsAndAdjacentToOpponentCellWith2Dots != null)
        {
            ProcessBotCellTurn(botCellWith2DotsAndAdjacentToOpponentCellWith2Dots, botTeam);
            return;
        }

        // Variant 5: Find the closest bot cell to an opponent cell and attack it
        var closestBotCellToOpponentCell = FindClosestBotCellToOpponentCell(botCells, opponentCells);
        if (closestBotCellToOpponentCell != null)
        {
            ProcessBotCellTurn(closestBotCellToOpponentCell, botTeam);
            return;
        }
    }

    private float GetDistanceBetweenCells(Cell cell1, Cell cell2)
    {
        Vector2 cell1Position = cell1.CellInstance.gameObject.transform.position;
        Vector2 cell2Position = cell2.CellInstance.gameObject.transform.position;
        return Vector2.Distance(cell1Position, cell2Position);
    }

    private Cell FindClosestBotCellToOpponentCell(List<Cell> botCells, List<Cell> opponentCells)
    {
        Cell closestBotCell = null;
        float shortestDistance = float.MaxValue;

        foreach (Cell botCell in botCells)
        {
            foreach (Cell opponentCell in opponentCells)
            {
                float distance = GetDistanceBetweenCells(botCell, opponentCell);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestBotCell = botCell;
                }
            }
        }

        return closestBotCell;
    }

    private bool HasAdjacentCellWithNDots(Cell cell, Cell targetCell, int dotsAmount)
    {
        return (cell.Neighbours.top == targetCell && cell.Neighbours.top?.NumberOfDots == dotsAmount) ||
        (cell.Neighbours.bottom == targetCell && cell.Neighbours.bottom?.NumberOfDots == dotsAmount) ||
        (cell.Neighbours.right == targetCell && cell.Neighbours.right?.NumberOfDots == dotsAmount) ||
        (cell.Neighbours.left == targetCell && cell.Neighbours.left?.NumberOfDots == dotsAmount);
    }

    private Cell GetAdjacentCellWithNDots(List<Cell> botCells, List<Cell> opponentCells, int dots)
    {
        return botCells.FirstOrDefault(botCell =>
            botCell.NumberOfDots == dots &&
            opponentCells.Any(opponentCell =>
                HasAdjacentCellWithNDots(botCell, opponentCell, dots) &&
                opponentCell.NumberOfDots == dots
            )
        );
    }

    private void ProcessBotCellTurn(Cell botCell, Team botTeam)
    {
        GiveTurnIfNotThreeDots(botCell, botTeam);
        botCell.AddDot();
    }

    private void GiveTurnIfNotThreeDots(Cell botCell, Team team)
    {
        _gameManager.Cells.FirstOrDefault(botCell => botCell.CellTeam == team)?.Material.SetInt("_IsGlowing", 0);
        if (botCell.NumberOfDots != 3)
        {
            StartCoroutine(GiveNextTurn(team));
        }
    }

    private IEnumerator GiveNextTurn(Team team)
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame/4);
        EventAggregator.Post(this, new NextTurn { cellTeam = team });
    }
}