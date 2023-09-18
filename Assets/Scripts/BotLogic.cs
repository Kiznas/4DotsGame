using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Events;
using Game_Managing;
using UnityEngine;

public class BotLogic : MonoBehaviour
{
    [SerializeField] private GameManage gameManager;
    private List<Enums.Team> _botTeams = new();
    private static readonly int IsGlowing = Shader.PropertyToID("_IsGlowing");

    private void Start()
    {
        EventAggregator.Subscribe<GetTurn>(OnGetTurn);
        EventAggregator.Subscribe<AddBots>(AddBotsToList);
    }

    private void AddBotsToList(object arg1, AddBots data)
    {
        _botTeams = data.Teams;
    }

    private void OnDestroy()
    {
        EventAggregator.Unsubscribe<GetTurn>(OnGetTurn);
        EventAggregator.Unsubscribe<AddBots>(AddBotsToList);
    }

    private void OnGetTurn(object sender, GetTurn turnData)
    {
        foreach (Enums.Team botTeam in _botTeams)
        {
            if (turnData.GameState == Constants.TeamsDictionary[botTeam])
            {
                StartCoroutine(BotDelay(botTeam));
                return;
            }
        }
    }

    private IEnumerator BotDelay(Enums.Team team)
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame);
        TakeBotTurn(team);
    }

    private void TakeBotTurn(Enums.Team botTeam)
    {
        var opponentCells = gameManager.Cells.Where(cell => cell.CellTeam != botTeam && cell.CellTeam != Enums.Team.None).ToList();
        var botCells = gameManager.Cells.Where(cell => cell.CellTeam == botTeam).ToList();

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
        }
    }

    private float GetDistanceBetweenCells(Cell.Cell cell1, Cell.Cell cell2)
    {
        Vector2 cell1Position = cell1.CellInstance.gameObject.transform.position;
        Vector2 cell2Position = cell2.CellInstance.gameObject.transform.position;
        return Vector2.Distance(cell1Position, cell2Position);
    }

    private Cell.Cell FindClosestBotCellToOpponentCell(List<Cell.Cell> botCells, List<Cell.Cell> opponentCells)
    {
        Cell.Cell closestBotCell = null;
        float shortestDistance = float.MaxValue;

        foreach (Cell.Cell botCell in botCells)
        {
            foreach (Cell.Cell opponentCell in opponentCells)
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

    private bool HasAdjacentCellWithNDots(Cell.Cell cell, Cell.Cell targetCell, int dotsAmount)
    {
        return (cell.Neighbours.top == targetCell && cell.Neighbours.top?.NumberOfDots == dotsAmount) ||
        (cell.Neighbours.bottom == targetCell && cell.Neighbours.bottom?.NumberOfDots == dotsAmount) ||
        (cell.Neighbours.right == targetCell && cell.Neighbours.right?.NumberOfDots == dotsAmount) ||
        (cell.Neighbours.left == targetCell && cell.Neighbours.left?.NumberOfDots == dotsAmount);
    }

    private Cell.Cell GetAdjacentCellWithNDots(List<Cell.Cell> botCells, List<Cell.Cell> opponentCells, int dots)
    {
        return botCells.FirstOrDefault(botCell =>
            botCell.NumberOfDots == dots &&
            opponentCells.Any(opponentCell =>
                HasAdjacentCellWithNDots(botCell, opponentCell, dots) &&
                opponentCell.NumberOfDots == dots
            )
        );
    }

    private void ProcessBotCellTurn(Cell.Cell botCell, Enums.Team botTeam)
    {
        gameManager.Cells.FirstOrDefault(cell => cell.CellTeam == botTeam)?.Material.SetInt(IsGlowing, 0);

        if (botCell.NumberOfDots != 3)
        {
            StartCoroutine(GiveNextTurn(botTeam));
        }

        botCell.AddDot();
    }

    private IEnumerator GiveNextTurn(Enums.Team team)
    {
        yield return new WaitForSeconds(Constants.SpeedOfGame);
        EventAggregator.Post(this, new NextTurn { CellTeam = team });
    }
}