using CellLogic;
using System.Linq;
using System.Collections.Generic;
using ConstantValues;
using Events;
using UnityEngine;

namespace Game_Managing.CellsManager
{
    public static class CellManagerExtensions
    {
        internal static HashSet<Enums.Team> CheckAliveTeams(HashSet<Enums.Team> previouslyAliveTeams, Cell[] cells, CellManager cellManager)
        {
            var aliveTeams = new HashSet<Enums.Team>(cells.Select(cell => cell.CellTeam));
            foreach (var lostTeam in previouslyAliveTeams.Except(aliveTeams))
            {
                var playerName = Constants.Player + (int)lostTeam;
                EventAggregator.Post(cellManager, new PlayerLost { PlayerName = playerName });
            }

            return aliveTeams;
        }

        internal static void ProcessCell(Cell cell, Stack<Cell> stackToChange)
        {
            var cells = cell.Neighbours;

            foreach (var item in cells)
            {
                if (item != null && cell.TeamColor != Color.white)
                {
                    item.SetTeam(cell.TeamColor, cell.Material, cell.CellTeam);
                    item.AddDot();
                }

                StackAdd(item, stackToChange);
            }

            cell.ClearCell();
        }

        private static void StackAdd(Cell cell, Stack<Cell> stackToChange)
        {
            if (cell != null)
            {
                var cells = new List<Cell> { cell };
                cells.AddRange(cell.Neighbours.Where(neighbour => neighbour != null));

                foreach (var item in cells.Where(item =>
                             item != null && !stackToChange.Contains(item)))
                    stackToChange.Push(item);
            }
        }
    }
}