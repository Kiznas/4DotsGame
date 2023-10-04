using System.Collections.Generic;
using System.Linq;
using CellLogic;
using UnityEngine;

namespace BotScripts
{
    public static class BotExtensions
    {
        public static Cell GetBotCellToAttack(List<Cell> botCells)
        {
            for (int i = 3; i > 0; i--)
            {
                for (int j = 3; j > 0; j--)
                {
                    var botCell = FindBotCellWithDotsAdjacentToOpponent(botCells,i, j);
                    if (botCell != null)
                        return botCell;
                }
            }
            return null;
        }
        
        private static Cell FindBotCellWithDotsAdjacentToOpponent(List<Cell> botCells, int botDots, int opponentDots)
        {
            foreach (var botCell in botCells)
            {
                if (botCell.NumberOfDots == botDots)
                {
                    var adjacentOpponentCell = botCell.Neighbours.FirstOrDefault(cell => 
                        cell != null &&
                        cell.NumberOfDots == opponentDots && 
                        cell.CellTeam != botCell.CellTeam);
                    
                    if (adjacentOpponentCell != null)
                        return botCell;
                }
            }

            return null;
        }

        public static Cell FindClosestBotCellToOpponentCell(List<Cell> botCells,
            List<Cell> opponentCells)
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

        private static float GetDistanceBetweenCells(Cell cell1, Cell cell2)
        {
            Vector2 cell1Position = cell1.CellInstance.gameObject.transform.position;
            Vector2 cell2Position = cell2.CellInstance.gameObject.transform.position;
            return Vector2.Distance(cell1Position, cell2Position);
        }
    }
}